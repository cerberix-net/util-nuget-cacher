using Cerberix.Serialization;
using Moq;
using NUnit.Framework;

namespace Cerberix.Caching.DotNet.Tests
{
    [TestFixture]
    public class DotNetMemoryCacherTests
    {
        //
        //  Clear
        //

        [Test]
        public void ClearWhenCacheEmptyByRegionExpectResult()
        {
            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);
            ICacher cacher = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "ClearWhenCacheEmptyByRegionExpectResult"
                );

            //
            // assert pre-conditions
            //

            Assert.IsNotNull(cacher.GetKeys());
            Assert.AreEqual(expected: 0, actual: cacher.GetKeys().Length);

            //
            //  assert expected conditions
            //

            cacher.Clear();

            Assert.IsNotNull(cacher.GetKeys());
            Assert.AreEqual(expected: 0, actual: cacher.GetKeys().Length);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }

        [Test]
        public void ClearWhenCacheNotEmptyByRegionExpectResult()
        {
            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);
            mockJsonConverter.Setup(m => m.Serialize("bar1")).Returns("bar1").Verifiable();
            mockJsonConverter.Setup(m => m.Serialize("bar2")).Returns("bar2").Verifiable();
            mockJsonConverter.Setup(m => m.Serialize("bar3")).Returns("bar3").Verifiable();

            var mockCacheItemPolicy = new CacherItemPolicy(keepAlive: 30, policyType: CacherItemPolicyType.Sliding);

            ICacher cacher = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "ClearWhenCacheNotEmptyByRegionExpectResult"
                );

            //
            // assert pre-conditions
            //

            cacher.Set(
                cacheItemKey: "foo1",
                cacheItem: "bar1",
                cacheItemPolicy: mockCacheItemPolicy
                );
            cacher.Set(
                cacheItemKey: "foo2",
                cacheItem: "bar2",
                cacheItemPolicy: mockCacheItemPolicy
                );
            cacher.Set(
                cacheItemKey: "foo3",
                cacheItem: "bar3",
                cacheItemPolicy: mockCacheItemPolicy
                );

            Assert.IsNotNull(cacher.GetKeys());
            Assert.AreEqual(expected: 3, actual: cacher.GetKeys().Length);

            //
            //  assert expected conditions
            //

            cacher.Clear();

            Assert.IsNotNull(cacher.GetKeys());
            Assert.AreEqual(expected: 0, actual: cacher.GetKeys().Length);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }

        [Test]
        public void ClearWhenCacheDuelingByRegionExpectResult()
        {
            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);
            mockJsonConverter.Setup(m => m.Serialize("bar")).Returns("bar").Verifiable();
            mockJsonConverter.Setup(m => m.Serialize("baz")).Returns("baz").Verifiable();

            var mockCacheItemPolicy = new CacherItemPolicy(keepAlive: 30, policyType: CacherItemPolicyType.Sliding);

            ICacher cacherAlpha = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "ClearWhenCacheDuelingByRegionExpectResult_A"
                );
            ICacher cacherBeta = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "ClearWhenCacheDuelingByRegionExpectResult_a"
                );

            //
            // assert pre-conditions
            //

            cacherAlpha.Set(
                cacheItemKey: "foo",
                cacheItem: "bar",
                cacheItemPolicy: mockCacheItemPolicy
                );
            cacherBeta.Set(
                cacheItemKey: "foo",
                cacheItem: "baz",
                cacheItemPolicy: mockCacheItemPolicy
                );

            Assert.IsNotNull(cacherAlpha.GetKeys());
            Assert.AreEqual(expected: 1, actual: cacherAlpha.GetKeys().Length);

            Assert.IsNotNull(cacherBeta.GetKeys());
            Assert.AreEqual(expected: 1, actual: cacherBeta.GetKeys().Length);

            //
            //  assert expected conditions
            //

            cacherAlpha.Clear();

            Assert.IsNotNull(cacherAlpha.GetKeys());
            Assert.AreEqual(expected: 0, actual: cacherAlpha.GetKeys().Length);

            Assert.IsNotNull(cacherBeta.GetKeys());
            Assert.AreEqual(expected: 1, actual: cacherBeta.GetKeys().Length);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }

        //
        //  ContainsKey
        //

        [Test]
        public void ContainsKeyWhenCacheItemExistsByRegionExpectTrue()
        {
            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);
            mockJsonConverter.Setup(m => m.Serialize("bar")).Returns("bar").Verifiable();

            var mockCacheItemPolicy = new CacherItemPolicy(keepAlive: 30, policyType: CacherItemPolicyType.Sliding);

            ICacher cacher = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "ContainsKeyWhenCacheItemExistsByRegionExpectTrue"
                );

            //
            // assert pre-conditions
            //

            cacher.Set(
                cacheItemKey: "foo",
                cacheItem: "bar",
                cacheItemPolicy: mockCacheItemPolicy
                );

            Assert.IsNotNull(cacher.GetKeys());
            Assert.AreEqual(expected: 1, actual: cacher.GetKeys().Length);

            //
            //  assert
            //

            var actual = cacher.ContainsKey("foo");

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }

        [Test]
        public void ContainsKeyWhenCacheItemNotExistsByRegionExpectFalse()
        {
            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);
            mockJsonConverter.Setup(m => m.Serialize("bar")).Returns("bar").Verifiable();

            var mockCacheItemPolicy = new CacherItemPolicy(keepAlive: 30, policyType: CacherItemPolicyType.Sliding);

            ICacher cacher = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "ContainsKeyWhenCacheItemNotExistsByRegionExpectFalse"
                );

            //
            // assert pre-conditions
            //

            cacher.Set(
                cacheItemKey: "foo",
                cacheItem: "bar",
                cacheItemPolicy: mockCacheItemPolicy
                );

            Assert.IsNotNull(cacher.GetKeys());
            Assert.AreEqual(expected: 1, actual: cacher.GetKeys().Length);

            //
            //  assert
            //

            var actual = cacher.ContainsKey("f00");

            Assert.IsNotNull(actual);
            Assert.IsFalse(actual);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }

        [Test]
        public void ContainsKeyWhenCacheEmptyByRegionExpectFalse()
        {
            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);

            ICacher cacher = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "ContainsKeyWhenCacheEmptyByRegionExpectFalse"
                );

            //
            //  assert
            //

            var actual = cacher.ContainsKey("foo");

            Assert.IsNotNull(actual);
            Assert.IsFalse(actual);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }

        //
        //  GetOrSet
        //

        [Test]
        public void GetWhenCacheExpiredByRegionExpectResult()
        {
            const int mockCacheTimeSpanIntervalOverride = 3;

            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);
            mockJsonConverter.Setup(m => m.Serialize("bar")).Returns("bar").Verifiable();
            mockJsonConverter.Setup(m => m.Deserialize<string>("bar")).Returns("bar").Verifiable();

            var mockCacheItemPolicy = new CacherItemPolicy(keepAlive: mockCacheTimeSpanIntervalOverride, policyType: CacherItemPolicyType.Sliding);

            ICacher cacher = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "GetWhenCacheExpiredByRegionExpectResult"
                );

            //
            // assert pre-conditions
            //

            cacher.GetOrSet(
                cacheItemKey: "foo",
                cacheItemPolicy: mockCacheItemPolicy,
                getCacheItemFunc: () => { return "bar"; }
                );
            var lookup = cacher.Get<string>("foo");

            Assert.IsNotNull(lookup);
            Assert.AreEqual(expected: "bar", actual: lookup);

            // wait for the cache item to expire
            System.Threading.Thread.Sleep(mockCacheTimeSpanIntervalOverride * 1000);

            //
            //  assert
            //

            var actual = cacher.ContainsKey("foo");

            Assert.IsNotNull(actual);
            Assert.IsFalse(actual);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }

        [Test]
        [TestCase(CacherItemPolicyType.Absolute)]
        [TestCase(CacherItemPolicyType.Sliding)]
        public void SetWhenCacheEmptyByRegionExpectResult(CacherItemPolicyType policyType)
        {
            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);
            mockJsonConverter.Setup(m => m.Serialize("bar")).Returns("bar").Verifiable();
            mockJsonConverter.Setup(m => m.Deserialize<string>("bar")).Returns("bar").Verifiable();

            var mockCacheItemPolicy = new CacherItemPolicy(keepAlive: 30, policyType: policyType);

            ICacher cacher = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: $"SetWhenCacheEmptyByRegionExpectResult_{policyType.ToString()}"
                );

            //
            // assert pre-conditions
            //

            cacher.GetOrSet(
                cacheItemKey: "foo",
                getCacheItemFunc: () => { return "bar"; },
                cacheItemPolicy: mockCacheItemPolicy
                );

            Assert.IsNotNull(cacher.GetKeys());
            Assert.AreEqual(expected: 1, actual: cacher.GetKeys().Length);

            //
            //  assert
            //

            string actual = cacher.Get<string>(
                cacheItemKey: "foo"
                );

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected: "bar", actual: actual);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }

        [Test]
        public void SetWhenCacheUpdateByRegionExpectResult()
        {
            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);
            mockJsonConverter.Setup(m => m.Serialize("bar")).Returns("bar").Verifiable();
            mockJsonConverter.Setup(m => m.Serialize("baz")).Returns("baz").Verifiable();
            mockJsonConverter.Setup(m => m.Deserialize<string>("baz")).Returns("baz").Verifiable();

            var mockCacheItemPolicy = new CacherItemPolicy(keepAlive: 30, policyType: CacherItemPolicyType.Sliding);

            ICacher cacher = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "SetWhenCacheUpdateByRegionExpectResult"
                );

            //
            // assert pre-conditions
            //

            cacher.GetOrSet(
                cacheItemKey: "foo",
                getCacheItemFunc: () => { return "bar"; },
                cacheItemPolicy: mockCacheItemPolicy
                );

            Assert.IsNotNull(cacher.GetKeys());
            Assert.AreEqual(expected: 1, actual: cacher.GetKeys().Length);

            //
            //  assert
            //

            cacher.Set(
                cacheItemKey: "foo",
                cacheItem: "baz",
                cacheItemPolicy: mockCacheItemPolicy
                );

            string actual = cacher.Get<string>(
                cacheItemKey: "foo"
                );

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected: "baz", actual: actual);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }

        [Test]
        public void SetWhenCacheEmptyRemoveByMissingKeyExpectResult()
        {
            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);
  
            ICacher cacher = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "SetWhenCacheEmptyRemoveByMissingKeyExpectResult"
                );

            //
            // assert pre-conditions
            //

            Assert.IsNotNull(cacher.GetKeys());
            Assert.AreEqual(expected: 0, actual: cacher.GetKeys().Length);

            //
            //  assert
            //

            bool actual = cacher.Remove(
                cacheItemKey: "foo"
                );

            Assert.IsNotNull(actual);
            Assert.IsFalse(actual);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }

        [Test]
        public void SetWhenCacheNonEmptyRemoveByMissingKeyExpectResult()
        {
            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);
            mockJsonConverter.Setup(m => m.Serialize("bar")).Returns("bar").Verifiable();

            var mockCacheItemPolicy = new CacherItemPolicy(keepAlive: 30, policyType: CacherItemPolicyType.Sliding);

            ICacher cacher = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "SetWhenCacheNonEmptyRemoveByMissingKeyExpectResult"
                );

            //
            // assert pre-conditions
            //

            cacher.Set(
                cacheItemKey: "foo",
                cacheItem: "bar",
                cacheItemPolicy: mockCacheItemPolicy
                );

            Assert.IsNotNull(cacher.GetKeys());
            Assert.AreEqual(expected: 1, actual: cacher.GetKeys().Length);

            //
            //  assert
            //

            bool actual = cacher.Remove(
                cacheItemKey: "f00"
                );

            Assert.IsNotNull(actual);
            Assert.IsFalse(actual);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }

        [Test]
        public void SetWhenCacheNonEmptyRemoveByBoundKeyExpectResult()
        {
            //
            //  arrange
            //

            var mockJsonConverter = new Mock<IJsonConverter>(MockBehavior.Strict);
            mockJsonConverter.Setup(m => m.Serialize("bar")).Returns("bar").Verifiable();

            var mockCacheItemPolicy = new CacherItemPolicy(keepAlive: 30, policyType: CacherItemPolicyType.Sliding);

            ICacher cacher = new DotNetMemoryCacher(
                jsonConverter: mockJsonConverter.Object,
                regionName: "SetWhenCacheNonEmptyRemoveByBoundKeyExpectResult"
                );

            //
            // assert pre-conditions
            //

            cacher.Set(
                cacheItemKey: "foo",
                cacheItem: "bar",
                cacheItemPolicy: mockCacheItemPolicy
                );

            Assert.IsNotNull(cacher.GetKeys());
            Assert.AreEqual(expected: 1, actual: cacher.GetKeys().Length);

            //
            //  assert
            //

            bool actual = cacher.Remove(
                cacheItemKey: "foo"
                );

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual);

            //
            //  verify dependents
            //

            mockJsonConverter.Verify();
        }
    }
}