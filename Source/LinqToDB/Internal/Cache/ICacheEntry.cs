﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace LinqToDB.Internal.Cache
{
	/// <summary>
	/// Represents an entry in the <see cref="IMemoryCache{TKey,TEntity}"/> implementation.
	/// </summary>
	interface ICacheEntry<TKey,TEntity> : IDisposable
		where TKey : notnull
	{
		/// <summary>
		/// Gets the key of the cache entry.
		/// </summary>
		TKey Key { get; }

		/// <summary>
		/// Gets or set the value of the cache entry.
		/// </summary>
		TEntity? Value { get; set; }

		/// <summary>
		/// Gets or sets an absolute expiration date for the cache entry.
		/// </summary>
		DateTimeOffset? AbsoluteExpiration { get; set; }

		/// <summary>
		/// Gets or sets an absolute expiration time, relative to now.
		/// </summary>
		TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

		/// <summary>
		/// Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed.
		/// This will not extend the entry lifetime beyond the absolute expiration (if set).
		/// </summary>
		TimeSpan? SlidingExpiration { get; set; }

		/// <summary>
		/// Gets the <see cref="IChangeToken"/> instances which cause the cache entry to expire.
		/// </summary>
		IList<IChangeToken> ExpirationTokens { get; }

		/// <summary>
		/// Gets or sets the callbacks will be fired after the cache entry is evicted from the cache.
		/// </summary>
		IList<PostEvictionCallbackRegistration<TKey>> PostEvictionCallbacks { get; }

		/// <summary>
		/// Gets or sets the priority for keeping the cache entry in the cache during a
		///  cleanup. The default is <see cref="CacheItemPriority.Normal"/>.
		/// </summary>
		CacheItemPriority Priority { get; set; }

		/// <summary>
		/// Gets or set the size of the cache entry value.
		/// </summary>
		long? Size { get; set; }
	}
}
