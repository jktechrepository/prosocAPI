using ProsocAPI.Services.Synchronization;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Middleware;
using ProsocAPI.Services.Queue;
using ProsocAPI.Services.Mobile;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Data;
using ProsocAPI.Hubs;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Configuration Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();
        builder.Services.AddSignalR(); // Ajout de SignalR
builder.Services.AddEndpointsApiExplorer();

// ✅ AJOUT : HttpContextAccessor pour ErrorService
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<ProsocDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ProsocConnection"),
        new MariaDbServerVersion(new Version(10, 6))
    ));

// Repositories / Services
builder.Services.AddScoped<ICategorieAdhesionRepository, CategorieAdhesionService>();
builder.Services.AddScoped<ITypeAdhesionRepository, TypeAdhesionService>();
builder.Services.AddScoped<ITarifCotisationRepository, TarifCotisationService>();
builder.Services.AddScoped<ITarifCotisationMetierService, TarifCotisationMetierService>();
builder.Services.AddScoped<ICotisationAffilieRepository>(sp => (ICotisationAffilieRepository)sp.GetRequiredService<ITarifCotisationRepository>());
builder.Services.AddScoped<ICotisationAffilieMetierService>(sp => (ICotisationAffilieMetierService)sp.GetRequiredService<ITarifCotisationMetierService>());
builder.Services.AddScoped<IAffilieRepository, AffilieService>();
builder.Services.AddScoped<IAdhesionRepository, AdhesionService>();
builder.Services.AddScoped<IDependantRepository, DependantService>();

builder.Services.AddScoped<IUtilisateurRepository, UtilisateurService>();
builder.Services.AddScoped<IRoleRepository, RoleService>();
builder.Services.AddScoped<IPermissionRepository, PermissionService>();

// 🆕 SERVICES DE SYNCHRONISATION
builder.Services.AddScoped<IUserSynchronizationService, UserSynchronizationService>();
builder.Services.AddScoped<IUserDeviceRepository, UserDeviceService>();

// Services d'authentification améliorés
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenService>();
builder.Services.AddScoped<EnhancedAuthService>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IParametresMetierProvider, ParametresMetierProvider>();

// Services additionnels
builder.Services.Configure<AgentMaashOptions>(
    builder.Configuration.GetSection(AgentMaashOptions.SectionName));
builder.Services.Configure<RetraitAgentOptions>(
    builder.Configuration.GetSection(RetraitAgentOptions.SectionName));
builder.Services.AddScoped<ICaisseService, CaisseService>();
builder.Services.AddScoped<IPerceptionVirtuelleService, PerceptionVirtuelleService>();
builder.Services.AddScoped<IPerceptionVirtuelleExportService, PerceptionVirtuelleExportService>();
builder.Services.AddScoped<IAgentMaashRetenueService, AgentMaashRetenueService>();
builder.Services.AddHostedService<AgentMaashRetenueBackgroundService>();
builder.Services.AddScoped<IAgentRepository, AgentService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IDeviseRepository, DeviseService>();
builder.Services.AddScoped<IProvinceRepository, ProvinceService>();
builder.Services.AddScoped<IPrestationRepository, PrestationService>();
builder.Services.AddScoped<IAssureurRepository, AssureurService>();
builder.Services.AddScoped<IProduitAssureurRepository, ProduitAssureurService>();
builder.Services.AddScoped<ICollecteRepository, CollecteService>();
builder.Services.AddScoped<IProduitMutuelRepository, ProduitMutuelService>();
builder.Services.Configure<BonEnvoiQrOptions>(
    builder.Configuration.GetSection(BonEnvoiQrOptions.SectionName));
