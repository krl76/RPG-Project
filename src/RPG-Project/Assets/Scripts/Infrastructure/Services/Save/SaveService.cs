using Infrastructure.Providers;

namespace Infrastructure.Services.Save
{
    public class SaveService : ISaveService
    {
        private readonly IDataProvider _dataProvider;
        
        public SaveService(IDataProvider dataProvider) // все сервисы которые должны что то сохранять
        {
            _dataProvider = dataProvider;
        }
        
        public void SaveData()
        {
            // в каждом сервисе вызвать метод, который обратится к DataProvider с нужным ему методом
            
            _dataProvider.SaveData();
        }

        public void LoadData()
        {
            // аналогично только методы загрузки
            
            _dataProvider.LoadData();
        }
    }
}