using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    /// <summary>
    /// Interface for begin and end step events for any instance stored in an IScene.
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// Intended to be called from IScene's step method before movement calculations are performed.
        /// </summary>
        void StepBegin();
        /// <summary>
        /// Intended to be called from IScene's step method after movement calculations are performed.
        /// </summary>
        void StepEnd();
    }
}
