using UnityEngine;

namespace Infrastructure.Services.Player
{
    public interface IPlayerService
    {
        GameObject PlayerObject { get; }
        Transform PlayerTransform { get; }
        void InstallService();
    }
}