/// ------------------------------------------------------------------------------------------------
/// A component that plays animations as reactions to Bulletin issues.
///
/// It requires MyBox library (https://github.com/Deadcows/MyBox) for the AnimationStateReference class.
/// If you have MyBox installed, feel free to uncomment the code below. 
/// ------------------------------------------------------------------------------------------------

// using System;
// using System.Collections.Generic;
// using MyBox;
// using UnityEngine;
// using UnityEngine.Serialization;

// #pragma warning disable IDE0130
// namespace UnityBulletin
// {
//     /// <summary>
//     /// A component that plays animations as reactions to Bulletin issues.
//     ///
//     /// It requires MyBox library for the AnimationStateReference class.
//     /// </summary>
//     [RequireComponent(typeof(Animator))]
//     public class BulletinAnimatorListener : MonoBehaviour
//     {
//         [Serializable]
//         protected struct Reaction
//         {
//             public Issue issue;
//             public AnimationStateReference animation;
//         }

//         [SerializeField]
//         private List<Reaction> reactions = default;

//         private void OnEnable()
//         {
//             foreach (var reaction in reactions)
//                 Bulletin.Subscribe(reaction.issue, reaction.animation.Play);
//         }

//         private void OnDisable()
//         {
//             foreach (var reaction in reactions)
//                 Bulletin.Unsubscribe(reaction.issue, reaction.animation.Play);
//         }
//     }
// }
