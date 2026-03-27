namespace Core.Gameplay.Save
{
    public interface IGameSaveInteractor
    {
        bool SaveGame();
        bool PrepareLoadGame();
        void ApplyPendingGameState();
        void ClearPendingRestore();
        bool HasSave();
    }
}
