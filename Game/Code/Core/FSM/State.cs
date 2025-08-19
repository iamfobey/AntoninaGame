using System;
using System.Runtime.CompilerServices;
using Game.Core.Serialize;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Core.FSM
{
    [Tool]
    [GameSerializable]
    public partial class State : Node2D
    {
        #region PUBLIC METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enable()
        {
            IsEnabled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Disable()
        {
            IsEnabled = false;
        }

        public virtual void _StateReady()
        {
            _globalSubscriptions = new CompositeDisposable();
            _localSubsribtions = new CompositeDisposable();
            _localSubsribtions.IgnoreWarnings = true;
        }

        public virtual void _StateInitialize()
        {
        }

        public virtual void _StateEnter()
        {
        }

        public virtual void _StateHandleInput(InputEvent @event)
        {
        }

        public virtual void _StateProcess(double delta)
        {
        }

        public virtual void _StateProcessPhysics(double delta)
        {
        }

        public virtual void _StateExit()
        {
            _localSubsribtions.Dispose();
            _localSubsribtions = new();
            _localSubsribtions.IgnoreWarnings = true;
        }

        public virtual void _StateExitTree()
        {
            _globalSubscriptions.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Root<T>() where T : Node2D
        {
            #if DEBUG
            return FSM.Root switch
            {
                T root => root,
                _ => throw new InvalidCastException($"FSM.Root не является типом {typeof(T)}.")
            };
            #else
            return (T)FSM.Root;
            #endif
        }
        #endregion

        #region PUBLIC FIELDS
        [GameSerialize] public bool IsEnabled { get; private set; } = false;
        [GameSerialize]
        public FSM FSM;
        /// <summary>
        ///     Parameters of state which was received from <c>FSM.RequestState</c>
        /// </summary>
        public Dictionary Parameters;
        #endregion

        protected CompositeDisposable _globalSubscriptions;
        protected CompositeDisposable _localSubsribtions;
    }
}