using System;
using System.Collections.Generic;
using Infrastructure.Services.UI;
using UnityEngine;

namespace Data.Configs
{
    [CreateAssetMenu(menuName = "Configs/Windows Config", fileName = "WindowsConfig")]
    /// <summary>
    /// Конфиг соответствия идентификаторов окон и их префабов.
    /// </summary>
    public class WindowsConfig : ScriptableObject
    {
        [Serializable]
        /// <summary>
        /// Запись о префабе окна для конкретного `WindowID`.
        /// </summary>
        public struct WindowRecord
        {
            public WindowID windowID;
            public GameObject prefab;
        }

        public List<WindowRecord> windows;
    }
}
