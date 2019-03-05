using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Ty.Core.Builder;
using Ty.Core.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Ty.Core.Data;

namespace Ty.Core.DependencyInjection
{
    public static class TyCoreServiceCollectionExtensions
    {
        public static ITyCoreBuilder AddTyCore(this IServiceCollection services)
        {
            var builder = new TyCoreBuilder(services);

            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<TyCoreOptions>, TyCoreOptionsSetup>());
            builder.Services.AddSingleton<IDataProxy, DataProxy>();

            return builder;
        }

        public static ITyCoreBuilder AddTyCore(this IServiceCollection services, Action<TyCoreOptions> setupAction)
        {
            var builder = services.AddTyCore();

            builder.Services.Configure(setupAction);

            return builder;
        }
    }
}
