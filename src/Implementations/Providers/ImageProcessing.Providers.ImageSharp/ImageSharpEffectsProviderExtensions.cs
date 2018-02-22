using ImageProcessing.Providers;
using ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageProcessing.Contracts
{
    /// <summary>
    /// Extend friendly api for provider specific effects or effect's parameters
    /// </summary>
    public static class ImageSharpEffectsProviderExtensions
    {
        #region Grayscale

        /// <summary>
        /// Extend the Grayscale effect options.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        public static EffectBuilderBase Grayscale(
            this EffectBuilderBase instance,
            GrayscaleMode mode)
        {
            var parameters = new GrayscaleModeEffectParameters(mode);
            return instance.Append(parameters);
        }

        #endregion // Grayscale
    }
}
