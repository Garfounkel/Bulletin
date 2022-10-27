using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnitySharedFolder;

// ReSharper disable CheckNamespace
namespace UnityBulletin
{
    public static class Bulletin
    {
        public static bool debug = false;

        private static Dictionary<Issue, ThreadSafeList<Action>> _issues;

        private static Dictionary<Issue, ThreadSafeList<SubscribeLog>> _subscribeLogs;

        static Bulletin() => Init();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Init()
        {
            _issues = new Dictionary<Issue, ThreadSafeList<Action>>();
            _subscribeLogs = new Dictionary<Issue, ThreadSafeList<SubscribeLog>>();
        }

        public static void Publish(Issue issue)
        {
            if (_issues.TryGetValue(issue, out var actions))
            {
                foreach (var action in actions)
                    action();
            }
        }

        /// <summary> Subscribe an action to an issue. The action will be invoked whenever the issue is published. </summary>
        /// <remarks> Prefer the UnityEvent overload as it also tracks useful debug infos contrary to this one. </remarks>
        public static void Subscribe(Issue issue, Action action)
        {
            if (_issues.TryGetValue(issue, out var actions))
                actions.Add(action);
            else
                _issues.Add(issue, new ThreadSafeList<Action> { action });
        }

        /// <summary>
        /// Subscribe an action to an issue. The action will be invoked whenever the issue is published.
        /// Also tracks useful debug infos.
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

        public static void Unsubscribe(Issue issue, Action action)
        {
            if (_issues.TryGetValue(issue, out var actions))
            {
                if (!actions.Remove(action) && debug)
                    Debug.Log($"Action [{action.Target}    -    {action.Method}] not found " +
                              $"for issue {issue.ToString()}. Failed to unsubscribe.");
            }
        }

        public static ReadOnlyCollection<Action> InspectSubscribers(Issue issue)
        {
            if (_issues.TryGetValue(issue, out var actions))
                return actions.AsReadOnly();
            return new List<Action>().AsReadOnly();
        }

        public static ReadOnlyCollection<SubscribeLog> InspectListeners(Issue issue)
        {
            if (_subscribeLogs.TryGetValue(issue, out var subLog))
                return subLog.AsReadOnly();
            return new List<SubscribeLog>().AsReadOnly();
        }

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

    internal class ThreadSafeList<T> : IEnumerable<T>
    {
        protected List<T> _internalList = new List<T>();

        public IEnumerator<T> GetEnumerator() => Clone().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Clone().GetEnumerator();

        protected static object _lock = new object();

        public List<T> Clone()
        {
            List<T> newList = new List<T>();

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
