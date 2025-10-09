using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
using UnityEngine.Events;


#pragma warning disable IDE0130
namespace UnityBulletin
{
    /// <summary>
    /// Bulletin main class.
    ///
    /// It allows you to publish issues and subscribe to them.
    ///
    /// It is a lightweight system that can be used to manage events in your game.
    ///
    /// Access it from anywhere with Bulletin.Publish(Issue.IssueName) and Bulletin.Subscribe(Issue.IssueName, Action).
    /// </summary>
    public static class Bulletin
    {
        /// <summary>
        /// Whether to enable debug mode.
        /// </summary>
        public static bool debug = false;

        private static Dictionary<Issue, ThreadSafeList<Action>> _issues;

        private static Dictionary<Issue, ThreadSafeList<SubscribeLog>> _subscribeLogs;

        static Bulletin() => Init();

        /// <summary>
        /// Initialize the Bulletin system when automatic domain reload is disabled.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Init()
        {
            _issues = new Dictionary<Issue, ThreadSafeList<Action>>();
            _subscribeLogs = new Dictionary<Issue, ThreadSafeList<SubscribeLog>>();
        }

        /// <summary>
        /// Publish an issue.
        /// </summary>
        public static void Publish(Issue issue)
        {
            if (_issues.TryGetValue(issue, out var actions))
            {
                foreach (var action in actions)
                    action();
            }
        }

        /// <summary> Subscribe an action to an issue. The action will be invoked whenever the issue is published. </summary>
        /// <remarks> Prefer the UnityEvent overload as it tracks additional debug infos. </remarks>
        public static void Subscribe(Issue issue, Action action)
        {
            if (_issues.TryGetValue(issue, out var actions))
                actions.Add(action);
            else
                _issues.Add(issue, new ThreadSafeList<Action> { action });
        }

        /// <summary>
        /// Subscribe an action to an issue. The action will be invoked whenever the issue is published.
        /// Tracks additional debug infos like the caller game object and the list of listeners.
        /// </summary>
        /// <remarks> Prefer this overload over the default Subscribe(Issue, Action) which doesn't track debug infos.</remarks>
        public static void Subscribe(Issue issue, UnityEvent unityEvent, GameObject caller)
        {
            var subLog = new SubscribeLog(caller, issue, unityEvent.PrettyPrintListeners());
            if (_subscribeLogs.TryGetValue(issue, out var logs))
                logs.Add(subLog);
            else
                _subscribeLogs.Add(issue, new ThreadSafeList<SubscribeLog> { subLog });

            Subscribe(issue, unityEvent.Invoke);
        }

        /// <summary>
        /// Unsubscribe an action from an issue.
        /// </summary>
        public static void Unsubscribe(Issue issue, Action action)
        {
            if (_issues.TryGetValue(issue, out var actions))
            {
                if (!actions.Remove(action) && debug)
                    Debug.Log($"Action [{action.Target}    -    {action.Method}] not found " +
                              $"for issue {issue}. Failed to unsubscribe.");
            }
        }

        /// <summary>
        /// Inspect the subscribers for an issue.
        /// </summary>
        public static ReadOnlyCollection<Action> InspectSubscribers(Issue issue)
        {
            if (_issues.TryGetValue(issue, out var actions))
                return actions.AsReadOnly();
            return new List<Action>().AsReadOnly();
        }

        /// <summary>
        /// Inspect the listeners for an issue.
        /// </summary>
        public static ReadOnlyCollection<SubscribeLog> InspectListeners(Issue issue)
        {
            if (_subscribeLogs.TryGetValue(issue, out var subLog))
                return subLog.AsReadOnly();
            return new List<SubscribeLog>().AsReadOnly();
        }

        /// <summary>
        /// Pretty print the subscribe logs.
        /// </summary>
        public static string PrettyPrintSubscribeLogs()
        {
            if (_subscribeLogs.Count == 0) return "No subscribe logs recorded.";

            var sb = new StringBuilder();

            sb.AppendLine($"Issue ➔ {{");
            sb.AppendLine($"[Scene:GameObject] ➔ [ListenerGameObject (Script).Method]");
            sb.AppendLine($"}}");
            sb.AppendLine($"---");
            sb.AppendLine();

            foreach (var keyValuePair in _subscribeLogs)
            {
                var issue = keyValuePair.Key;
                sb.AppendLine($"{issue} ➔ {{");
                foreach (var subLog in keyValuePair.Value)
                    sb.Append($"{subLog.gameObject.Fullname()}{subLog.subscribers}");
                sb.AppendLine($"}}");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Helper class to store the subscribe logs.
    /// </summary>
    public class SubscribeLog
    {
        public GameObject gameObject;
        public Issue issue;
        public string subscribers;

        public SubscribeLog(GameObject gameObject, Issue issue, string subscribers)
        {
            this.gameObject = gameObject;
            this.issue = issue;
            this.subscribers = subscribers;
        }
    }

    /// <summary>
    /// Basic thread safe list implementation.
    /// </summary>
    internal class ThreadSafeList<T> : IEnumerable<T>
    {
        protected List<T> _internalList = new();

        public IEnumerator<T> GetEnumerator() => Clone().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Clone().GetEnumerator();

        protected static object _lock = new();

        public List<T> Clone()
        {
            List<T> newList = new();

            lock (_lock)
            {
                _internalList.ForEach(x => newList.Add(x));
            }

            return newList;
        }

        public void Add(T value) => _internalList.Add(value);
        public bool Remove(T item) => _internalList.Remove(item);
        public ReadOnlyCollection<T> AsReadOnly() => _internalList.AsReadOnly();
    }
}
