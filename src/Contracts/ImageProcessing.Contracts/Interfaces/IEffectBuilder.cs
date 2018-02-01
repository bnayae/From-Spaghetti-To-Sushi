using System;
using System.IO;

namespace ImageProcessing.Contracts
{
    /// <summary>
    /// Represent unit in the effects pipeline
    /// this basic interface will be extend with extension method by the effects providers.
    /// will be implement by the common logic.
    /// the instance will keep the parameters and the provider (in order for future execution).
    /// </summary>
    public abstract class EffectBuilderBase
    {
        #region Append

        /// <summary>
        /// Lazy appends of effect manipulator to the effect pipeline.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public abstract EffectBuilderBase Append(
                                in IEffectParameters parameters);

        #endregion // Append

        /// <summary>
        /// Builds effect's pipeline executer.
        /// </summary>
        /// <returns></returns>
        public abstract IEffectPipelineExecuter Build();

        #region Operator overloads (+)

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="pipe">The pipe.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static EffectBuilderBase operator +(
            EffectBuilderBase pipe,
            IEffectParameters parameter)
        {
            return pipe.Append(parameter);
        }

        #endregion // Operator overloads (+)


    }
}
