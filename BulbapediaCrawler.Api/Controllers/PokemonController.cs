// <copyright file="PokemonController.cs" company="Patrick Kramer">
// Licensed under the terms of the MIT license. See the LICENSE file in the repository root for the full license text.
// </copyright>

namespace BulbapediaCrawler.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BulbapediaCrawler.Api.Services;
    using BulbapediaCrawler.Model;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// API controller that provides access to Pokemon data.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PokemonController : ControllerBase
    {
        private readonly PokemonService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="PokemonController"/> class.
        /// </summary>
        /// <param name="service">The Pokemon service instance to use, not null.</param>
        public PokemonController(PokemonService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Gets a list of all Pokemon ids.
        /// </summary>
        /// <returns>All Pokemon ids.</returns>
        [HttpGet]
        public Task<IEnumerable<int>> Get()
        {
            return this.service.GetIds();
        }

        /// <summary>
        /// Gets a list of all Pokemon references.
        /// </summary>
        /// <returns>All Pokemon references.</returns>
        [HttpGet("references")]
        public Task<IEnumerable<PokemonReference>> GetReferences()
        {
            return this.service.GetReferences();
        }

        /// <summary>
        /// Gets the Pokemon reference with the given id.
        /// </summary>
        /// <param name="id">The Pokemon id.</param>
        /// <returns>The Pokemon reference with the given id.</returns>
        [HttpGet("{id}")]
        public Task<PokemonReference> Get(int id)
        {
            return this.service.GetReference(id);
        }
    }
}
