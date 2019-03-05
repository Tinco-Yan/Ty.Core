using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ty.Core.DependencyInjection
{
    public static class TyCoreApplicationBuilderExtensions
    {
        public static IApplicationBuilder UserTyCore(this IApplicationBuilder app)
        {
            return app;
        }
    }
}
