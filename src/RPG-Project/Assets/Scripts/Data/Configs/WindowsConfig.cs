using System;
using System.Collections.Generic;
using Infrastructure.Services.UI;
using UnityEngine;

namespace Data.Configs
{
    [CreateAssetMenu(menuName = "Configs/Windows Config", fileName = "WindowsConfig")]
    public class WindowsConfig : ScriptableObject
    {
        [Serializable]
        public struct WindowRecord
        {
            public WindowID windowID;
            public GameObject prefab;
        }

        public List<WindowRecord> windows;
    }
}