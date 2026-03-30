namespace Infrastructure.Services.Scene
{
    /// <summary>
    /// Контракт экрана загрузки.
    /// </summary>
    public interface ILoadingScreenService
    {
        void Show();
        void SetProgress(float progress);
        void Hide();
    }
}
