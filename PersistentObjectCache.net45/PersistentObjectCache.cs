#region File Header

// // ***********************************************************************
// // Author           : Sander Struijk
// // ***********************************************************************

#endregion


#region Using statements

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PersistentObjectCachenetcore451;

#endregion

// ReSharper disable once CheckNamespace

namespace PersistentObjectCachenet45
{
    /// <summary>   A persistent object cache. </summary>
    /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
    public static class PersistentObjectCache
    {
        /// <summary>   The objects. </summary>
        private static readonly IDictionary<string, ICacheObject> Objects;

        /// <summary>   The default invalidation time. </summary>
        private static TimeSpan _defaultInvalidationTime;

        /// <summary>   Default constructor. </summary>
        /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
        static PersistentObjectCache()
        {
            Objects = new Dictionary<string, ICacheObject>();
            _defaultInvalidationTime = new TimeSpan(0, 5, 0); //5 minutes
        }

        /// <summary>   Sets default invalidation time. </summary>
        /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
        /// <param name="time"> the time. </param>
        public static void SetDefaultInvalidationTime(TimeSpan time)
        {
            _defaultInvalidationTime = time;
        }

        /// <summary>   Gets object asynchronous. </summary>
        /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="key">                      The key. </param>
        /// <param name="ignoreInvalidationTime">   (Optional) true to ignore invalidation time. </param>
        /// <param name="storageType">              (Optional) type of the storage. </param>
        /// <returns>   The object asynchronous. </returns>
        public static async Task<T> GetObjectAsync<T>(string key, bool ignoreInvalidationTime = false, StorageType storageType = StorageType.Local)
        {
            var cacheObject = TryGetValue(key);
            if(cacheObject != null && cacheObject.GetValue(ignoreInvalidationTime) != null) return (T)cacheObject.GetValue(ignoreInvalidationTime);

            try
            {
                var isoCachedObject = await new IsoStorage<CacheObject<T>>(storageType).LoadAsync(key);
                if(isoCachedObject != null && isoCachedObject.GetValue(ignoreInvalidationTime) != null) return (T)isoCachedObject.GetValue(ignoreInvalidationTime);
                if(!Objects.ContainsKey(key)) Objects.Add(key, isoCachedObject);
            }
            catch(InvalidOperationException)
            {
                return default(T);
            }

            return default(T);
        }

        /// <summary>   Sets object asynchronous. </summary>
        /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="key">          The key. </param>
        /// <param name="value">        The value. </param>
        /// <param name="time">         (Optional) the time. </param>
        /// <param name="storageType">  (Optional) type of the storage. </param>
        /// <returns>   A T. </returns>
        public static T SetObjectAsync<T>(string key, T value, TimeSpan? time = null, StorageType storageType = StorageType.Local)
        {
            var cacheObject = new CacheObject<T> {InvalidationTime = time ?? _defaultInvalidationTime, CachedValue = value, CachedDateTime = DateTime.Now};
            new IsoStorage<CacheObject<T>>(storageType).SaveAsync(key, cacheObject);
            if(!Objects.ContainsKey(key)) Objects.Add(key, cacheObject);
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
            new IsoStorage<CacheObject<object>>(storageType).Delete(key);
        }

        /// <summary>   Clears all cache described by storageType. </summary>
        /// <remarks>   Sander.struijk, 12.05.2014. </remarks>
        /// <param name="storageType">  (Optional) </param>
        public static void ClearAllCache(StorageType storageType = StorageType.Local)
        {
            Objects.Clear();
            new IsoStorage<CacheObject<object>>(storageType).DeleteAll();
        }

        /// <summary>   Try get value. </summary>
        /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
        /// <param name="key">  The key. </param>
        /// <returns>   An ICacheObject. </returns>
        private static ICacheObject TryGetValue(string key)
        {
            ICacheObject value;
            Objects.TryGetValue(key, out value);
            return value;
        }
    }

    /// <summary>   Interface for cache object. </summary>
    /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
    public interface ICacheObject
    {
        /// <summary>   Gets the value. </summary>
        /// <value> The value. </value>
        object Value { get; }

        /// <summary>   Gets a value. </summary>
        /// <param name="ignoreInvalidationTime">   true to ignore invalidation time. </param>
        /// <returns>   The value. </returns>
        object GetValue(bool ignoreInvalidationTime);
    }

    /// <summary>   A cache object. </summary>
    /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    [DataContract]
    public class CacheObject<T> : ICacheObject
    {
        /// <summary>   The date time when this object was first cached. </summary>
        /// <value> The cached date time. </value>
        [DataMember]
        public DateTime CachedDateTime { get; set; }

        /// <summary>   The invalidation time. </summary>
        /// <value> The invalidation time. </value>
        [DataMember]
        public TimeSpan InvalidationTime { get; set; }

        /// <summary>   The cached value. </summary>
        /// <value> The cached value. </value>
        [DataMember]
        public T CachedValue { get; set; }

        /// <summary>   Gets the value. </summary>
        /// <value> The value. </value>
        public object Value
        {
            get { return CachedDateTime.Add(InvalidationTime).CompareTo(DateTime.Now) >= 0 ? CachedValue as object : null; }
        }

        /// <summary>   Gets a value. </summary>
        /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
        /// <param name="ignoreInvalidationTime">   (Optional) true to ignore invalidation time. </param>
        /// <returns>   The value. </returns>
        public object GetValue(bool ignoreInvalidationTime = false)
        {
            return ignoreInvalidationTime ? CachedValue : Value;
        }
    }
}