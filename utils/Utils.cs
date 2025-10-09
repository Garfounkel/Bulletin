using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable IDE0130
namespace UnityBulletin
{
    /// <summary>
    /// Utility methods for the Bulletin system.
    /// </summary>
    public static class Utils
    {
        // ------------------------------------------------------------
        // GameObjectExtensions
        // ------------------------------------------------------------

        /// <summary>
        /// Extension method of GameObject that returns the full name of the game object.
        /// </summary>
        public static string Fullname(this GameObject obj)
        {
            return $"[{obj.scene.name}:{obj.name}]";
        }

        // ------------------------------------------------------------
        // MonoBehaviourExtensions
        // ------------------------------------------------------------

        /// <summary>
        /// Extension method of MonoBehaviour that invokes an action after a delay.
        /// </summary>
        /// <param name="f">The action to invoke.</param>
        /// <param name="delay">The delay in seconds.</param>
        public static void InvokeLambda(this MonoBehaviour mb, Action f, float delay)
        {
            mb.StartCoroutine(InvokeRoutine(f, delay));
        }

        /// <summary>
        /// Invokes an action after a delay.
        /// </summary>
        /// <param name="f">The action to invoke.</param>
        /// <param name="delay">The delay in seconds.</param>
        private static IEnumerator InvokeRoutine(Action f, float delay)
        {
            yield return new WaitForSeconds(delay);
            f();
        }

        // ------------------------------------------------------------
        // UnityEventExtensions
        // ------------------------------------------------------------

        /// <summary>
        /// Extension method of UnityEventBase that returns a list of persistent listeners.
        /// </summary>
        public static List<(UnityEngine.Object targetObject, string methodName)> GetPersistentListeners(this UnityEventBase unityEvent)
        {
            var list = new List<(UnityEngine.Object targetObject, string methodName)>();

            for (var i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            {
                var targetObject = unityEvent.GetPersistentTarget(i);
                var methodName = unityEvent.GetPersistentMethodName(i);
                list.Add((targetObject, methodName));
            }

            return list;
        }

        /// <summary>
        /// Extension method of UnityEventBase that returns a pretty printed string of the listeners.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        public static string PrettyPrintListeners(this UnityEventBase unityEvent, string eventName="")
        {
            var listeners = unityEvent.GetPersistentListeners();

            var sb = new StringBuilder();

            if (listeners.Count == 0)
                sb.AppendLine($"{eventName} ➔ [no listeners]");
            else if (listeners.Count == 1)
                sb.AppendLine($"{eventName} ➔ [{listeners[0].targetObject}.{listeners[0].methodName}]");
            else
            {
                sb.AppendLine($"{eventName} ({listeners.Count} listeners) ➔ [");
                foreach (var (targetObject, methodName) in listeners)
                    sb.AppendLine($"    {targetObject}.{methodName}");
                sb.AppendLine("]");
            }

            return sb.ToString();
        }
    }
}