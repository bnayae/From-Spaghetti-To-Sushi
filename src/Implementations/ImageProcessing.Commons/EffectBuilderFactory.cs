using ImageProcessing.Contracts;
using System;
using System.IO;
using System.Linq;

namespace ImageProcessing.Commons
{
    /// <summary>
    /// Creates the first builder of a chain Exposed by the IoC.
    /// </summary>
    public class EffectBuilderFactory: IEffectBuilderFactory
    {
        private readonly IEffectProviderFactory[] _providers;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectBuilderFactory"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public EffectBuilderFactory(params IEffectProviderFactory[] provider)
        {
            _providers = provider;
        }

        #endregion // Ctor

        #region Create

        /// <summary>
        /// Creates the effect builder 
        /// using effect provider which support the effect's parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public EffectBuilderBase Create(in IEffectParameters parameters)
        {
            var builder = EffectBuilder.Create(null, parameters, _providers);
            return builder;
        }

        #endregion // Create
    }
}