builder.Services.AddScoped<IBonEnvoiQrCodeService, BonEnvoiQrCodeService>();
builder.Services.AddScoped<IBonEnvoiRepository, BonEnvoiService>();
builder.Services.AddScoped<BonEnvoiService>();
builder.Services.AddScoped<IAntecedentRepository, AntecedentService>();
builder.Services.AddScoped<IRetraitAgentRepository, RetraitAgentService>();
builder.Services.AddScoped<ITargetAgentRepository, TargetAgentService>();
builder.Services.AddScoped<ICategorieAgentRepository, CategorieAgentService>();
builder.Services.AddScoped<IMatriculeGeneratorService, MatriculeGeneratorService>();
builder.Services.AddScoped<ICodeAdhesionGeneratorService, CodeAdhesionGeneratorService>();
builder.Services.AddScoped<IZoneSocialeRepository, ZoneSocialeService>();
builder.Services.AddScoped<ICommuneRepository, CommuneService>();
builder.Services.AddScoped<ITerritorialEncadrementService, TerritorialEncadrementService>();

// Service de jetons médicaux
builder.Services.AddScoped<IJetonMedicalRepository, JetonMedicalService>();

// Service de demandes de bon d'envoi
builder.Services.AddScoped<IDemandeBonEnvoiRepository, DemandeBonEnvoiService>();
builder.Services.AddScoped<DemandeBonEnvoiService>();

// Service de retrait agent
builder.Services.AddScoped<IRetraitAgentRepository, RetraitAgentService>();
builder.Services.AddScoped<IDemandeRetraitAgentRepository, RetraitAgentService>();
builder.Services.AddScoped<RetraitAgentService>();

// Service de wallet agent
builder.Services.AddScoped<IWalletAgentRepository, WalletAgentService>();

// Service de souscription prestation
builder.Services.AddScoped<ISouscriptionPrestationRepository, SouscriptionPrestationService>();
builder.Services.AddScoped<ISouscriptionPrestationAchatService, SouscriptionPrestationAchatService>();

// Dashboard Admin Service
builder.Services.AddScoped<IDashboardAdminRepository, DashboardAdminService>();

// Dashboard SuperAdmin Service
builder.Services.AddScoped<IDashboardSuperAdminRepository, DashboardSuperAdminService>();

// Dashboard Assureur Service
builder.Services.AddScoped<IDashboardAssureurRepository, DashboardAssureurService>();

// Dashboard Agent AA (encodeur)
builder.Services.AddScoped<IDashboardAgentAARepository, DashboardAgentAAService>();

// Dashboard Agent Hôpital
builder.Services.AddScoped<IDashboardAgentHopitalRepository, DashboardAgentHopitalService>();

// Dashboard Agent Service
builder.Services.AddScoped<IDashboardAgentRepository, DashboardAgentService>();
builder.Services.AddScoped<IDashboardChefEquipeRepository, DashboardChefEquipeService>();

// Dashboard Affilie Service
builder.Services.AddScoped<IDashboardAffilieRepository, DashboardAffilieService>();

// Dashboard Superviseur Service
builder.Services.AddScoped<ISuperviseurRepository, SuperviseurService>();

// Dashboard Percepteur Service
builder.Services.AddScoped<IDashboardPercepteurRepository, DashboardPercepteurService>();

// Dashboard Caissier Service
builder.Services.AddScoped<IDashboardCaissierRepository, DashboardCaissierService>();

// Dashboard Financier Service
builder.Services.AddScoped<IDashboardFinancierRepository, DashboardFinancierService>();

// Service de données géographiques
builder.Services.AddScoped<IGeographicDataService, GeographicDataService>();

// Service de commission
builder.Services.AddScoped<ICommissionService, CommissionService>();
builder.Services.AddScoped<IWalletVirtuelMouvementService, WalletVirtuelMouvementService>();
builder.Services.AddScoped<IWalletVirtuelPaymentService, WalletVirtuelPaymentService>();
builder.Services.AddScoped<ITypeAdhesionDependantsValidationService, TypeAdhesionDependantsValidationService>();

// Service de paiement affilié
builder.Services.AddScoped<IPaiementAffilieService, PaiementAffilieService>();

// Service de gestion des arriérés affilié
builder.Services.Configure<ArrieresOptions>(
    builder.Configuration.GetSection(ArrieresOptions.SectionName));
builder.Services.Configure<PenaliteOptions>(
    builder.Configuration.GetSection(PenaliteOptions.SectionName));
