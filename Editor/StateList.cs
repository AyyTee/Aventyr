using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class StateList
    {
        /// <summary>Used as a placeholder to avoid edge cases in the linked list.</summary>
        class FirstNode : ICommand
        {
            public FirstNode() {}
            public void Do() { throw new NotSupportedException(); }
            public void Redo() { throw new NotSupportedException(); }
            public void Undo() { throw new NotSupportedException(); }
            public ICommand Clone() { throw new NotSupportedException(); }
        }

        LinkedList<ICommand> _list = new LinkedList<ICommand>();
        LinkedListNode<ICommand> _currentState;
        public readonly int UndoSteps;

        public StateList()
        {
            _list.AddFirst(new FirstNode());
            _currentState = _list.First;
            UndoSteps = 1000;
        }

        public bool Undo()
        {
            if (_currentState.Previous == null)
            {
                return false;
            }
            _currentState.Value.Undo();
            _currentState = _currentState.Previous;
            return true;
        }

        public bool Redo()
        {
            if (_currentState.Next == null)
            {
                return false;
            }
            _currentState = _currentState.Next;
            _currentState.Value.Redo();
            return true;
        }

        public void Add(ICommand state, bool callDo)
        {
            while (_list.Last != _currentState)
            {
                _list.RemoveLast();
            }
            ICommand clonedState = state.Clone();
            if (_currentState == null)
            {
                _list.AddFirst(clonedState);
            }
            else
            {
                _list.AddAfter(_currentState, clonedState);
            }

            if (callDo)
            {
                clonedState.Do();
            }
            if (_list.Count > UndoSteps + 1)
            {
                _list.RemoveFirst();
            }
            _currentState = _list.Last;
        }
    }
}
