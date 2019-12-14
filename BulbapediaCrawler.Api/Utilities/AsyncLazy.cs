// <copyright file="AsyncLazy.cs" company="Patrick Kramer">
// Licensed under the terms of the MIT license. See the LICENSE file in the repository root for the full license text.
// </copyright>

namespace BulbapediaCrawler.Api.Utilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A light-weight, asynchronous variant of a <see cref="Lazy{T}"/> value.
    /// </summary>
    /// <typeparam name="T">The type of element stored in this lazy-loaded value.</typeparam>
    public struct AsyncLazy<T> : IDisposable
        where T : class
    {
        private readonly Func<CancellationToken, Task<T>> supplier;
        private readonly SemaphoreSlim semafore;
        private volatile T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> struct
        /// that runs the provided task asynchronously when first requested.
        /// </summary>
        /// <param name="supplier">A synchronous function that will be asynchronously executed.</param>
        public AsyncLazy(Func<T> supplier)
            : this(token => Task.Run(supplier))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> struct
        /// that runs the provided asynchronous function when first requested.
        /// </summary>
        /// <param name="supplier">The supplier that provides the value for this instance.</param>
        public AsyncLazy(Func<Task<T>> supplier)
            : this(token => supplier())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> struct
        /// that runs the provided asynchronous function when first requested.
        /// </summary>
        /// <param name="supplier">The function that accepts a <see cref="CancellationToken"/> and provides the value for this instance.</param>
        public AsyncLazy(Func<CancellationToken, Task<T>> supplier)
        {
            this.supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
            this.semafore = new SemaphoreSlim(1);
            this.value = null;
        }

        /// <summary>
        /// Gets the lazy-loaded value, loading it lazily if it was not yet loaded.
        /// </summary>
        /// <returns>A task that produces the value.</returns>
        public Task<T> GetAsync() => this.GetAsync(CancellationToken.None);

        /// <summary>
        /// Gets the lazy-loaded value, loading it lazily if it was not yet loaded.
        /// </summary>
        /// <param name="token">The cancellation token for the async task.</param>
        /// <returns>A task that produces the value.</returns>
        public async Task<T> GetAsync(CancellationToken token)
        {
            T result = this.value;
            if (result == null)
            {
                await this.semafore.WaitAsync(token);
                try
                {
                    result = this.value;
                    if (result == null)
                    {
                        result = await this.supplier(token);
                        this.value = result;
                    }
                }
                finally
                {
                    this.semafore.Release();
                }
            }

            token.ThrowIfCancellationRequested();
            return result;
        }

        /// <summary>
        /// Clears the value, if it is present.
        /// </summary>
        /// <returns>True if the value was loaded and has been cleared, false otherwise.</returns>
        public Task<bool> Clear() => this.Clear(CancellationToken.None);

        /// <summary>
        /// Clears the value, if it is present.
        /// </summary>
        /// <param name="token">The cancellation token for the async task.</param>
        /// <returns>True if the value was loaded and has been cleared, false otherwise.</returns>
        public async Task<bool> Clear(CancellationToken token)
        {
            if (this.value == null)
            {
                return false;
            }

            await this.semafore.WaitAsync(token);
            try
            {
                bool result = this.value != null;
                this.value = null;
                return result;
            }
            finally
            {
                this.semafore.Release();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.semafore.Dispose();
        }
    }
}