builder.Services.Configure<MultideviseOptions>(
    builder.Configuration.GetSection(MultideviseOptions.SectionName));
builder.Services.AddScoped<IDeviseConversionService, DeviseConversionService>();
builder.Services.AddScoped<ICollecteMultideviseService, CollecteMultideviseService>();

builder.Services.Configure<ErrorHandlingOptions>(
    builder.Configuration.GetSection(ErrorHandlingOptions.SectionName));

builder.Services.Configure<FlexPayOptions>(
    builder.Configuration.GetSection(FlexPayOptions.SectionName));
builder.Services.AddHttpClient("FlexPay");
builder.Services.AddScoped<IFlexPayService, FlexPayService>();
builder.Services.AddScoped<IInfoPaiementMarchandService, InfoPaiementMarchandService>();
builder.Services.AddScoped<IPaiementHoldService, PaiementHoldService>();
builder.Services.AddScoped<IFlexPayFinalizationService, FlexPayFinalizationService>();
builder.Services.AddScoped<IFlexPayCollecteService, FlexPayCollecteService>();
builder.Services.AddScoped<IFlexPayPaiementAffilieService, FlexPayPaiementAffilieService>();
builder.Services.AddScoped<IFlexPayCallbackService, FlexPayCallbackService>();
builder.Services.AddScoped<IFlexPayRealtimeNotificationService, FlexPayRealtimeNotificationService>();
builder.Services.AddScoped<IAdhesionWithAffilieExecutorService, AdhesionWithAffilieExecutorService>();
builder.Services.AddScoped<IFlexPayAdhesionService, FlexPayAdhesionService>();
builder.Services.AddScoped<IFlexPaySouscriptionAchatService, FlexPaySouscriptionAchatService>();

builder.Services.AddScoped<IArrieresAffilieService, ArrieresAffilieService>();
builder.Services.AddScoped<IAffilieConformiteService, AffilieConformiteService>();
builder.Services.AddScoped<IPenaliteAffilieService, PenaliteAffilieService>();
builder.Services.AddHostedService<ArrieresGenerationBackgroundService>();

// Service de pagination universelle
builder.Services.AddScoped<IPaginationService, PaginationService>();

// ✅ AJOUT : Service de gestion des erreurs structurées
builder.Services.AddScoped<ErrorService>();

// Service email
builder.Services.AddScoped<IEmailService, EmailService>();

// Service SMS
builder.Services.AddScoped<ISmsService, SmsService>();

// Service Push Notifications
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();

// Service de notifications unifié
builder.Services.AddScoped<INotificationService, NotificationService>();

// Service de notification de commissions
builder.Services.AddScoped<ICommissionNotificationService, CommissionNotificationService>();

// Service de dashboard des commissions
builder.Services.AddScoped<ICommissionDashboardService, CommissionDashboardService>();
builder.Services.AddScoped<IStatistiquesService, StatistiquesService>();

// Service de queue de notifications (Hosted Service)
builder.Services.AddSingleton<INotificationQueueService, NotificationQueueService>();
builder.Services.AddHostedService<NotificationQueueService>();

// Service de types de notifications
builder.Services.AddScoped<INotificationTypeService, NotificationTypeService>();

// Service de l'application mobile
builder.Services.AddScoped<IMobileAppServiceSimple, MobileAppServiceSimple>();

// Service des frais
builder.Services.AddScoped<IFraisService, FraisService>();

// Service de mise à jour des permissions
builder.Services.AddScoped<UpdatePermissionsService>();

// Service d'authentification (compatibilité)
builder.Services.AddScoped<IAuthService, EnhancedAuthService>();

// JWT Authentication (Configuration améliorée)
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // En développement uniquement
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Élimine le décalage horaire pour une expiration précise
    };
});

builder.Services.AddAuthorization();

