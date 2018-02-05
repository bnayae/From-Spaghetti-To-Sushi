using System;
using System.Threading.Tasks;

namespace Setting.Contracts
{
    /// <summary>
    /// Setting contract
    /// </summary>
    public interface ISetting
    {
        /// <summary>
        /// Gets the setting by key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key);
    }
}
