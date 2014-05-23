#region File Header

// ***********************************************************************
// Author	: Sander Struijk
// File		: PersistentObjectCacheTests.cs
// Created	: 2014 05 08 10:26
// Updated	: 2014 05 23 12:32
// ***********************************************************************

#endregion

#region Using statements

using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersistentObjectCachenet45;

#endregion

// ReSharper disable once CheckNamespace

namespace PersistentObjectCachenet45Tests
{
    [DataContract]
    public class TestObject
    {
        [DataMember]
        public string Name { get; set; }
    }

    [TestClass]
    public class PersistentObjectCacheTests
    {
        [TestMethod]
        public void SaveAndLoadAndDeleteTest()
        {
            const string key = "Test";

            PersistentObjectCache.SetObjectAsync(key, new TestObject {Name = "Sander"});

            var testObject = PersistentObjectCache.GetObjectAsync<TestObject>(key).Result;
            testObject.Should().NotBeNull();
            testObject.Name.Should().Be("Sander");

            PersistentObjectCache.ClearCache(key);
            testObject = PersistentObjectCache.GetObjectAsync<TestObject>(key).Result;
            testObject.Should().BeNull();
        }

        [TestMethod]
        public void LoadAsyncFileDoesNotExistTest()
        {
            var testObject = PersistentObjectCache.GetObjectAsync<TestObject>("Test2").Result;
            testObject.Should().BeNull();
        }
    }
}