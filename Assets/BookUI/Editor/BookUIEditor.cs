using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.UI.Extensions
{
    [CustomEditor(typeof(BookUI))]
    public class BookUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            BookUI book = target as BookUI;

            EditorGUILayout.LabelField("Resolution : " + book.Resolution.ToString());
            using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Current Page");
                    if (GUILayout.Button("--"))
                        book.CurrentPage--;
                    int page = EditorGUILayout.IntField(book.CurrentPage);
                    book.CurrentPage = page;
                    if (GUILayout.Button("++"))
                        book.CurrentPage++;
                }
            }

            using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
            {
                DrawDefaultInspector();
            }
        }
    }
}
