using ImageProcessing.Contracts;
using ImageSharp;
using ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageProcessing.Providers
{
    public class ImageSharpEffectsProvider : IEffectProviderFactory
    {
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
                    return Executer.Create(parent, p);
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion // CreateExecutor

        #region Executer [nested]

        /// <summary>
        /// Factory for IEffectPipelineExecuter implementation
        /// </summary>
        private static class Executer
        {
            /// <summary>
            /// Create executer.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="parent">
            /// The previous executer in the chain.
            /// optimization: will merge the implementation of some effect and 
            /// refer to previous of the first pipe in the optimized sequence (previous of the previous).
            /// </param>
            /// <param name="parameters">The parameters.</param>
            /// <returns></returns>
            public static Executer<T> Create<T>(
                                in IEffectPipelineExecuter parent,
                                in T parameters)
                where T : IEffectParameters
            {
                return new Executer<T>(parent, parameters);
            }
        }
        private interface IOptimaizable
        {
            IEffectPipelineExecuter Parent { get; }
        }

        private class Executer<T> : IEffectPipelineExecuter, IOptimaizable
            where T : IEffectParameters
        {
            private readonly T _parameters;

            /// <summary>
            /// Initializes a new instance .
            /// </summary>
            /// <param name="parent">
            /// The previous executer in the chain.
            /// optimization: will merge the implementation of some effect and 
            /// refer to previous of the first pipe in the optimized sequence (previous of the previous).
            /// </param>
            /// <param name="parameters">The parameters.</param>
            public Executer(in IEffectPipelineExecuter parent, in T parameters)
            {
                _parameters = parameters;
                Parent = parent;
            }

            public IEffectPipelineExecuter Parent { get; }

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
                // Optimization (find the first non-optimizable parent)
                IEffectPipelineExecuter nonOptimizedParent = Parent;
                var compatableParents = new List<IOptimaizable>();
                while (nonOptimizedParent is IOptimaizable optimizable)
                {
                    compatableParents.Add(optimizable);
                    nonOptimizedParent = optimizable.Parent;

                    // Image<Collor> manip optimization
                }

                using (var prevStream = new MemoryStream())
                {
                    Stream source = inputStream;
                    if (nonOptimizedParent != null)
                    {
                        await nonOptimizedParent.ExecuteAsync(inputStream, prevStream);
                        prevStream.Position = 0;
                        source = prevStream;
                    }

                    using (var image = new Image(source))
                    {
                        switch (_parameters)
                        {
                            case ResizeEffectParameters resize:
                                {
                                    var size = new Size(resize.Width, resize.Height);
                                    using (var maniped = image
                                        .Resize(size))
                                    using (maniped.Save(outputStream))
                                    {
                                    }
                                }
                                break;
                            case GrayscalEffectParameters grayscale:
                                {
                                    using (var maniped = image
                                        .Grayscale(GrayscaleMode.Bt601))
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
        }

        #endregion // Executer [nested]
    }
}
