using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Ty.Core.Builder
{
    internal class TyCoreBuilder : ITyCoreBuilder
    {
        private IServiceCollection _services;
        public IServiceCollection Services => _services;

        public TyCoreBuilder(IServiceCollection services)
        {
            this._services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}
