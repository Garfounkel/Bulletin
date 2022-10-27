using System;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable CheckNamespace
namespace BulletinModule
{
    [RequireComponent(typeof(Animator))]
    public class BulletinAnimatorListener : MonoBehaviour
    {
        [Serializable]
        protected struct Reaction
        {
            public Issue issue;
            public AnimationStateReference animation;
        }

        [SerializeField, FormerlySerializedAs("reactions2")]
        private List<Reaction> reactions = default;

        private void OnEnable()
        {
            foreach (var reaction in reactions)
                Bulletin.Subscribe(reaction.issue, reaction.animation.Play);
        }

        private void OnDisable()
        {
            foreach (var reaction in reactions)
                Bulletin.Unsubscribe(reaction.issue, reaction.animation.Play);
        }
    }
}
