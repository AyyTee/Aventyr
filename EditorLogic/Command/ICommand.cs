using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;

namespace EditorLogic.Command
{
    public interface ICommand : IShallowClone<ICommand>
    {
        /// <summary>
        /// Undo/Redo stops at this command.
        /// </summary>
        bool IsMarker { get; }
        /// <summary>
        /// Method that is called the first time the command is called.
        /// </summary>
        void Do();
        /// <summary>
        /// Method that is called if the command is repeated.
        /// </summary>
        void Redo();
        /// <summary>
        /// Method that is called to reverse the effects of Do or Redo.
        /// </summary>
        void Undo();
    }
}
