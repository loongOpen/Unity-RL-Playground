using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.UIElements;

namespace GifImporter.Editors
{
    [CustomEditor(typeof(Gif))]
    public class GifEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var gif = (Gif)target;
            if (gif.Frames == null) return;

            GUILayout.Label(gif.Frames.Count + " frames.");

            foreach (var frame in gif.Frames)
            {
                if (frame == null) continue;
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(frame.Sprite,  typeof(Sprite), false);
                EditorGUILayout.IntField(frame.DelayInMs);
                EditorGUILayout.LabelField("ms", GUILayout.Width(30));
                GUILayout.EndHorizontal();
            }
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var gif = (Gif)target;
            if (gif.Frames != null
                && gif.Frames.Count > 0
                && gif.Frames[0].Sprite != null)
            {
                var tex = AssetPreview.GetAssetPreview(gif.Frames[0].Sprite);
                if (tex != null)
                {
                    var newTex = Instantiate(tex);
                    return newTex;
                }
            }

            return null;
        }
    }
}