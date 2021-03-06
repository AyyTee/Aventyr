﻿using EditorLogic.Command;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorLogic
{
    public class StateList
    {
        /// <summary>Used as a placeholder to avoid edge cases in the linked list.</summary>
        class FirstNode : ICommand
        {
            public FirstNode() {}
            public bool IsMarker => true;
            public void Do() { throw new NotSupportedException(); }
            public void Redo() { throw new NotSupportedException(); }
            public void Undo() { throw new NotSupportedException(); }
            public ICommand ShallowClone() { throw new NotSupportedException(); }
        }

        LinkedList<ICommand> _list = new LinkedList<ICommand>();
        LinkedListNode<ICommand> _currentState;
        public readonly int UndoSteps;

        public StateList()
        {
            Reset();
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

        public void Reset()
        {
            _list.Clear();
            _list.AddFirst(new FirstNode());
            _currentState = _list.First;
        }

        public void Add(ICommand state, bool callDo = true)
        {
            while (_list.Last != _currentState)
            {
                _list.RemoveLast();
            }
            ICommand clonedState = state.ShallowClone();
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
