using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Game.Core.Logger;
using Game.Core.Serialize;
using Game.Utils;
using Godot;
using Godot.Collections;
using ZLinq;

namespace Game.Core.FSM
{
    [Tool]
    [GameSerializable]
    public partial class FSM : Node2D
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ReadyGame()
        {
            base._ReadyGame();

            foreach (var state in States)
            {
                if (state != null)
                {
                    state.FSM = this;
                    state._StateReady();
                }
                else
                {
                    Log.Warn($"Null state found in FSM '{Name}' during _ReadyGame. Please check States array.", ELogCategory.FSM);
                }
            }

            Callable.From(() =>
            {
                foreach (var state in States)
                {
                    if (state != null)
                    {
                        state._StateInitialize();
                    }
                    else
                    {
                        Log.Warn($"Null state found in FSM '{Name}' during _ReadyGame. Please check States array.", ELogCategory.FSM);
                    }
                }
            }).CallDeferred();
        }

        [GameMethod]
        public override void _ExitTreeGame()
        {
            base._ExitTreeGame();

            foreach (var state in States)
            {
                if (state != null)
                {
                    state._StateExit();
                    state._StateExitTree();
                }
                else
                {
                    Log.Warn($"Null state found in FSM '{Name}' during _ExitTreeGame. Please check States array.", ELogCategory.FSM);
                }
            }
        }

        [GameMethod]
        public override void _InputGame(InputEvent @event)
        {
            _currentState?._StateHandleInput(@event);
        }

        [GameMethod]
        public override void _ProcessGame(double delta)
        {
            _currentState?._StateProcess(delta);
        }

        [GameMethod]
        public override void _PhysicsProcessGame(double delta)
        {
            _currentState?._StateProcessPhysics(delta);
        }

        [GameMethod]
        public override void _Notification(int what)
        {
            base._Notification(what);

            #if TOOLS
            if (Engine.IsEditorHint())
            {
                if (what == NotificationEditorPostSave)
                {
                    States.Clear();
                    foreach (var child in this.Children())
                    {
                        if (child is State state)
                        {
                            States.Add(state);
                        }
                    }
                    NotifyPropertyListChanged();
                }
            }
            #endif
        }
        #endregion

        #region PUBLIC METHODS
        /// <summary>
        ///     Request to change of current state.
        ///     <example>
        ///         For example:
        ///         <code>
        /// FSM.RequestState{IdleState}(new Dictionary{{"ExampleParam", ParamVar}});
        ///         </code>
        ///     </example>
        /// </summary>
        /// <typeparam name="TStateType">
        ///     Type of state which was registered to <c>States</c>.
        ///     If the type of the state is equal to the current, then the requested state will not change.
        /// </typeparam>
        /// <param name="parameters">
        ///     Parameters to state.
        ///     If the parameters are equal null, then parameters will be allocated.
        /// </param>
        /// <param name="force">Force change state.</param>
        public void RequestState<TStateType>(Dictionary parameters = null, bool force = false,
            [CallerMemberName] string fsmCallerMemberName = "",
            [CallerFilePath] string fsmCallerFilePath = "",
            [CallerLineNumber] int fsmCallerLineNumber = 0)
            where TStateType : State
        {
            if (!force && IsCurrentStateEqual<TStateType>())
                return;

            var state = GetState<TStateType>();
            if (state == null)
            {
                Log.Error(
                    $"Failed to get state of type '{typeof(TStateType).Name}' for FSM '{Root?.Name ?? Name.ToString()}'. State not registered or error in GetState.",
                    ELogCategory.FSM);
                return;
            }


            string originNs;
            string originClass;

            try
            {
                var frame = new StackFrame(1, false);
                var method = frame.GetMethod();
                var declaringType = method?.DeclaringType;

                originNs = declaringType?.Namespace;
                originClass = declaringType?.Name;

                if (originClass != null && originClass.Contains("<") && originClass.Contains(">"))
                {
                    if (declaringType.DeclaringType != null)
                    {
                        originClass = declaringType.DeclaringType.Name;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn($"Could not determine origin for FSM state change log: {e.Message}", ELogCategory.Utils);
                originNs = "N/A (FrameError)";
                originClass = "N/A (FrameError)";
            }

            string currentRootName = Root?.Name?.ToString() ?? Name.ToString();
            string currentName = _currentState?.Name?.ToString() ?? "Null";
            string nextName = state.Name.ToString();

            Log.Info(
                $"'{currentRootName}' requested changing state from '{currentName}' to '{nextName}'",
                ELogCategory.FSM,
                new Event(
                    originNs,
                    originClass,
                    fsmCallerMemberName,
                    fsmCallerFilePath,
                    fsmCallerLineNumber
                )
            );

            parameters ??= new Dictionary();

            // Old State
            if (_currentState != null)
            {
                _currentState.Parameters?.Clear();
                _currentState._StateExit();
                _currentState.Disable();
            }

            // New State
            _currentState = state;
            _currentState.Parameters = parameters;
            _currentState.Enable();
            _currentState._StateEnter();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public State GetCurrentState()
        {
            return _currentState;
        }

        /// <typeparam name="TStateType">Type of state which was registered to <c>States</c>.</typeparam>
        /// <returns>State</returns>
        /// <exception cref="InvalidOperationException">If type of state not registered to <c>States</c> or cast fails.</exception>
        public TStateType GetState<TStateType>() where TStateType : State
        {
            foreach (var stateNode in States)
            {
                if (stateNode is TStateType typedState)
                {
                    return typedState;
                }
            }
            throw new InvalidOperationException(
                $"State of type '{typeof(TStateType).Name}' is not registered in FSM '{Name}' or is not a child of this FSM.");
        }

        /// <typeparam name="TStateType">Type of state which was registered to <c>States</c>.</typeparam>
        public bool IsCurrentStateEqual<TStateType>() where TStateType : State
        {
            return _currentState is TStateType;
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Node2D Root;
        [Export]
        public Array<State> States = [];
        #endregion

        #region PRIVATE FIELDS
        [GameSerialize]
        private State _currentState;
        #endregion
    }
}