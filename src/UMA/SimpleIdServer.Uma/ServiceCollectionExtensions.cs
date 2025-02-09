﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleIdServer.OAuth.Api.Configuration;
using SimpleIdServer.OAuth.Api.Token.Handlers;
using SimpleIdServer.OAuth.Domains;
using SimpleIdServer.OAuth.Options;
using SimpleIdServer.Uma;
using SimpleIdServer.Uma.Api.Configuration;
using SimpleIdServer.Uma.Api.Token.Fetchers;
using SimpleIdServer.Uma.Api.Token.Handlers;
using SimpleIdServer.Uma.Api.Token.Validators;
using SimpleIdServer.Uma.Domains;
using SimpleIdServer.Uma.Helpers;
using SimpleIdServer.Uma.Persistence;
using SimpleIdServer.Uma.Persistence.InMemory;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register UMA2.0 dependencies.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static SimpleIdServerUmaBuilder AddSIDUma(this IServiceCollection services, bool isStandalone = true)
        {
            var builder = new SimpleIdServerUmaBuilder(services);
            if (isStandalone)
            {
                services.AddSIDOAuth()
                    .AddScopes(new List<OAuthScope> { UMAConstants.StandardUMAScopes.UmaProtection });
            }

            services.AddUMAStore()
                .AddUMATokenApi()
                .AddUMAHelpers()
                .AddUMAConfiguration();
            return builder;
        }

        /// <summary>
        /// Register UMA2.0 dependencies.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static SimpleIdServerUmaBuilder AddSIDUma(this IServiceCollection services, Action<UMAHostOptions> umaOptions, Action<OAuthHostOptions> oauthOptions = null, bool isStandalone = true)
        {
            services.Configure(umaOptions);
            if (oauthOptions != null)
            {
                services.Configure(oauthOptions);
            }

            return services.AddSIDUma(isStandalone);
        }

        private static IServiceCollection AddUMAStore(this IServiceCollection services)
        {
            var umaResources = new List<UMAResource>();
            var umaPendingRequests = new List<UMAPendingRequest>();
            services.TryAddSingleton<IUMAResourceRepository>(new DefaultUMAResourceRepository(umaResources));
            services.TryAddSingleton<IUMAPendingRequestRepository>(new DefaultUMAPendingRequestRepository(umaPendingRequests));
            return services;
        }

        private static IServiceCollection AddUMATokenApi(this IServiceCollection services)
        {
            services.AddTransient<IClaimTokenFormat, OpenIDClaimTokenFormat>();
            services.AddTransient<IGrantTypeHandler, UmaTicketHandler>();
            services.AddTransient<IUmaTicketGrantTypeValidator, UmaTicketGrantTypeValidator>();
            return services;
        }

        private static IServiceCollection AddUMAHelpers(this IServiceCollection services)
        {
            services.AddTransient<IUMAPermissionTicketHelper, UMAPermissionTicketHelper>();
            return services;
        }

        private static IServiceCollection AddUMAConfiguration(this IServiceCollection services)
        {
            services.RemoveAll<IConfigurationRequestHandler>();
            services.AddTransient<IConfigurationRequestHandler, UMAConfigurationRequestHandler>();
            return services;
        }
    }
}