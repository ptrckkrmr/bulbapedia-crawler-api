// <copyright file="PokemonDetails.cs" company="Patrick Kramer">
// Licensed under the terms of the MIT license. See the LICENSE file in the repository root for the full license text.
// </copyright>

namespace BulbapediaCrawler.Model
{
    /// <summary>
    /// Detailed information about a Pokemon.
    /// </summary>
    public class PokemonDetails : PokemonReference
    {
        /// <summary>
        /// Gets or sets the catch rate of this Pokemon.
        /// </summary>
        public int CatchRate { get; set; }
    }
}
