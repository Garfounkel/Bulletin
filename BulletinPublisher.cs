using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnitySharedFolder;

// ReSharper disable CheckNamespace
namespace UnityBulletin
{
    public class BulletinPublisher : MonoBehaviour
    {
        [Serializable]
        public struct TimedIssue
        {
            public float timestamp;
            public Issue issue;
        }

        public List<TimedIssue> timedIssues;

        [Button, HideInEditorMode]
        public void Publish()
        {
            foreach (var timedIssue in timedIssues)
                this.InvokeLambda(() => Bulletin.Publish(timedIssue.issue), timedIssue.timestamp);
        }
    }
}
