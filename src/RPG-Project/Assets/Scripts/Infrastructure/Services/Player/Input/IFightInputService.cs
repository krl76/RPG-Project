namespace Infrastructure.Services.Player.Input
{
    public interface IFightInputService
    {
        public void InstallService();
        public void UninstallService();
        public void AttackEnd();
    }
}