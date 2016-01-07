#region File Header

// ***********************************************************************
// Author	: Sander Struijk
// File		: ICacheObject.cs
// Created	: 2014 05 23 12:31
// Updated	: 2014 05 23 12:32
// ***********************************************************************

#endregion

namespace PersistentObjectCachenet45
{
    /// <summary>   Interface for cache object. </summary>
    /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
    public interface ICacheObject
    {
        /// <summary>   Gets the value. </summary>
        /// <value> The value. </value>
        object Value { get; }

        /// <summary>   Gets a value. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <param name="ignoreInvalidationTime">   true to ignore invalidation time. </param>
        /// <returns>   The value. </returns>
        object GetValue(bool ignoreInvalidationTime = false);
    }
}