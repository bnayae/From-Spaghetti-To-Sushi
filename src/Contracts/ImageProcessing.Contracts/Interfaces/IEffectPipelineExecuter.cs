using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageProcessing.Contracts
{
    /// <summary>
    /// Effect's pipeline executer
    /// </summary>
    public interface IEffectPipelineExecuter
    {
        /// <summary>
        /// Executes the specified image.
        /// will pass the image to the previous and
        /// act on the result of the previous pipe.
        /// </summary>
        /// <param name="inputStream">The input image stream.</param>
        /// <param name="outputStream">The output image stream.</param>
        /// <returns></returns>
        Task ExecuteAsync(Stream inputStream, Stream outputStream);
    }
}
