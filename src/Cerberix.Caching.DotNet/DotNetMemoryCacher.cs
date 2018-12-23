using System;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using Cerberix.Caching.Core;
using Cerberix.Serialization.Core;

[assembly: InternalsVisibleTo("Cerberix.Caching.DotNet.Tests")]

namespace Cerberix.Caching.DotNet
{
    public class DotNetMemoryCacher : ICacher
    {
        private static readonly MemoryCache _cache;

        private readonly IJsonConverter _jsonConverter;
        private readonly string _regionName;

        static DotNetMemoryCacher()
        {
            _cache = MemoryCache.Default;
        }

        public DotNetMemoryCacher(
            IJsonConverter jsonConverter,
            string regionName
            )
        {
            _jsonConverter = jsonConverter;
            _regionName = regionName;
        }

        //
        //  PUBLIC
        //

        public void Clear()
        {
            var regionKeys = GetKeys()
                .Select(k => GetRegionKeyCore(
                    cacheItemKey: k,
                    regionName: _regionName)
                 ).ToArray();

            lock (this)
            {
                foreach (var regionKey in regionKeys)
                {
                    RemoveCore(regionKey: regionKey);
                }
            }
        }

        public T GetOrSet<T>(string cacheItemKey, CacherItemPolicy cacheItemPolicy, Func<T> getCacheItemFunc)
        {
            if (ContainsKey(cacheItemKey: cacheItemKey))
            {
                return Get<T>(cacheItemKey: cacheItemKey);
            }

            var cacheItem = getCacheItemFunc.Invoke();
            return Set(
                cacheItemKey: cacheItemKey,
                cacheItemPolicy: cacheItemPolicy,
                cacheItem: cacheItem
                );
        }

        public bool ContainsKey(string cacheItemKey)
        {
            var regionKey = GetRegionKeyCore(
                cacheItemKey: cacheItemKey,
                regionName: _regionName
            );

            return ContainsKeyCore(regionKey: regionKey);
        }

        public T Get<T>(string cacheItemKey)
        {
            var regionKey = GetRegionKeyCore(
                cacheItemKey: cacheItemKey,
                regionName: _regionName
            );
            var getCacheObject = GetCore(regionKey: regionKey);
            var getCacheItem = _jsonConverter.Deserialize<T>((string)getCacheObject);
            return getCacheItem;
        }

        public T Set<T>(string cacheItemKey, CacherItemPolicy cacheItemPolicy, T cacheItem)
        {
            var regionKey = GetRegionKeyCore(
                cacheItemKey: cacheItemKey,
                regionName: _regionName
            );
            var putCacheItem = _jsonConverter.Serialize(
                value: cacheItem
                );
            var putCacheItemPolicy = GetCacheItemPolicy(cacheItemPolicy);

            lock (this)
            {
                SetCore(
                    regionKey: regionKey,
                    cacheItem: putCacheItem,
                    cacheItemPolicy: putCacheItemPolicy
                    );
            }

            return cacheItem;
        }

        public bool Remove(string cacheItemKey)
        {
            if (!ContainsKey(cacheItemKey: cacheItemKey))
            {
                return false;
            }

            lock (this)
            {
                var regionKey = GetRegionKeyCore(
                    cacheItemKey: cacheItemKey,
                    regionName: _regionName
                );
                RemoveCore(regionKey: regionKey);
            }

            return true;
        }

        public string[] GetKeys()
        {
            var regionSegment = GetRegionKeyOrDefaultCore(regionName: _regionName);

            return GetKeysCore(regionName: _regionName)
                .Select(rk => rk.Replace(regionSegment, string.Empty))
                .ToArray();
        }

        //
        //  PRIVATE
        //

        private static bool ContainsKeyCore(string regionKey)
        {
            return _cache.Contains(
                key: regionKey,
                regionName: null
                );
        }

        private static object GetCore(string regionKey)
        {
            var getCacheObject = _cache.Get(
                key: regionKey,
                regionName: null
                );
            return getCacheObject;
        }

        private static string[] GetKeysCore(string regionName)
        {
            return _cache
                .Where(mc => mc.Key.StartsWith(regionName))
                .Select(mc => mc.Key)
                .ToArray();
        }

        private static string GetRegionKeyCore(string cacheItemKey, string regionName)
        {
            if (cacheItemKey == null)
            {
                throw new ArgumentNullException("cacheItemKey");
            }

            var cleanRegionName = GetRegionKeyOrDefaultCore(regionName: regionName);
            return $"{cleanRegionName}{cacheItemKey}";
        }

        private static string GetRegionKeyOrDefaultCore(string regionName)
        {
            var regionSegment = !string.IsNullOrWhiteSpace(regionName)
                ? $"{regionName.Trim()}|"
                : $"Default|";
            return regionSegment;
        }

        private static CacheItemPolicy GetCacheItemPolicy(
            CacherItemPolicy cacherItemPolicy
            )
        {
            if (cacherItemPolicy == null)
            {
                throw new ArgumentNullException("cacherItemPolicy");
            }

            CacheItemPolicy policy;
            if (cacherItemPolicy.PolicyType == CacherItemPolicyType.Absolute)
            {
                policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(cacherItemPolicy.KeepAlive)
                };
            }
            else if (cacherItemPolicy.PolicyType == CacherItemPolicyType.Sliding)
            {
                policy = new CacheItemPolicy
                {
                    SlidingExpiration = TimeSpan.FromSeconds(cacherItemPolicy.KeepAlive)
                };
            }
            else
            {
                throw new NotImplementedException("Given CacherItemPolicyType is not implemented.");
            }

            return policy;
        }

        private static void RemoveCore(string regionKey)
        {
            _cache.Remove(
                    key: regionKey,
                    regionName: null
                    );
        }

        private static void SetCore(string regionKey, string cacheItem, CacheItemPolicy cacheItemPolicy)
        {
            _cache.Set(
                    key: regionKey,
                    value: cacheItem,
                    policy: cacheItemPolicy,
                    regionName: null
                    );
        }
    }
}
