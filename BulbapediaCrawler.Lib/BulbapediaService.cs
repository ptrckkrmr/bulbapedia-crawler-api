// <copyright file="BulbapediaService.cs" company="Patrick Kramer">
// Licensed under the terms of the MIT license. See the LICENSE file in the repository root for the full license text.
// </copyright>

namespace BulbapediaCrawler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using AngleSharp;
    using AngleSharp.Dom;
    using BulbapediaCrawler.Model;

    /// <summary>
    /// Provides operations to retrieve pages from Bulbapedia as <see cref="XDocument"/> instances.
    /// </summary>
    public class BulbapediaService
    {
        private readonly IBrowsingContext browsingContext;
        private readonly Uri baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulbapediaService"/> class
        /// with a default browsing context created from the specified configuration.
        /// </summary>
        /// <param name="config">The configuration to use, or null to use a default configuration.</param>
        /// <param name="baseUri">The base <see cref="Uri"/> to use, not null.</param>
        /// <exception cref="ArgumentNullException">If the <paramref name="baseUri"/> argument is null.</exception>
        public BulbapediaService(IConfiguration config, Uri baseUri)
            : this(BrowsingContext.New(config), baseUri)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulbapediaService"/> class
        /// with the specified browsing context.
        /// </summary>
        /// <param name="context">The browsing context to use, not null.</param>
        /// <param name="baseUri">The base <see cref="Uri"/> to use, not null.</param>
        /// <exception cref="ArgumentNullException">If either argument is null.</exception>
        public BulbapediaService(IBrowsingContext context, Uri baseUri)
        {
            this.browsingContext = context ?? throw new ArgumentNullException(nameof(context));
            this.baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        }

        /// <summary>
        /// Gets the raw HTML page from the specified relative path as an <see cref="IDocument"/>.
        /// </summary>
        /// <param name="path">The relative path for the page.</param>
        /// <returns>The <see cref="IDocument"/> parsed from the path.</returns>
        /// <exception cref="ArgumentNullException">If the argument is null.</exception>
        public Task<IDocument> GetPage(string path) => this.browsingContext.OpenAsync(this.baseUri.AbsoluteUri + path);

        /// <summary>
        /// Gets an enumeration of all pokemon available in the national Pokedex, according to Bulbapedia.
        /// </summary>
        /// <returns>The enumeration of all available pokemon, asynchronously.</returns>
        public async Task<IEnumerable<PokemonReference>> GetAllPokemon()
        {
            IDocument document = await this.GetPage("Ndex").ConfigureAwait(false);

            return document.QuerySelectorAll("#bodyContent table:not(:first-child) tr")
                .Where(row => row.QuerySelector("td:nth-child(4)") != null)
                .Where(row => Regex.IsMatch(row.QuerySelector("td:nth-child(2)").TextContent, @"^\s*#\d+\s*$"))
                .Select(row => new PokemonReference
                {
                    Number = Convert.ToInt32(row.QuerySelector("td:nth-child(2)").Text().Trim().TrimStart('#')),
                    Name = row.QuerySelector("td:nth-child(4)").TextContent.Trim(),
                })
                .Distinct()
                .OrderBy(reference => reference.Number);
        }

        public async Task<PokemonDetails> GetDetails(PokemonReference reference)
        {
            IDocument document = await this.GetPage(reference.Name + "_(Pokémon)").ConfigureAwait(false);

            // This is the table on the left of the page with the general info of the Pokemon.
            IElement infoTable = document.QuerySelectorAll("table + table.roundy")
                .Where(table => table.QuerySelector("table table > tbody > tr:first-child > td:first-child > big > big > b") != null)
                .Single();

            int catchRate = infoTable.QuerySelectorAll("td")
                .Where(td => td.QuerySelector("b + table") != null)
                .Where(td => td.QuerySelector("b").TextContent == "Catch rate")
                .Select(td => td.QuerySelector("table td").FirstChild as IText)
                .Where(textNode => textNode != null)
                .Select(textNode => Convert.ToInt32(textNode.Data.Trim()))
                .FirstOrDefault();

            return new PokemonDetails
            {
                Number = reference.Number,
                Name = reference.Name,
                CatchRate = catchRate,
            };
        }
    }
}
