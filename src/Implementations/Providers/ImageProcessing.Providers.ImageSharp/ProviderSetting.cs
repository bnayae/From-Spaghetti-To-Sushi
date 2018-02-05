using System;
using System.Collections.Generic;
using System.Text;

namespace ImageProcessing.Providers
{
    /// <summary>
    /// Setting for the provider
    /// </summary>
    internal class ProviderSetting
    {
        public const string Key = "ImageProcessing.Providers.ImageSharp.Setting";

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
