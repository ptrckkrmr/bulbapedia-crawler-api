// <copyright file="PokemonReference.cs" company="Patrick Kramer">
// Licensed under the terms of the MIT license. See the LICENSE file in the repository root for the full license text.
// </copyright>

namespace BulbapediaCrawler.Model
{
    /// <summary>
    /// A simple reference to a Pokemon, without additional data.
    /// </summary>
    public class PokemonReference
    {
        /// <summary>
        /// Gets or sets the number of the Pokemon in the national Pokedex.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the canonical (English) name of the Pokemon.
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return this.Number == (obj as PokemonReference).Number;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Number.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "#" + this.Number + " - " + this.Name;
        }
    }
}
