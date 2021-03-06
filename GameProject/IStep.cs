﻿namespace Game
{
    /// <summary>
    /// Interface for begin and end step events for any instance stored in an IScene.
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// Intended to be called from IScene's step method before movement calculations are performed.
        /// </summary>
        /// <param name="stepSize">Represents physics step size in seconds.</param>
        void StepBegin(IScene scene, float stepSize);
        /// <summary>
        /// Intended to be called from IScene's step method after movement calculations are performed.
        /// </summary>
        /// <param name="stepSize">Represents physics step size in seconds.</param>
        void StepEnd(IScene scene, float stepSize);
    }
}
