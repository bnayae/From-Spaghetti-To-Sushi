using System;
using System.IO;

namespace ImageProcessing.Contracts
{
    /// <summary>
    /// Creates the first builder of a chain Exposed by the IoC.
    /// </summary>
    public interface IEffectBuilderFactory
    {
        /// <summary>
        /// Creates builder.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        EffectBuilderBase Create(
                        in IEffectParameters parameters);
        /// <summary>
        /// Creates builder.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        EffectBuilderBase Create();
    }
}
