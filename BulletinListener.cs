using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


#pragma warning disable IDE0130
namespace UnityBulletin
{
    /// <summary>
    /// A component that subscribes UnityEvents to Bulletin issues. Bulletin then invokes UnityEvents as issues are published.
    /// Use this to setup reactions to Bulletin issues in the inspector.
    /// </summary>
    public class BulletinListener : MonoBehaviour
    {
        [Serializable]
        protected struct Reaction
        {
            public Issue issue;
            public UnityEvent action;
        }

        [SerializeField] private List<Reaction> reactions = default;

        /// <summary>
        /// Subscribes UnityEvents to Bulletin issues.
        /// </summary>
        private void OnEnable()
        {
            foreach (var reaction in reactions)
                Bulletin.Subscribe(reaction.issue, reaction.action, gameObject);
        }

        /// <summary>
        /// Unsubscribes UnityEvents from Bulletin issues.
        /// </summary>
        private void OnDisable()
        {
            foreach (var reaction in reactions)
                Bulletin.Unsubscribe(reaction.issue, reaction.action.Invoke);
        }
    }
}
