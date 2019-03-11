using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Ty.Core.Builder
{
    public interface ITyCoreBuilder
    {
        IServiceCollection Services { get; }
    }
}
