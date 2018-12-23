namespace Cerberix.Caching.Core
{
    public class CacherItemPolicy
    {
        /// <summary>
        ///     The total number of seconds to keep cache item alive based on the policy.
        /// </summary>
        public int KeepAlive { get; }

        /// <summary>
        ///     Specifies the policy type to consider for respective cache item.
        /// </summary>
        public CacherItemPolicyType PolicyType { get; }

        public CacherItemPolicy(
            int keepAlive,
            CacherItemPolicyType policyType
            )
        {
            KeepAlive = keepAlive;
            PolicyType = policyType;
        }
    }
}
