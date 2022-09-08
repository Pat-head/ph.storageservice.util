using System.Collections.Generic;

namespace PatHead.StorageService.Util.Services
{
    public class StorageFactoryService
    {
        private readonly IEnumerable<IStorageService> _storageServices;

        public StorageFactoryService(IEnumerable<IStorageService> storageServices)
        {
            _storageServices = storageServices;
        }

        public IStorageService GetStorageService<T>()
        {
            foreach (var storageService in _storageServices)
            {
                if (storageService is T)
                {
                    return storageService;
                }
            }

            return null;
        }
    }
}