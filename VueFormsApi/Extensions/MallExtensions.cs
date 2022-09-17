using VueFormsApi.DataStructures.MallStructures;

namespace VueFormsApi.Extensions
{
    public static class MallExtensions
    {
        public static IServiceCollection UseMallDataProvider(this IServiceCollection services)
        {
            services.AddSingleton<IMall, Mall>();

            return services;
        }
    }
}
