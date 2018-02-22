using ImageProcessing.Contracts;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessing.Commons
{
    /// <summary>
    /// Creates the first builder of a chain Exposed by the IoC.
    /// </summary>
    public class EffectBuilderFactory : IEffectBuilderFactory
    {
        private readonly IEffectProviderFactory[] _providers;
        private readonly EffectBuilderBase _empty;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectBuilderFactory"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public EffectBuilderFactory(
            params IEffectProviderFactory[] provider)
        {
            _providers = provider;
            _empty = new EmptyEffectBuilder(provider);
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

        public EffectBuilderBase Create() => _empty;

        #endregion // Create

        #region EmptyEffectBuilder [nested]

        /// <summary>
        /// Use as empty builder for initial step of fluent api.
        /// expected single instance per EffectBuilderFactory.
        /// </summary>
        private class EmptyEffectBuilder : EffectBuilderBase
        {
            private readonly IEffectProviderFactory[] _providers;

            #region Ctor

            public EmptyEffectBuilder(IEffectProviderFactory[] providers)
            {
                _providers = providers;
            }

            #endregion // Ctor

            #region Append

            /// <summary>
            /// Lazy appends of effect manipulator 
            /// to the effect pipeline.
            /// </summary>
            /// <param name="parameters">The parameters.</param>
            /// <returns></returns>
            public override EffectBuilderBase Append(in IEffectParameters parameters)
            {
                return EffectBuilder.Create(null, parameters, _providers); ;
            }

            #endregion // Append

            #region Build

            /// <summary>
            /// Builds effect's pipeline executer.
            /// </summary>
            /// <returns></returns>
            public override EffectPipelineExecuterBase Build() => NonEffectPipelineExecuter.Default;

            #endregion // Build

            #region NonEffectPipelineExecuter [nested]

            private class NonEffectPipelineExecuter : EffectPipelineExecuterBase
            {
                public NonEffectPipelineExecuter()
                    :base(null, null)
                {

                }
                public readonly static EffectPipelineExecuterBase Default = new NonEffectPipelineExecuter();

                protected override Task OnExecuteAsync(
                    Stream inputStream,
                    Stream outputStream)
                {
                    return inputStream.CopyToAsync(outputStream);
                }
            }

            #endregion // NonEffectPipelineExecuter [nested]
        }

        #endregion // EmptyEffectBuilder [nested]
    }
}
