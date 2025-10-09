using System;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif


#pragma warning disable IDE0130
namespace UnityBulletin
{
    /// <summary>
    /// Used to publish Bulletin issues through the inspector, you can assign this in UnityEvents and target Publish() to publish the issue when the event is triggered.
    /// </summary>
    public class BulletinPublisher : MonoBehaviour
    {
        [Serializable]
        public struct TimedIssue
        {
            public float timestamp;
            public Issue issue;
        }

        public List<TimedIssue> timedIssues;

        #if ODIN_INSPECTOR
        [Button, HideInEditorMode]
        #endif
        /// <summary>
        /// Publishes all issues at the corresponding timestamps.
        /// </summary>
        public void Publish()
        {
            foreach (var timedIssue in timedIssues)
                this.InvokeLambda(() => Bulletin.Publish(timedIssue.issue), timedIssue.timestamp);
        }
    }
}