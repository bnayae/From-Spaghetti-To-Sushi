using System;
using System.Collections.Generic;
using System.Text;

namespace ImageProcessing.Contracts
{
    /// <summary>
    /// Setting for the provider
    /// </summary>
    public class ProviderSetting
    {
        public const string Key = "ImageProcessing.Providers.Setting";

        #region Ctor

        public ProviderSetting(bool optimized)
        {
            Optimized = optimized;
        }

        #endregion // Ctor

        #region Optimized

        /// <summary>
        /// Indicating whether to optimize.
        /// </summary>
        public bool Optimized { get; }

        #endregion // Optimized
    }
}
