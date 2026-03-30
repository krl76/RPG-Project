using Core.Gameplay.Save.Data;

namespace Infrastructure.Repositories.Save
{
    /// <summary>
    /// Контракт репозитория, который читает и пишет файл сохранения.
    /// </summary>
    public interface IGameSaveRepository
    {
        bool HasSave();
        void Save(GameSaveData data);
        bool TryLoad(out GameSaveData data);
    }
}
