using Serilog;
using Serilog.Context;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageProcessing.Contracts
{
    /// <summary>
    /// Effect's pipeline executer
    /// </summary>
    public abstract class EffectPipelineExecuterBase
    {
        protected readonly ILogger _logger;
        private readonly IMetricReporter _metric;

        public EffectPipelineExecuterBase(
            IMetricReporter metric,
            ILogger logger)
        {
            _logger = logger.ForContext<EffectPipelineExecuterBase>();
            _metric = metric;
        }

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
            using (LogContext.PushProperty("CorrelateId", Guid.NewGuid()))
            {
                try
                {
                    _logger.Debug("Start");
                    _metric.Hit();
                    using (_metric.StartScope())
                    {
                        await OnExecuteAsync(inputStream, outputStream);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error {@ex}", ex);
                    throw;
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
        /// <returns></returns>
        protected internal abstract Task OnExecuteAsync(Stream inputStream, Stream outputStream);
    }
}
