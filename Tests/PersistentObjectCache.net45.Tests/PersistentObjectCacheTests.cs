using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersistentObjectCachenet45;

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
