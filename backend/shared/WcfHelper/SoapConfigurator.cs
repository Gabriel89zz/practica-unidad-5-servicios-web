using Microsoft.AspNetCore.Builder;
using CoreWCF.Configuration;

namespace WcfHelper
{
    public static class SoapConfigurator
    {
        public static void ConfigureServiceModel(IApplicationBuilder app, Action<IServiceBuilder> configure)
        {
            app.UseServiceModel(configure);
        }
    }
}
