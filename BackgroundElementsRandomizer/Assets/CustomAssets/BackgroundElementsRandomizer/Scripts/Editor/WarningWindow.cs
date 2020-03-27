using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nolanfa.BackgroundElementsRandomizer
{
    public class WarningWindow : EditorWindow
    {
        void ShowWarningWindow(string title)
        {
            WarningWindow window = (WarningWindow) EditorWindow.GetWindow<WarningWindow>(title:title, focus:true);
            window.ShowPopup();
            window.title = title;
        }

        Rect OKButton;
        Rect CancelButton;

        private void OnGUI()
        {

        }

    }
}
