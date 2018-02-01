using System;

namespace ImageProcessing.Contracts
{
    /// <summary>
    /// Represent effects parameter for resize
    /// the parameter will be used in a chain of responsibility pattern 
    /// in order to find the IImageManipPine 
    /// which can accept (know how to work with) the parameter type 
    /// </summary>
    public struct ResizeEffectParameters: IEffectParameters, IEquatable<ResizeEffectParameters>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance .
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="hight">The hight.</param>
        public ResizeEffectParameters(int width, int hight)
        {
            Height = hight;
            Width = width;
        }

        #endregion // Ctor

        #region Width

        /// <summary>
        /// Gets the width.
        /// </summary>
        public int Width { get; }

        #endregion // Width

        #region Hight

        /// <summary>
        /// Gets the hight.
        /// </summary>
        public int Height { get; }

        #endregion // Hight

        #region Equality and Hash Code

        public override bool Equals(object obj)
        {
            return obj is ResizeEffectParameters && Equals((ResizeEffectParameters)obj);
        }

        public bool Equals(ResizeEffectParameters other)
        {
            return Width == other.Width &&
                   Height == other.Height;
        }

        public override int GetHashCode()
        {
            var hashCode = -1439475742;
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ResizeEffectParameters parameters1, ResizeEffectParameters parameters2)
        {
            return parameters1.Equals(parameters2);
        }

        public static bool operator !=(ResizeEffectParameters parameters1, ResizeEffectParameters parameters2)
        {
            return !(parameters1 == parameters2);
        }

        #endregion // Equality and Hash Code
    }
}
