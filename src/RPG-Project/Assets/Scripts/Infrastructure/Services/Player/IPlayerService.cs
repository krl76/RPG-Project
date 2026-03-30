using UnityEngine;

namespace Infrastructure.Services.Player
{
    /// <summary>
    /// Сервис доступа к корневым объектам игрока в сцене.
    /// </summary>
    public interface IPlayerService
    {
        GameObject PlayerObject { get; }
        Transform PlayerTransform { get; }
        void InstallService();
    }
}
