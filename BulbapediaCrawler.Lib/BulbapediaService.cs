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
    using AngleSharp;
    using AngleSharp.Dom;
    using BulbapediaCrawler.Model;

    /// <summary>
    /// Provides operations to retrieve pages from Bulbapedia as HTML documents.
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

        /// <summary>
        /// Gets detailed information about the provided Pokemon.
        /// </summary>
        /// <param name="reference">The Pokemon reference.</param>
        /// <returns>The detailed information about the Pokemon.</returns>
        public async Task<PokemonDetails> GetDetails(PokemonReference reference)
        {
            IDocument document = await this.GetPage(reference.Name + "_(Pokémon)").ConfigureAwait(false);

            // The layout of the body content is as follows:
            // - Table with the top navigation bar (to navigate between Pokemon species)
            // - Table with Pokemon information (right side panel)
            // - One or more paragraph tags with the leading introduction of the Pokemon (this is what we want).
            // - A div element that encapsulates the table of contents.
            // Note that HTML tag names are always upper case (https://developer.mozilla.org/en-US/docs/Web/API/Element/tagName)
            string description = string.Join("\n", document.GetElementById("mw-content-text").Children
                .SkipWhile(e => e.TagName == "TABLE")
                .TakeWhile(e => e.TagName == "P")
                .Select(e => e.TextContent.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line)));

            // This is the table on the left of the page with the general info of the Pokemon.
            IElement infoTable = document.QuerySelectorAll("table + table.roundy")
                .Single(table => table.QuerySelector("table table > tbody > tr:first-child > td:first-child > big > big > b") != null);

            string catchRate = this.GetPokemonInfoPanelValue(infoTable, "Catch rate");
            string baseExpYield = this.GetPokemonInfoPanelValue(infoTable, "Base experience yield");
            string hatchTimeDescription = this.GetPokemonInfoPanelValue(infoTable, "Hatch time");
            string baseFriendship = this.GetPokemonInfoPanelValue(infoTable, "Base friendship");

            int[] hatchTime = hatchTimeDescription == null
                ? new[] { 0 }
                : hatchTimeDescription.Split('-').Select(s => this.TryParseInt32(s, 0)).ToArray();

            return new PokemonDetails
            {
                Number = reference.Number,
                Name = reference.Name,
                Description = description,
                Types = this.GetTypes(infoTable),
                CatchRate = this.TryParseInt32(catchRate, 0),
                BaseExperienceYield = this.TryParseInt32(baseExpYield, 0),
                HatchTimeMin = hatchTime[0],
                HatchTimeMax = hatchTime.Length > 1 ? hatchTime[1] : hatchTime[0],
                BaseFriendship = this.TryParseInt32(baseFriendship, 0),
            };
        }

        /// <summary>
        /// Gets a value from a Pokemon information panel.
        /// </summary>
        /// <param name="infoTable">The information table element to read from.</param>
        /// <param name="name">The name of the value to retrieve, case-insensitive.</param>
        /// <returns>The string value for the provided name, or null if no such value could be found.</returns>
        private string GetPokemonInfoPanelValue(IElement infoTable, string name)
        {
            return infoTable.QuerySelectorAll("td")
                .Where(td => td.QuerySelector("b + table") != null)
                .Where(td => td.QuerySelector("b").TextContent.Trim().Equals(name, StringComparison.InvariantCultureIgnoreCase))
                .Take(1)
                .SelectMany(td => td.QuerySelectorAll("table td"))
                .Select(td => td.FirstChild as IText)
                .Where(textNode => textNode != null)
                .Select(textNode => textNode.Data.Trim())
                .FirstOrDefault(textNode => !textNode.StartsWith("unknown", StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Gets the types for the Pokemon from the given info table element.
        /// </summary>
        /// <param name="infoTable">The info table element.</param>
        /// <returns>The types of the Pokemon.</returns>
        private string[] GetTypes(IElement infoTable)
        {
            return infoTable.QuerySelector("tbody").Children
                .Where(tr => tr.Children.Count() == 1)
                .Select(tr => tr.Children[0])
                .Where(td => td.Children[0].TextContent.Trim().StartsWith("type", StringComparison.InvariantCultureIgnoreCase))
                .Take(1)
                .SelectMany(typesCell => typesCell.QuerySelectorAll("table > tbody > tr > td:first-child > table > tbody > tr > td"))
                .Select(typeCell => typeCell.TextContent.Trim())
                .Where(type => !type.StartsWith("unknown", StringComparison.InvariantCultureIgnoreCase))
                .Where(type => !type.Contains('\n'))
                .ToArray();
        }

        private int TryParseInt32(string s, int defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToInt32(s.Trim());
            }
            catch (FormatException)
            {
                return defaultValue;
            }
        }
    }
}
