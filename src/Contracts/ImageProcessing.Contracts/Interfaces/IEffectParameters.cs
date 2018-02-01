using System;

namespace ImageProcessing.Contracts
{
    /// <summary>
    /// Represent effects parameter
    /// the parameter will be used in a chain of responsibility pattern 
    /// in order to find the IImageManipPine 
    /// which can accept (know how to work with) the parameter type 
    /// </summary>
    public interface IEffectParameters
    {
    }
}
