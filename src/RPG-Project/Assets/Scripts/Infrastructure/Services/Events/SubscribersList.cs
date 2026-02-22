using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Infrastructure.Services.Events
{
    internal class SubscribersList<TSubscriber> where TSubscriber : class, IGlobalSubscriber
    {
        private readonly List<WeakReference<TSubscriber>> _subscribers = new();
        private readonly object _lock = new();

        public void Add(TSubscriber subscriber)
        {
            lock (_lock)
            {
                _subscribers.Add(new WeakReference<TSubscriber>(subscriber));
            }
        }

        public void Remove(TSubscriber subscriber)
        {
            lock (_lock)
            {
                _subscribers.RemoveAll(weakReference =>
                    !weakReference.TryGetTarget(out var target) ||
                    ReferenceEquals(target, subscriber)
                );
            }
        }

        public void Execute(Action<TSubscriber> action)
        {
            List<TSubscriber> snapshot;
            lock (_lock)
            {
                snapshot = _subscribers
                    .Select(wr => wr.TryGetTarget(out var target) ? target : null)
                    .Where(s => s != null)
                    .ToList();
                
                _subscribers.RemoveAll(wr => !wr.TryGetTarget(out _));
            }

            foreach (var subscriber in snapshot)
            {
                try
                {
                    action.Invoke(subscriber);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[EventBus] Error performing action for subscriber {ex}");
                }
            }
        }
    }
}