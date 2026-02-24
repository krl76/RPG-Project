using UnityEngine;

namespace prototype_Roma.Scripts
{
    public interface IPlayerService
    {
        GameObject PlayerObject { get; }
        Transform PlayerTransform { get; }
        void InstallService();
    }
}