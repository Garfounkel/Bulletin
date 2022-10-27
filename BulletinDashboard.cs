#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnitySharedFolder;

// ReSharper disable CheckNamespace
namespace BulletinModule
{
    public class BulletinDashboard : EditorWindow
    {
        [MenuItem("Tools/Bulletin")]
        private static void Init()
        {
            var window = (BulletinDashboard)GetWindow(typeof(BulletinDashboard), false, "Bulletin");
            window.Show();
        }

        private Vector2 _scrollPosition = Vector2.zero;
        private string _searchText = "";
        private SearchField _searchField;
        private int _tab;

        private static GUIStyle _issueStyle;
        private static GUIStyle _publishStyle;
        private static GUIStyle _subscriberStyle;

        private void OnGUI()
        {
            _issueStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft };
            _publishStyle = new GUIStyle(EditorStyles.largeLabel) { alignment = TextAnchor.MiddleCenter };
            _subscriberStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight };

            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
                { alignment = TextAnchor.MiddleCenter, fontSize = 18 };

            GUILayout.Label($"{nameof(Bulletin)}", titleStyle);
            EditorGUILayout.Space(10);

            _tab = GUILayout.Toolbar(_tab, new[] { "Manual publish", "Trace listeners" });
            var regularTab = _tab == 0;
            if (regularTab) // Manual publish
            {
                Bulletin.debug = EditorGUILayout.ToggleLeft("Activate debug logs", Bulletin.debug);
            }
            else // Trace listeners
            {
                if (GUILayout.Button("Print subscribe/listen logs", GUILayout.ExpandWidth(false)))
                    Debug.Log(Bulletin.PrettyPrintSubscribeLogs());
            }

            EditorGUILayout.Space(10);

            var filteredIssues = SearchBox();

            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollView.scrollPosition;
                if (regularTab)
                {
                    foreach (var issue in filteredIssues)
                        IssueButton(issue);
                }
                else
                {
                    foreach (var issue in filteredIssues)
                        TraceIssueButton(issue);
                    Separator();
                }
            }
        }

        private static void Separator()
        {
            using (new GUILayout.VerticalScope("CN Box", GUILayout.MaxHeight(1)))
                GUILayout.Space(1);
        }

        private List<Issue> SearchBox()
        {
            if (_searchField == null)
                _searchField = new SearchField();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Filter issues");
                _searchText = _searchField.OnGUI(_searchText, GUILayout.ExpandWidth(true));
            }

            var issues = Enum.GetValues(typeof(Issue)).Cast<Issue>().ToList();
            if (!string.IsNullOrEmpty(_searchText))
                issues = issues.Where(issue => issue.ToString().ToLower().Contains(_searchText.ToLower())).ToList();

            return issues;
        }

        private static void IssueButton(Issue issue)
        {
            var r = EditorGUILayout.BeginHorizontal("Button");
            {
                if (GUI.Button(r, GUIContent.none))
                {
                    Bulletin.Publish(issue);
                    if (Bulletin.debug) PrintDebugSubscriber(issue);
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"{issue}", _issueStyle);
                        GUILayout.Label($"{Bulletin.InspectSubscribers(issue).Count} subscribers", _subscriberStyle);
                    }

                    GUILayout.Label("Publish", _publishStyle);
                    GUILayout.Label(" ");
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void TraceIssueButton(Issue issue)
        {
            Separator();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label($"{issue}", _issueStyle);
                GUILayout.Label($"{Bulletin.InspectSubscribers(issue).Count} subscribers", _subscriberStyle);
            }

            var listeners = Bulletin.InspectListeners(issue);
            if (listeners.Count > 0)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(20);
                    using (new EditorGUILayout.VerticalScope("textField"))
                    {
                        GUILayout.Label("Listeners (click to select)");
                        foreach (var subLog in listeners)
                        {
                            if (!subLog.gameObject) continue;
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                var content = new GUIContent(subLog.gameObject.Fullname(),
                                    "Select this gameobject");
                                if (GUILayout.Button(content, GUILayout.ExpandWidth(false)))
                                {
                                    EditorGUIUtility.PingObject(subLog.gameObject);
                                    Selection.activeObject = subLog.gameObject;
                                }
                            }
                        }
                    }
                }
            }

            GUILayout.Space(10);
        }

        private static void PrintDebugSubscriber(Issue issue)
        {
            var subscribers = Bulletin.InspectSubscribers(issue);
            if (subscribers.Count > 0)
            {
                var msg = $"{issue} issue has {subscribers.Count} subscribers: [\n";
                msg = subscribers.Aggregate(msg, (cur, a) => cur + $"  {a.Target}    -    {a.Method}\n");
                msg += "]\n";
                Debug.Log(msg);
            }
            else
                Debug.Log($"No subscribers for issue {issue}.");
        }
    }
}
#endif
