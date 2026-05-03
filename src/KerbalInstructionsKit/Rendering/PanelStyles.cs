using UnityEngine;

namespace KerbalInstructionsKit.Rendering
{
    public static class PanelStyles
    {
        public static GUIStyle Title { get; private set; }
        public static GUIStyle PageTitle { get; private set; }
        public static GUIStyle RichLabel { get; private set; }
        public static GUIStyle Caption { get; private set; }
        public static GUIStyle CategoryHeader { get; private set; }
        public static GUIStyle TocItem { get; private set; }
        public static GUIStyle TocItemActive { get; private set; }
        public static GUIStyle LinkButton { get; private set; }
        public static GUIStyle Foldout { get; private set; }

        private static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            Title = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 18,
                wordWrap = false,
            };

            PageTitle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 15,
                wordWrap = false,
            };

            RichLabel = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                wordWrap = true,
            };

            Caption = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Italic,
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
            };

            CategoryHeader = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
            };

            TocItem = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true,
            };
            TocItem.normal.textColor = Color.white;

            TocItemActive = new GUIStyle(TocItem)
            {
                fontStyle = FontStyle.Bold,
            };
            TocItemActive.normal.textColor = new Color(0.51f, 0.7f, 0.91f);

            LinkButton = new GUIStyle(GUI.skin.button)
            {
                richText = true,
                fontStyle = FontStyle.Normal,
            };
            LinkButton.normal.textColor = new Color(0.51f, 0.7f, 0.91f);

            Foldout = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 13,
            };
        }
    }
}
