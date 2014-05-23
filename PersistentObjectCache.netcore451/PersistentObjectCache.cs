#region File Header

// ***********************************************************************
// Author	: Sander Struijk
// File		: PersistentObjectCache.cs
// Created	: 2014 04 25 14:43
// Updated	: 2014 05 23 12:32
// ***********************************************************************

#endregion

#region Using statements

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace PersistentObjectCachenetcore451
{
    /// <summary>   A persistent object cache. </summary>
    /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
    public static class PersistentObjectCache
    {
        /// <summary>   The objects. </summary>
        private static readonly IDictionary<string, ICacheObject> Objects;

        /// <summary>   The default invalidation time. </summary>
        private static TimeSpan _defaultInvalidationTime;

        /// <summary>   Default constructor. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        static PersistentObjectCache()
        {
            Objects = new Dictionary<string, ICacheObject>();
            _defaultInvalidationTime = new TimeSpan(0, 5, 0); //5 minutes
        }

        /// <summary>   Sets default invalidation time. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <param name="time"> The time. </param>
        public static void SetDefaultInvalidationTime(TimeSpan time)
        {
            _defaultInvalidationTime = time;
        }

        /// <summary>   Gets object asynchronous. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="key">                      The key. </param>
        /// <param name="ignoreInvalidationTime">   (Optional) true to ignore invalidation time. </param>
        /// <param name="storageType">              (Optional) </param>
        /// <returns>   The object asynchronous. </returns>
        public static async Task<T> GetObjectAsync<T>(string key, bool ignoreInvalidationTime = false, StorageType storageType = StorageType.Local)
        {
            var cacheObject = TryGetValue(key);
            if (cacheObject != null && cacheObject.GetValue(ignoreInvalidationTime) != null) return (T)cacheObject.GetValue(ignoreInvalidationTime);

            try
            {
                var isoCachedObject = await new IsoStorage<CacheObject<T>>(storageType).LoadAsync(key);
                if (isoCachedObject != null && isoCachedObject.GetValue(ignoreInvalidationTime) != null) return (T)isoCachedObject.GetValue(ignoreInvalidationTime);
                if (!Objects.ContainsKey(key)) Objects.Add(key, isoCachedObject);
            }
            catch (InvalidOperationException)
            {
                return default(T);
            }

            return default(T);
        }

        /// <summary>   Returns if the cache contains the object associated with the provided key. </summary>
        /// <remarks>   Sander.struijk, 23.05.2014. </remarks>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="key">          The key. </param>
        /// <param name="storageType">  (Optional) </param>
        /// <returns>   A Task&lt;bool&gt; </returns>
        public static async Task<bool> ContainsAsync<T>(string key, StorageType storageType = StorageType.Local)
        {
            var cacheObject = TryGetValue(key);
            if(cacheObject != null) return true;

            try
            {
                var isoCachedObject = await new IsoStorage<CacheObject<T>>(storageType).LoadAsync(key);
                if(isoCachedObject != null && isoCachedObject.GetValue(true) != null)
                {
                    if (!Objects.ContainsKey(key)) 
                        Objects.Add(key, isoCachedObject);
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return false;
        }

        /// <summary>   Returns if the cached object associated with the provided key has expired. </summary>
        /// <remarks>   Sander.struijk, 23.05.2014. </remarks>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="key">          The key. </param>
        /// <param name="storageType">  (Optional) </param>
        /// <returns>   A Task&lt;bool&gt; </returns>
        public static async Task<bool> HasExpired<T>(string key, StorageType storageType = StorageType.Local)
        {
            var cacheObject = TryGetValue(key);
            if(cacheObject != null && cacheObject.GetValue() != null) return false;

            try
            {
                var isoCachedObject = await new IsoStorage<CacheObject<T>>(storageType).LoadAsync(key);
                if (isoCachedObject != null && isoCachedObject.GetValue() != null)
                {
                    if (!Objects.ContainsKey(key))
                        Objects.Add(key, isoCachedObject);
                    return false;
                }
            }
            catch (InvalidOperationException)
            {
                return true;
            }

            return true;
        }

        /// <summary>   Sets object asynchronous. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="key">          The key. </param>
        /// <param name="value">        The value. </param>
        /// <param name="time">         (Optional) The time. </param>
        /// <param name="storageType">  (Optional) </param>
        /// <returns>   A T. </returns>
        public static T SetObjectAsync<T>(string key, T value, TimeSpan? time = null, StorageType storageType = StorageType.Local)
        {
            var cacheObject = new CacheObject<T> { InvalidationTime = time ?? _defaultInvalidationTime, CachedValue = value, CachedDateTime = DateTime.Now };
            new IsoStorage<CacheObject<T>>(storageType).SaveAsync(key, cacheObject);
            if (!Objects.ContainsKey(key)) Objects.Add(key, cacheObject);
            return value;
        }

        /// <summary>   Clears the cache described by key. </summary>
        /// <remarks>   Sander.struijk, 12.05.2014. </remarks>
        /// <param name="key">          The key. </param>
        /// <param name="storageType">  (Optional) </param>
        public static void ClearCache(string key, StorageType storageType = StorageType.Local)
        {
            var cacheObject = TryGetValue(key);
            if (cacheObject != null) Objects.Remove(key);
            new IsoStorage<CacheObject<object>>(storageType).DeleteAsync(key);
        }

        /// <summary>   Clears all cache described by storageType. </summary>
        /// <remarks>   Sander.struijk, 12.05.2014. </remarks>
        /// <param name="storageType">  (Optional) </param>
        public static void ClearAllCache(StorageType storageType = StorageType.Local)
        {
            Objects.Clear();
            new IsoStorage<CacheObject<object>>(storageType).DeleteAllAsync();
        }

        /// <summary>   Try get value. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <param name="key">  The key. </param>
        /// <returns>   An ICacheObject. </returns>
        private static ICacheObject TryGetValue(string key)
        {
            ICacheObject value;
            Objects.TryGetValue(key, out value);
            return value;
        }
    }
}