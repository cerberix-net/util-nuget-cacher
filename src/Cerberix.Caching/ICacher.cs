using System;

namespace Cerberix.Caching
{
    public interface ICacher
    {
        void Clear();
        bool ContainsKey(string cacheItemKey);
        T Get<T>(string cacheItemKey);
        T GetOrSet<T>(string cacheItemKey, CacherItemPolicy cacheItemPolicy, Func<T> getCacheItemFunc);
        string[] GetKeys();
        T Set<T>(string cacheItemKey, CacherItemPolicy cacheItemPolicy, T cacheItem);
        bool Remove(string cacheItemKey);
    }
}
