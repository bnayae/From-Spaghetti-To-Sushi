using ImageProcessing.Contracts;
using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Processing;
using Setting.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageProcessing.Providers
{
    public class ImageSharpEffectsProvider : IEffectProviderFactory
    {
        private readonly Task<ProviderSetting> _settingTask;

        #region Ctor

        public ImageSharpEffectsProvider(ISetting setting)
        {
            var cfg = Configuration.Default;
            cfg.AddImageFormat(new JpegFormat());
            cfg.AddImageFormat(new PngFormat());
            cfg.AddImageFormat(new GifFormat());
            cfg.AddImageFormat(new BmpFormat());
            _settingTask = setting.GetAsync<ProviderSetting>(ProviderSetting.Key);
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
        public IEffectPipelineExecuter CreateExecutor(
            in IEffectPipelineExecuter parent,
            in IEffectParameters parameters)
        {
            switch (parameters)
            {
                case ResizeEffectParameters p:
                    return Executer.Create(parent, p, _settingTask);
                case GrayscaleEffectParameters p:
                    return Executer.Create(parent, p, _settingTask);
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion // CreateExecutor

        #region Executer [nested]

        /// <summary>
        /// Factory for IEffectPipelineExecuter implementation
        /// </summary>
        private abstract class Executer
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
                                in IEffectPipelineExecuter parent,
                                in T parameters,
                                in Task<ProviderSetting> settingTask)
                where T : IEffectParameters
            {
                return new Executer<T>(parent, parameters, settingTask);
            }

            #endregion // Create

            #region DelegateExecutionAsync

            /// <summary>
            /// Provider internal optimization the specified parent.
            /// </summary>
            /// <param name="inputStream">The input stream.</param>
            /// <returns></returns>
            public abstract Task<Image<Color>> DelegateExecutionAsync(Stream inputStream);

            #endregion // DelegateExecutionAsync
        }

        private class Executer<T> : Executer, IEffectPipelineExecuter
            where T : IEffectParameters
        {
            private readonly Task<ProviderSetting> _settingTask;
            private readonly T _parameters;

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
                in IEffectPipelineExecuter parent,
                in T parameters,
                in Task<ProviderSetting> settingTask)
            {
                _parameters = parameters;
                Parent = parent;
                _settingTask = settingTask;
            }

            #endregion // Ctor

            #region Parent

            /// <summary>
            /// The previous executor (in the pipe-line).
            /// </summary>
            public IEffectPipelineExecuter Parent { get; }

            #endregion // Parent

            #region DoResize

            /// <summary>
            /// Does the resize effect.
            /// </summary>
            /// <param name="image">The image.</param>
            /// <param name="resize">The resize.</param>
            /// <returns></returns>
            private static Image<Color> DoResize(Image<Color> image, ResizeEffectParameters resize)
            {
                var size = new Size(resize.Width, resize.Height);
                image = image.Resize(size);
                return image;
            }

            #endregion // DoResize

            #region DoGrayscale

            /// <summary>
            /// Does the grayscale effect.
            /// </summary>
            /// <param name="image">The image.</param>
            /// <returns></returns>
            private static Image<Color> DoGrayscale(Image<Color> image)
            {
                image = image.Grayscale(GrayscaleMode.Bt601);
                return image;
            }

            #endregion // DoGrayscale

            #region DelegateExecution

            /// <summary>
            /// Provider internal optimization the specified parent.
            /// </summary>
            /// <param name="inputStream">The input stream.</param>
            /// <returns></returns>
            public override async Task<Image<Color>> DelegateExecutionAsync(Stream inputStream)
            {
                #region Image<Color> image = ...

                Image<Color> image;
                if (Parent == null)
                    image = new Image(inputStream);
                else
                {
                    var optimizedParent = Parent as Executer;
                    if (optimizedParent == null)
                    {
                        #region image = await Parent.ExecuteAsync(inputStream, memStream)

                        using (var memStream = new MemoryStream())
                        {
                            await Parent.ExecuteAsync(inputStream, memStream);
                            memStream.Position = 0;
                            image = new Image(memStream);
                        }

                        #endregion // image = await Parent.ExecuteAsync(inputStream, memStream)
                    }
                    else
                        image = await optimizedParent.DelegateExecutionAsync(inputStream);
                }

                #endregion // Image<Color> image = ...

                switch (_parameters)
                {
                    case ResizeEffectParameters resize:
                        {
                            image = DoResize(image, resize);
                            break;
                        }
                    case GrayscaleEffectParameters grayscale:
                        {
                            image = DoGrayscale(image);
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
                return image;
            }

            #endregion // DelegateExecution

            #region ExecuteAsync

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
                ProviderSetting setting = await _settingTask;
                if (setting.Optimized)
                    await ExecuteOptimizedAsync(inputStream, outputStream);
                else
                    await ExecuteNoneOptimizedAsync(inputStream, outputStream);
            }

            #endregion // ExecuteAsync

            #region ExecuteOptimizedAsync

            /// <summary>
            /// Executes the specified image.
            /// will pass the image to the previous and
            /// act on the result of the previous pipe.
            /// </summary>
            /// <param name="inputStream">The input image stream.</param>
            /// <param name="outputStream">The output image stream.</param>
            /// <returns></returns>
            public async Task ExecuteOptimizedAsync(
                Stream inputStream,
                Stream outputStream)
            {
                using (Image<Color> maniped = await DelegateExecutionAsync(
                                                    inputStream))
                using (maniped.Save(outputStream))
                {
                }
            }

            #endregion // ExecuteOptimizedAsync

            #region ExecuteNoneOptimizedAsync

            /// <summary>
            /// Executes the specified image.
            /// will pass the image to the previous and
            /// act on the result of the previous pipe.
            /// </summary>
            /// <param name="inputStream">The input image stream.</param>
            /// <param name="outputStream">The output image stream.</param>
            /// <returns></returns>
            private async Task ExecuteNoneOptimizedAsync(Stream inputStream, Stream outputStream)
            {
                IDisposable disp = EmptyDisposable.Instance;

                #region await Parent.ExecuteAsync(inputStream, afterManipStream); disp = ...

                Stream afterManipStream = null;
                if (Parent != null)
                {
                    afterManipStream = new MemoryStream();
                    disp = afterManipStream;
                    // delegate the call to the parent and use it's output as input
                    await Parent.ExecuteAsync(inputStream, afterManipStream);
                    afterManipStream.Position = 0;
                }

                #endregion // await Parent.ExecuteAsync(inputStream, afterManipStream); disp = ...

                using (disp)
                {
                    // construct image either from previous stage result or from input
                    using (var image = new Image(afterManipStream ?? inputStream))
                    {
                        switch (_parameters)
                        {
                            case ResizeEffectParameters resize:
                                {
                                    using (var maniped = DoResize(image, resize))
                                    using (maniped.Save(outputStream))
                                    {
                                    }
                                }
                                break;
                            case GrayscaleEffectParameters grayscale:
                                {
                                    using (var maniped = DoGrayscale(image))
                                    using (maniped.Save(outputStream))
                                    {
                                    }
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
            }

            #endregion // ExecuteNoneOptimizedAsync
        }

        #endregion // Executer [nested]

        #region EmptyDisposable [nested]

        private class EmptyDisposable : IDisposable
        {
            public static IDisposable Instance = new EmptyDisposable();
            private EmptyDisposable() { }
            public void Dispose() { }
        }

        #endregion // EmptyDisposable [nested]
    }
}
