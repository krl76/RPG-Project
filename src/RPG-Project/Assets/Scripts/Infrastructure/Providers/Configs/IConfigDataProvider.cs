using Infrastructure.Services.UI;
using UnityEngine;

namespace Infrastructure.Providers.Configs
{
    public interface IConfigDataProvider
    {
        void Load();
        GameObject GetWindowPrefab(WindowID id);
    }
}