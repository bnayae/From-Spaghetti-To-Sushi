using System;

namespace ImageProcessing.Contracts
{
    /// <summary>
    /// Represent effects parameter for resize
    /// the parameter will be used in a chain of responsibility pattern 
    /// in order to find the IImageManipPine 
    /// which can accept (know how to work with) the parameter type 
    /// </summary>
    public struct GrayscalEffectParameters : 
        IEffectParameters,
        IEquatable<GrayscalEffectParameters>
    {
        #region Equality and Hash Code

        public bool Equals(GrayscalEffectParameters other) => true;

        public override bool Equals(object obj) =>
            obj is GrayscalEffectParameters;

        public override int GetHashCode() => 0;

        #endregion // Equality and Hash Code
    }
}
