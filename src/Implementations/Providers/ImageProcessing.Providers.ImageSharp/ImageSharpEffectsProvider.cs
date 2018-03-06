using ImageProcessing.Contracts;
using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Processing;
using Serilog;
using Setting.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace ImageProcessing.Providers
{
    internal class ImageSharpEffectsProvider : IEffectProviderFactory
    {
        private readonly IMetricReporter _metric;
        private readonly ILogger _logger;

        #region Ctor

        public ImageSharpEffectsProvider(
            IMetricReporter metric,
            ILogger logger)
        {
            var cfg = Configuration.Default;
            cfg.AddImageFormat(new JpegFormat());
            cfg.AddImageFormat(new PngFormat());
            cfg.AddImageFormat(new GifFormat());
            cfg.AddImageFormat(new BmpFormat());
            _metric = metric;
            _logger = logger;
        }

        #endregion // Ctor

        #region CanProcess

        /// <summary>
        /// Determines whether this instance can process the specified parameters.
        /// use for discovering aligable processing for specific effect.
        /// Will be used by the builder
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// <c>true</c> if this instance can process the specified parameters; otherwise, <c>false</c>.
        /// </returns>
        public bool CanProcess(in IEffectParameters parameters)
        {
            if (parameters is ResizeEffectParameters)
                return true;
            if (parameters is GrayscaleEffectParameters)
                return true;
            if (parameters is GrayscaleModeEffectParameters)
                return true;
            return false;
        }

        #endregion // CanProcess

        #region CreateExecutor

        /// <summary>
        /// build the chain
        /// </summary>
        /// <param name="parent">
        /// The previous executer in the chain.
        /// optimization: will merge the implementation of some effect and 
        /// refer to previous of the first pipe in the optimized sequence (previous of the previous).
        /// </param>
        /// <param name="parameters">The parameters.</param>
        public EffectPipelineExecuterBase CreateExecutor(
            in EffectPipelineExecuterBase parent,
            in IEffectParameters parameters)
        {
            switch (parameters)
            {
                case ResizeEffectParameters p:
                    //return new Executer<ResizeEffectParameters>(...)
                    return Executer.Create(parent, p, _metric, _logger);
                case GrayscaleEffectParameters p:
                    return Executer.Create(parent, p, _metric, _logger);
                case GrayscaleModeEffectParameters p:
                    return Executer.Create(parent, p, _metric, _logger);
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion // CreateExecutor

        private interface ILocalExecuter
        {
        }

        #region Executer [nested]

        /// <summary>
        /// Factory for IEffectPipelineExecuter implementation
        /// </summary>
        private static class Executer
        {
            #region Create

            /// <summary>
            /// Create executer.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="parent">The previous executer in the chain.
            /// optimization: will merge the implementation of some effect and
            /// refer to previous of the first pipe in the optimized sequence (previous of the previous).</param>
            /// <param name="parameters">The parameters.</param>
            /// <param name="settingTask">The setting task.</param>
            /// <returns></returns>
            public static Executer<T> Create<T>(
                                in EffectPipelineExecuterBase parent,
                                in T parameters,
                                IMetricReporter metric,
                                ILogger logger)
                where T : IEffectParameters
            {
                return new Executer<T>(
                    parent,
                    parameters,
                    metric,
                    logger);
            }

            #endregion // Create
        }

        private class Executer<T> : EffectPipelineExecuterBase, ILocalExecuter
            where T : IEffectParameters
        {
            #region Ctor

            /// <summary>
            /// Initializes a new instance .
            /// </summary>
            /// <param name="parent">The previous executer in the chain.
            /// optimization: will merge the implementation of some effect and
            /// refer to previous of the first pipe in the optimized sequence (previous of the previous).</param>
            /// <param name="parameters">The parameters.</param>
            /// <param name="settingTask">The setting task.</param>
            public Executer(
                in EffectPipelineExecuterBase parent,
                in T parameters,
                IMetricReporter metric,
                ILogger logger)
                : base(parameters, metric, logger, parent)
            {
            }

            #endregion // Ctor

            #region DoResize

            /// <summary>
            /// Does the resize effect.
            /// </summary>
            /// <param name="image">The image.</param>
            /// <param name="resize">The resize.</param>
            /// <returns></returns>
            private static void DoResize(Image<Color> image, ResizeEffectParameters resize)
            {
                var size = new Size(resize.Width, resize.Height);
                image.Resize(size);
            }

            #endregion // DoResize

            #region DoGrayscale

            /// <summary>
            /// Does the grayscale effect.
            /// </summary>
            /// <param name="image">The image.</param>
            /// <returns></returns>
            private static void DoGrayscale(Image<Color> image, GrayscaleMode mode = GrayscaleMode.Bt601)
            {
                image.Grayscale(mode);
            }

            #endregion // DoGrayscale

            #region ExecuteAsync

            /// <summary>
            /// Executes the specified image.
            /// will pass the image to the previous and
            /// act on the result of the previous pipe.
            /// </summary>
            /// <param name="inputStream">The input image stream.</param>
            /// <param name="outputStream">The output image stream.</param>
            /// <returns></returns>
            protected override Task OnExecuteAsync(
                                        Stream inputStream,
                                        Stream outputStream,
                                        IImmutableList<EffectPipelineExecuterBase> effectExecuters)
            {
                using (Image<Color> image = new Image(inputStream))
                {
                    foreach (var executer in effectExecuters)
                    {
                        switch (executer.Parameters)
                        {
                            case ResizeEffectParameters resize:
                                {
                                    DoResize(image, resize);
                                    break;
                                }
                            case GrayscaleEffectParameters grayscale:
                                {
                                    DoGrayscale(image);
                                    break;
                                }
                            case GrayscaleModeEffectParameters grayscale:
                                {
                                    DoGrayscale(image, grayscale.Mode);
                                    break;
                                }
                            default:
                                throw new NotImplementedException();
                        }

                    }
                    image.Save(outputStream);
                }

                return Task.CompletedTask;
            }

            #endregion // ExecuteAsync

            #region ShouldSkipExecute

            protected override bool ShouldSkipExecute(
                EffectPipelineExecuterBase next) => next is ILocalExecuter;

            #endregion // ShouldSkipExecute
        }

        #endregion // Executer [nested]
    }
}
