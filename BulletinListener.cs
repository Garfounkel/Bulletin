using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnitySharedFolder;

// ReSharper disable CheckNamespace
namespace UnityBulletin
{
    public class BulletinListener : MonoBehaviour
    {
        [Serializable]
        protected struct Reaction
        {
            public Issue issue;
            public UnityEvent action;
        }

        [SerializeField] private List<Reaction> reactions = default;

        private void OnEnable()
        {
            foreach (var reaction in reactions)
                Bulletin.Subscribe(reaction.issue, reaction.action, gameObject);
        }

        private void OnDisable()
        {
            foreach (var reaction in reactions)
                Bulletin.Unsubscribe(reaction.issue, reaction.action.Invoke);
        }
    }
}
