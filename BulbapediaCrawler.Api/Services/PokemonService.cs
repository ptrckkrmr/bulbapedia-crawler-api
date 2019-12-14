// <copyright file="PokemonService.cs" company="Patrick Kramer">
// Licensed under the terms of the MIT license. See the LICENSE file in the repository root for the full license text.
// </copyright>

namespace BulbapediaCrawler.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BulbapediaCrawler.Api.Utilities;
    using BulbapediaCrawler.Model;

    /// <summary>
    /// Provides access to Pokemon data through Bulbapedia.
    /// </summary>
    public class PokemonService
    {
        private readonly BulbapediaService bulbapedia;
        private readonly AsyncLazy<IEnumerable<PokemonReference>> pokemonReferences;

        /// <summary>
        /// Initializes a new instance of the <see cref="PokemonService"/> class.
        /// </summary>
        /// <param name="bulbapedia">The service to use to load data from Bulbapedia.</param>
        public PokemonService(BulbapediaService bulbapedia)
        {
            this.bulbapedia = bulbapedia ?? throw new ArgumentNullException(nameof(bulbapedia));
            this.pokemonReferences = new AsyncLazy<IEnumerable<PokemonReference>>(
                async () => (await this.bulbapedia.GetAllPokemon()).ToList());
        }

        /// <summary>
        /// Gets all Pokemon ids.
        /// </summary>
        /// <returns>An enumeration of all Pokemon ids.</returns>
        public async Task<IEnumerable<int>> GetIds() => (await this.GetReferences()).Select(p => p.Number);

        /// <summary>
        /// Gets the Pokemon reference with the given id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The reference with the given id, or null if no such reference exists.</returns>
        public async Task<PokemonReference> GetReference(int id) => (await this.GetReferences()).SingleOrDefault(p => p.Number == id);

        /// <summary>
        /// Gets an enumeration of all Pokemon references.
        /// </summary>
        /// <returns>An enumeration of all Pokemon references.</returns>
        public Task<IEnumerable<PokemonReference>> GetReferences() => this.pokemonReferences.GetAsync();

        /// <summary>
        /// Clears cached data from this service.
        /// </summary>
        /// <returns>The result of this asynchronous task.</returns>
        public Task Clear() => this.pokemonReferences.Clear();
    }
}
