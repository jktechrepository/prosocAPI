using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using Prosoc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Logging;
using FluentValidation;
using FluentValidation.AspNetCore;
using StackExchange.Redis;

namespace ProsocAPI.Extensions
{
    /// <summary>
    /// Extensions pour la configuration des services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Ajoute les services de pagination
        /// </summary>
        public static IServiceCollection AddPaginationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuration des options de pagination
            services.Configure<PaginationOptions>(options =>
            {
                configuration.GetSection("Pagination").Bind(options);
                
                // Valeurs par défaut si non configurées
                if (options.DefaultPageSize <= 0)
                    options.DefaultPageSize = 20;
                
                if (options.MaxPageSize <= 0)
                    options.MaxPageSize = 100;
                
                if (options.MaxSearchResults <= 0)
                    options.MaxSearchResults = 1000;
                
                if (options.CacheDurationSeconds <= 0)
                    options.CacheDurationSeconds = 300;
            });

            // Ajout du service de pagination
            services.AddScoped<IPaginationService, PaginationService>();

            return services;
        }

        /// <summary>
        /// Ajoute les services de pagination avec configuration personnalisée
        /// </summary>
        public static IServiceCollection AddPaginationServices(this IServiceCollection services, Action<PaginationOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddScoped<IPaginationService, PaginationService>();

            return services;
        }

        /// <summary>
        /// Ajoute les services de validation avec FluentValidation
        /// </summary>
        public static IServiceCollection AddValidationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<Program>();
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();

            return services;
        }

        /// <summary>
        /// Ajoute les services de cache distribué
        /// </summary>
        public static IServiceCollection AddDistributedCacheServices(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("RedisConnection");
            
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "ProsocAPI:";
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            return services;
        }

        /// <summary>
        /// Ajoute les services de monitoring et health checks
        /// </summary>
        public static IServiceCollection AddMonitoringServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Health Checks
            services.AddHealthChecks();
            // services.AddHealthChecks().AddDbContextCheck<ProsocDbContext>(); // Commenté - package manquant

            // Rate Limiting
            // services.AddRateLimiter(options => { }); // Commenté - package manquant

            return services;
        }

        /// <summary>
        /// Configure les options de CORS
        /// </summary>
        public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" };
            var allowedMethods = configuration.GetSection("Cors:AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
            var allowedHeaders = configuration.GetSection("Cors:AllowedHeaders").Get<string[]>() ?? new[] { "*" };

            services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .WithMethods(allowedMethods)
                          .WithHeaders(allowedHeaders)
                          .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                });

                // Politique pour les requêtes API
                options.AddPolicy("ApiPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                          .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With")
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                });
            });

            return services;
        }

        /// <summary>
        /// Ajoute les services de compression de réponse
        /// </summary>
        public static IServiceCollection AddResponseCompressionServices(this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                // options.Providers.Add<BrotliCompressionProvider>(); // Commenté - package manquant
                // options.Providers.Add<GzipCompressionProvider>(); // Commenté - package manquant
                options.MimeTypes = new[]
                {
                    "application/json",
                    "application/javascript",
                    "text/css",
                    "text/html",
                    "text/plain",
                    "text/xml"
                };
            });

            // services.Configure<BrotliCompressionProviderOptions>(options => // Commenté - package manquant
            // {
            //     options.Level = CompressionLevel.Fastest;
            // });

            // services.Configure<GzipCompressionProviderOptions>(options => // Commenté - package manquant
            // {
            //     options.Level = CompressionLevel.Fastest;
            // });

            return services;
        }

        /// <summary>
        /// Ajoute les services d'API Versioning
        /// </summary>
        public static IServiceCollection AddApiVersioningServices(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Version"),
                    new QueryStringApiVersionReader("version"));
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
    }
}
