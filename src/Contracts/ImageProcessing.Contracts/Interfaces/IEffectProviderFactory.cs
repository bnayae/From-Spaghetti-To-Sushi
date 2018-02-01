using System;
using System.IO;

namespace ImageProcessing.Contracts
{
    /// <summary>
    /// Represent effect provider 
    /// </summary>
    public interface IEffectProviderFactory
    {
        /// <summary>
        /// Determines whether this instance can process the specified parameters.
        /// use for discovering aligable processing for specific effect.
        /// Will be used by the builder
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        ///   <c>true</c> if this instance can process the specified parameters; otherwise, <c>false</c>.
        /// </returns>
        bool CanProcess(in IEffectParameters parameters);

        /// <summary>
        /// build the chain
        /// </summary>
        /// <param name="parent">
        /// The previous executer in the chain.
        /// optimization: will merge the implementation of some effect and 
        /// refer to previous of the first pipe in the optimized sequence (previous of the previous).
        /// </param>
        /// <param name="parameters">The parameters.</param>
        IEffectPipelineExecuter CreateExecutor(
            in IEffectPipelineExecuter parent,
            in IEffectParameters parameters);
    }
}
