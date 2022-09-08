using System;
using Microsoft.Extensions.DependencyInjection;
using PatHead.StorageService.Util.Model;
using PatHead.StorageService.Util.Services;

namespace PatHead.StorageService.Util.DependencyInjection
{
    public static class StorageServiceCollectionExtensions
    {
        public static void AddMinioStorageService(
            this IServiceCollection services,
            Action<StorageConfig> setupAction)
        {
            services.Configure(setupAction);
            services.AddTransient<StorageFactoryService>();
            services.AddTransient<IStorageService, MinioStorageService>();
        }
    }
}