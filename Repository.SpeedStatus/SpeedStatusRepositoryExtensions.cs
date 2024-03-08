// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Repository.SpeedStatus
{
    public static class SpeedStatusRepositoryExtensions
    {
        public static IServiceCollection AddSpeedStatusRepository(this IServiceCollection services)
        {
            services.AddScoped<ISpeedStatusRepository, SpeedStatusRepository>();

            return services;
        }
    }
}
