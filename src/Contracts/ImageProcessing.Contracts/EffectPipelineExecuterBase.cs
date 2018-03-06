using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace ImageProcessing.Contracts
{
    /// <summary>
    /// Effect's pipeline executer
    /// </summary>
    public abstract class EffectPipelineExecuterBase
    {
        protected readonly ILogger _logger;
        private readonly IMetricReporter _metric;
        private readonly ImmutableList<EffectPipelineExecuterBase> _effectExecuters;

        [Obsolete("Should only be used by tests", true)]
        public EffectPipelineExecuterBase()
        {
            _effectExecuters = ImmutableList<EffectPipelineExecuterBase>.Empty;
        }

        public EffectPipelineExecuterBase(
            in IEffectParameters parameters,
            IMetricReporter metric,
            ILogger logger,
            EffectPipelineExecuterBase parent)
        {
            Parameters = parameters;
            _logger = logger.ForContext<EffectPipelineExecuterBase>();
            _metric = metric;
            var executers = ImmutableList<EffectPipelineExecuterBase>.Empty;
            if (parent != null)
                executers = parent._effectExecuters;
            _effectExecuters = executers.Add(this);
        }
        public IEffectParameters Parameters { get; }

        internal protected abstract bool ShouldSkipExecute(
            EffectPipelineExecuterBase next);

        /// <summary>
        /// Executes the specified image.
        /// will pass the image to the previous and
        /// act on the result of the previous pipe.
        /// </summary>
        /// <param name="inputStream">The input image stream.</param>
        /// <param name="outputStream">The output image stream.</param>
        /// <returns></returns>
        public async Task ExecuteAsync(Stream inputStream, Stream outputStream)
        {
            if (_effectExecuters.Count == 0)
                return;

            var optimizedBuilderList = ImmutableList.CreateBuilder<EffectPipelineExecuterBase>();
            for (int i = 0; i < _effectExecuters.Count; i++)
            {
                EffectPipelineExecuterBase current = _effectExecuters[i];
                EffectPipelineExecuterBase next = null;
                if (i < _effectExecuters.Count - 1)
                    next = _effectExecuters[i + 1];

                optimizedBuilderList.Add(current);
                if (next != null && current.ShouldSkipExecute(next))
                    continue;

                await ExecuteInternalAsync(current);
                optimizedBuilderList.Clear();
            }

            async Task ExecuteInternalAsync(EffectPipelineExecuterBase current)
            {

                using (LogContext.PushProperty("CorrelateId", Guid.NewGuid()))
                {
                    try
                    {
                        _logger.Debug("Start");
                        _metric.Hit();
                        using (_metric.StartScope())
                        {
                            IImmutableList<EffectPipelineExecuterBase> immutableList =
                                optimizedBuilderList.ToImmutable();
                            await current.OnExecuteAsync(inputStream, outputStream, immutableList);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error {@ex}", ex);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Executes the specified image.
        /// will pass the image to the previous and
        /// act on the result of the previous pipe.
        /// </summary>
        /// <param name="inputStream">The input image stream.</param>
        /// <param name="outputStream">The output image stream.</param>
        /// <param name="effectExecuters">The effect executers.</param>
        /// <returns></returns>
        protected internal abstract Task OnExecuteAsync(
                                        Stream inputStream,
                                        Stream outputStream,
                                        IImmutableList<EffectPipelineExecuterBase> effectExecuters);
    }
}
