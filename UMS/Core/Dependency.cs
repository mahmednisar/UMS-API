using Microsoft.Extensions.DependencyInjection;
using UMS.Services;
using UMS.Services.authentication;
using UMS.Services.Infrastructure;
using UMS.Services.Infrastructure.authentication;

namespace UMS.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Dependency
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceCollection"></param>
        public Dependency(IServiceCollection serviceCollection)
        {
            ResolveDependency(serviceCollection);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public void ResolveDependency(IServiceCollection services)
        {
            //Authentication Related Service Injection 
            services.AddTransient<IAuthService, AuthService>();


        }
    }
}