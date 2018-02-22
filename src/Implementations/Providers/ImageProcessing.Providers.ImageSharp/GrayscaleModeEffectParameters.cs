using ImageProcessing.Contracts;
using ImageSharp.Processing;
using System;

namespace ImageProcessing.Providers
{
    /// <summary>
    /// Represent effects parameter for resize
    /// the parameter will be used in a chain of responsibility pattern 
    /// in order to find the IImageManipPine 
    /// which can accept (know how to work with) the parameter type 
    /// </summary>
    public struct GrayscaleModeEffectParameters :
        IEffectParameters, IEquatable<GrayscaleModeEffectParameters>
    {
        public GrayscaleModeEffectParameters(GrayscaleMode mode)
        {
            Mode = mode;
        }

        public GrayscaleMode Mode { get;}

        #region Equality and Hash Code

        public override bool Equals(object obj)
        {
            return obj is GrayscaleModeEffectParameters && Equals((GrayscaleModeEffectParameters)obj);
        }

        public bool Equals(GrayscaleModeEffectParameters other)
        {
            return Mode == other.Mode;
        }

        public override int GetHashCode()
        {
            var hashCode = 1397651250;
            hashCode = hashCode * -1521134295 + Mode.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(GrayscaleModeEffectParameters parameters1, GrayscaleModeEffectParameters parameters2)
        {
            return parameters1.Equals(parameters2);
        }

        public static bool operator !=(GrayscaleModeEffectParameters parameters1, GrayscaleModeEffectParameters parameters2)
        {
            return !(parameters1 == parameters2);
        }

        #endregion // Equality and Hash Code
    }
}
