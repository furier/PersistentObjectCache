#region File Header

// ***********************************************************************
// Author	: Sander Struijk
// File		: CacheObject.cs
// Created	: 2014 05 23 12:31
// Updated	: 2014 05 23 12:32
// ***********************************************************************

#endregion

#region Using statements

using System;
using System.Runtime.Serialization;

#endregion

namespace PersistentObjectCachenetcore451
{
    /// <summary>   A cache object. </summary>
    /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
    [DataContract]
    public class CacheObject<T> : ICacheObject
    {
        /// <summary>   The date time when this object was first cached. </summary>
        [DataMember]
        public DateTime CachedDateTime { get; set; }

        /// <summary>   The invalidation time. </summary>
        [DataMember]
        public TimeSpan InvalidationTime { get; set; }

        /// <summary>   The cached value. </summary>
        [DataMember]
        public T CachedValue { get; set; }

        /// <summary>   Gets the value. </summary>
        /// <value> The value. </value>
        public object Value
        {
            get { return CachedDateTime.Add(InvalidationTime).CompareTo(DateTime.Now) >= 0 ? CachedValue as object : null; }
        }

        /// <summary>   Gets a value. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <param name="ignoreInvalidationTime">   (Optional) true to ignore invalidation time. </param>
        /// <returns>   The value. </returns>
        public object GetValue(bool ignoreInvalidationTime = false)
        {
            return ignoreInvalidationTime ? CachedValue : Value;
        }
    }
}