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
        public void SaveAsyncTest()
        {
            PersistentObjectCache.SetObjectAsync("Test", new TestObject {Name = "Sander"});
        }

        [TestMethod]
        public void LoadAsyncTest()
        {
            var testObject = PersistentObjectCache.GetObjectAsync<TestObject>("Test").Result;
            testObject.Should().NotBeNull();
            testObject.Name.Should().Be("Sander");
        }

        [TestMethod]
        public void LoadAsyncFileDoesNotExistTest()
        {
            var testObject = PersistentObjectCache.GetObjectAsync<TestObject>("Test2").Result;
            testObject.Should().BeNull();
        }
    }
}
