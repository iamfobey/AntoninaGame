using System;
using System.Collections.Generic;
using Game.Core.Logger;
using Godot;

namespace Game.Utils
{
    public sealed class SignalSubscription : IDisposable
    {
        public static class SignalCounter
        {
            public static int Count;
        }
        
        public SignalSubscription(GodotObject emitter, StringName signalName, Callable callable)
        {
            _emitter = emitter;
            _signalName = signalName;
            _callable = callable;

            if (GodotObject.IsInstanceValid(_emitter))
            {
                try
                {
                    _emitter.Connect(_signalName, _callable);
                    _isSubscribed = true;
                    Log.Debug($"Connected {_emitter} to {_signalName} {_callable.Delegate.Method}", ELogCategory.Utils);

                    SignalCounter.Count += 1;
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to connect to signal '{_signalName}' on '{_emitter}': {ex.Message}",
                        ELogCategory.Utils);
                    _isSubscribed = false;
                }
            }
            else
            {
                Log.Warn($"Emitter '{_emitter}' is not valid or null. Cannot connect signal '{_signalName}'.",
                    ELogCategory.Utils);
                _isSubscribed = false;
            }
        }

        #region PUBLIC METHODS
        public void Dispose()
        {
            if (_disposed) return;

            if (_isSubscribed && GodotObject.IsInstanceValid(_emitter))
            {
                if (_emitter.IsConnected(_signalName, _callable))
                {
                    try
                    {
                        _emitter.Disconnect(_signalName, _callable);
                        
                        SignalCounter.Count -= 1;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[SignalSubscription] Failed to disconnect from signal '{_signalName}' on '{_emitter}': {ex.Message}",
                            ELogCategory.System);
                    }
                }
            }
            _disposed = true;
            _isSubscribed = false;
        }
        #endregion

        #region PRIVATE FIELDS
        private readonly GodotObject _emitter;
        private readonly StringName _signalName;
        private readonly Callable _callable;
        private bool _isSubscribed;
        private bool _disposed;
        #endregion
    }

    public sealed class CompositeDisposable : IDisposable
    {
        ~CompositeDisposable()
        {
            if (IgnoreWarnings)
                return;
            
            if (SignalSubscription.SignalCounter.Count > 0)
                Log.Warn($"Not all subs was disposed. Count :{SignalSubscription.SignalCounter.Count}");
        }
        
        #region PUBLIC METHODS
        public void Add(IDisposable disposable)
        {
            if (disposable == null) return;

            if (_disposed)
            {
                disposable.Dispose();
                Log.Warn(
                    "Attempted to add to a disposed CompositeDisposable. The new item was disposed immediately.",
                    ELogCategory.Utils);
                return;
            }
            _disposables.Add(disposable);
        }

        public void Add(GodotObject emitter, StringName signalName, Callable callable)
        {
            if (emitter == null)
            {
                Log.Warn($"Attempted to add to a null emitter {signalName}.", ELogCategory.Utils);
                return;
            }

            if (!emitter.HasSignal(signalName) && !emitter.HasUserSignal(signalName))
            {
                Log.Warn($"Signal not exists {signalName}", ELogCategory.Utils);
                return;
            }

            if (emitter.IsConnected(signalName, callable))
            {
                Log.Warn($"Signal already connected {signalName}", ELogCategory.Utils);
                return;
            }


            Add(new SignalSubscription(emitter, signalName, callable));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            for (int i = _disposables.Count - 1; i >= 0; i--)
            {
                try
                {
                    _disposables[i]?.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error disposing item: {ex.Message} {ex.StackTrace}", ELogCategory.Utils);
                }
            }
            _disposables.Clear();
        }
        #endregion

        public bool IgnoreWarnings = false;
        
        #region PRIVATE FIELDS
        private readonly List<IDisposable> _disposables = new();
        private bool _disposed;
        #endregion
    }
}