using Core.Gameplay.Save.Data;

namespace Infrastructure.Repositories.Save
{
    public interface IGameSaveRepository
    {
        bool HasSave();
        void Save(GameSaveData data);
        bool TryLoad(out GameSaveData data);
    }
}