// Swagger (JWT + multipart IFormFile)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PROSOC API", Version = "v1" });

    // Évite "An item with the same key has already been added. Key: ContentType" avec plusieurs IFormFile
    c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
    c.OperationFilter<ProsocAPI.Swagger.MultipartFormFileOperationFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configuration CORS améliorée
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                // En développement, permettre toutes les origines
                policy.SetIsOriginAllowed(origin => true)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
            else
            {
                // PRODUCTION : Configuration CORS complète et sécurisée
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                
                if (allowedOrigins != null && allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .WithHeaders(
                              "Content-Type",
                              "Authorization",
                              "Accept",
                              "Origin",
                              "X-Requested-With",
                              "Cache-Control",  //  AJOUTÉ pour le web
                              "Pragma",         //  AJOUTÉ pour le web
                              "Expires"         //  AJOUTÉ pour le web
                          )
                          .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromMinutes(10)); // Cache des réponses preflight
                }
                else
                {
                    // ❌ SÉCURITÉ CRITIQUE : Ne PAS autoriser toutes les origines en production
                    throw new InvalidOperationException(
                        "❌ ERREUR CRITIQUE : Cors:AllowedOrigins DOIT être configuré en production !\n" +
                        "Ajoutez dans appsettings.Production.json :\n" +
                        "{\n" +
                        "  \"Cors\": {\n" +
                        "    \"AllowedOrigins\": [\"https://testprosoc.kansaconsulting.com\"]\n" +
                        "  }\n" +
                        "}"
                    );
                }
            }
        });
});

var app = builder.Build();

// Exceptions non gérées uniquement — les catch existants dans les contrôleurs restent inchangés
app.UseGlobalExceptionHandler();

// Middleware
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prosoc API v1");
        c.RoutePrefix = "swagger";
        c.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Middleware d'initialisation des données géographiques (DÉSACTIVÉ temporairement)
// app.UseMiddleware<GeographicDataInitializationMiddleware>();

// Middleware pour gérer les tokens avec ou sans "Bearer"
app.UseFlexibleToken();

app.UseAuthentication();
app.UseAuthorization();

// Configuration SignalR
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<FlexPayHub>("/flexPayHub");

app.MapControllers();

// Database migration and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");

        // 🌱 APPLIQUER LE SEED DATA (FORCER RÉINITIALISATION)
        await SeedData.InitializeAsync(context, logger, forceReset: false);

        // Vérification des données (pour le debug)
        var typeAdhesions = context.TypeAdhesions.AsNoTracking().OrderBy(x => x.IdTypeAdhesion).ToList();
        logger.LogInformation("Seed check: TypeAdhesions={Count}", typeAdhesions.Count);
        foreach (var t in typeAdhesions)
            logger.LogInformation("TypeAdhesion: Id={Id} Libelle={Libelle}", t.IdTypeAdhesion, t.Libelle);

        var devises = context.Devises.AsNoTracking().OrderBy(x => x.IdDevise).ToList();
        logger.LogInformation("Seed check: Devises={Count}", devises.Count);
        foreach (var d in devises)
            logger.LogInformation("Devise: Id={IdDevise} Code={Code} Nom={Nom}", d.IdDevise, d.Code, d.Nom);

        // Initialiser les types de notifications par défaut
        using (var notificationScope = app.Services.CreateScope())
        {
            var notificationTypeService = notificationScope.ServiceProvider.GetRequiredService<INotificationTypeService>();
            await notificationTypeService.SeedDefaultTypesAsync();
        }
        logger.LogInformation("Types de notifications par défaut initialisés");

        var roles = context.Roles.AsNoTracking().OrderBy(x => x.IdRole).ToList();
        logger.LogInformation("Seed check: Roles count: {Count}", roles.Count);
        foreach (var r in roles)
            logger.LogInformation("Role: IdRole={IdRole}, Nom={Nom}", r.IdRole, r.Nom);

        var utilisateur = await context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.EmailUtilisateur == "admin@prosoc.cd" || u.NomUtilisateur == "admin@prosoc.cd");
        if (utilisateur != null)
            logger.LogInformation("Seed check: Utilisateur={NomUtilisateur}", utilisateur.NomUtilisateur);
        else
            logger.LogWarning("Seed check: Utilisateur admin@prosoc.cd not found");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing database");
    }
}

app.Run();

public partial class Program { }
