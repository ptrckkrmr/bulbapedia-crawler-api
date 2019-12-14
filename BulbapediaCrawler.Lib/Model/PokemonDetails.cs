// <copyright file="PokemonDetails.cs" company="Patrick Kramer">
// Licensed under the terms of the MIT license. See the LICENSE file in the repository root for the full license text.
// </copyright>

namespace BulbapediaCrawler.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Detailed information about a Pokemon.
    /// </summary>
    public class PokemonDetails : PokemonReference
    {
        /// <summary>
        /// Gets or sets the Pokemon's short description as stated on Bulbapedia.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the types of this Pokemon.
        /// </summary>
        /// <remarks>
        /// A Pokemon typically has either one or two types.
        /// </remarks>
        public IEnumerable<string> Types { get; set; }

        /// <summary>
        /// Gets or sets the catch rate of this Pokemon.
        /// </summary>
        public int CatchRate { get; set; }

        /// <summary>
        /// Gets or sets the base experience yield of this Pokemon.
        /// </summary>
        public int BaseExperienceYield { get; set; }

        /// <summary>
        /// Gets or sets the minimum amount of steps needed to hatch this Pokemon.
        /// </summary>
        public int HatchTimeMin { get; set; }

        /// <summary>
        /// Gets or sets the maximum amount of steps needed to hatch this Pokemon.
        /// </summary>
        public int HatchTimeMax { get; set; }

        /// <summary>
        /// Gets or sets the base friendship level for this Pokemon.
        /// </summary>
        public int BaseFriendship { get; set; }
    }
}
