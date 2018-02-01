using ImageProcessing.Contracts;
using System;
using System.IO;
using System.Linq;

namespace ImageProcessing.Commons
{
    /// <summary>
    /// Represent unit in the effect pipeline.
    /// A chain of one or more EffectBuilder assemble effect pipeline.
    /// Each EffectBuilder can be used to append the next builder unit (IEffectBuilder).
    /// The creation of the first IEffectBuilder unit is done via IEffectBuilderFactory.
    /// </summary>
    /// <seealso cref="ImageProcessing.Contracts.EffectBuilderBase" />
    public class EffectBuilder : EffectBuilderBase
    {
        private readonly IEffectParameters _parameters;
        private readonly IEffectPipelineExecuter _executor;
        private readonly IEffectProviderFactory[] _providers;

        #region Ctor

        /// <summary>
        /// Initializes a new instance (using constructor injection).
        /// </summary>
        /// <param name="executor">The executor.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="provider">The provider.</param>
        internal EffectBuilder(
            IEffectPipelineExecuter executor,
            in IEffectParameters parameters,
            params IEffectProviderFactory[] provider)
        {
            _executor = executor;
            _parameters = parameters;
            _providers = provider;
        }

        #endregion // Ctor

        #region Append

        /// <summary>
        /// Lazy appends of effect manipulator to the effect pipeline.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public override EffectBuilderBase Append(in IEffectParameters parameters)
        {
            EffectBuilder builder = Create(_executor, parameters, _providers);
            return builder;
        }

        #endregion // Append

        #region Create

        /// <summary>
        /// Creates the effect builder
        /// using effect provider which support the effect's parameters.
        /// </summary>
        /// <param name="parent">The parent (can be used for optimizations).</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="providers">The providers.</param>
        /// <returns></returns>
        internal static EffectBuilder Create(
            in IEffectPipelineExecuter parent, // TODO: use for optimization
            in IEffectParameters parameters, 
            params IEffectProviderFactory[] providers)
        {
            IEffectParameters prms = parameters;
            IEffectProviderFactory provider = providers.First(m => m.CanProcess(prms)); // in not supported by delegate
            IEffectPipelineExecuter executor = provider.CreateExecutor(parent, parameters);
            var builder = new EffectBuilder(executor, parameters, providers);
            return builder;
        }


        #endregion // Create

        #region Build

        /// <summary>
        /// Builds effect's pipeline executer.
        /// </summary>
        /// <returns></returns>
        public override IEffectPipelineExecuter Build() => _executor;

        #endregion // Build
    }
}
