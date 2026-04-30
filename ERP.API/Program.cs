using ERP.API;
// Add Entity Framework Core DbContext registration
using Microsoft.EntityFrameworkCore;
using ERP.API.Services;
using ERP.API.Services.CompanySetup;
using ERP.API.Services.Implementation.CompanySetup;
using ERP.API.Services.Implementation;
using ERP.API.Services.Background;
using ERP.API.UserManagement;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ERP.API.UserManagement.Services;
using ERP.API.Hubs;
using System.Text.Json;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Enable legacy timestamp behavior for Npgsql (PostgreSQL)
// This allows using DateTime with Kind=Local or Unspecified which is common in this codebase
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<QuotationPdfService>();
builder.Services.AddMemoryCache(); // Added for EWayBill token caching

// Set Dapper to match names with underscores
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Configure Kestrel to listen on port 5104
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5104);
});

// Add JWT authentication
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//        };
//    });
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
        };

        // For SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<ERP.API.Services.IGoogleDriveService, ERP.API.Services.GoogleDriveService>();

// Get connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register AppDbContext for EF Core with PostgreSQL (required for DashboardController)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register SalesOpportunityService as a concrete type for DI
builder.Services.AddScoped<SalesOpportunityService>(sp =>
    new SalesOpportunityService(connectionString, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SalesOpportunityService>>()));

// Register SalesRepDashboardService for DI
builder.Services.AddScoped<ERP.API.Services.ISalesRepDashboardService, ERP.API.Services.SalesRepDashboardService>(sp =>
    new ERP.API.Services.SalesRepDashboardService(connectionString, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ERP.API.Services.SalesRepDashboardService>>()));

// Register IDbConnection for Dapper
builder.Services.AddScoped<System.Data.IDbConnection>(sp => new Npgsql.NpgsqlConnection(connectionString));

// Register User Service
builder.Services.AddScoped<ERP.API.Services.IUserService, ERP.API.Services.UserService>();

// Register Claim Voucher Card Service
builder.Services.AddScoped<IClaimVoucherCardService, ClaimVoucherCardService>();

// Register connection string for services that need it directly
builder.Services.AddScoped<ICsBranchWarehouseService>(sp =>
    new CsBranchWarehouseService(connectionString, sp.GetRequiredService<ILogger<CsBranchWarehouseService>>()));

// Register Bank Account Branch Service
builder.Services.AddScoped<ICsBankAccountBranchService, CsBankAccountBranchService>();

// Register Branch Cost Centre Service
builder.Services.AddScoped<ICsBranchCostCentreService>(sp =>
    new CsBranchCostCentreService(connectionString, sp.GetRequiredService<ILogger<CsBranchCostCentreService>>()));

// Register Cost Centre Service
builder.Services.AddScoped<ICsCostCentreService>(sp =>
    new CsCostCentreService(connectionString, sp.GetRequiredService<ILogger<CsCostCentreService>>()));

// Register GST Rate Service
builder.Services.AddScoped<ICsGstRateService>(sp =>
    new CsGstRateService(sp.GetRequiredService<IConfiguration>(), sp.GetRequiredService<ILogger<CsGstRateService>>()));

// Register Intercompany Account Service
builder.Services.AddScoped<ICsIntercompanyAccountService>(sp =>
    new CsIntercompanyAccountService(builder.Configuration));

// Register Team Service 
builder.Services.AddScoped<ITeamHierarchyService, TeamHierarchyService>();

// Register FinancialStatementTemplateService
builder.Services.AddScoped<FinancialStatementTemplateService>();

   
// Register TDS Rate Service
builder.Services.AddScoped<ICsTdsRateService>(sp =>
    new CsTdsRateService(connectionString));

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERP API",
        Version = "v1",
        Description = "API for the ERP system"
    });

    c.EnableAnnotations();

    // Enable XML comments in Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Include API Explorer settings
    c.DocInclusionPredicate((docName, apiDesc) => apiDesc.ActionDescriptor.EndpointMetadata.All(x => x is not ApiExplorerSettingsAttribute ax || !ax.IgnoreApi));

    // Add JWT Authentication support in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
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

    // Group endpoints by controller
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });    // Generate operation IDs based on controller and action name
    c.CustomOperationIds(apiDesc =>
    {
        var controller = apiDesc.ActionDescriptor.RouteValues["controller"];
        var action = apiDesc.ActionDescriptor.RouteValues["action"];
        return $"{controller}_{action}";
    });

    // Enable annotations
    c.EnableAnnotations();

    // Replace default "string" example values with empty strings
    c.SchemaFilter<ERP.API.Helpers.EmptyStringSchemaFilter>();
});

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddScoped<ICsInventoryLocationService, CsInventoryLocationService>();
builder.Services.AddScoped<ICsOpeningBalanceService, CsOpeningBalanceService>();
builder.Services.AddScoped<ICsPaymentTermService, CsPaymentTermService>();
builder.Services.AddScoped<ICsSacCodeService, CsSacCodeService>();
builder.Services.AddScoped<ICsIntercompanyRelationshipService, CsIntercompanyRelationshipService>();
builder.Services.AddScoped<ICsDefaultAccountMappingService, CsDefaultAccountMappingService>();
builder.Services.AddScoped<ICsWarehouseService, CsWarehouseService>();
builder.Services.AddScoped<ISalesOpportunityService>(sp =>
    new SalesOpportunityService(connectionString, sp.GetRequiredService<ILogger<SalesOpportunityService>>()));
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<ISalesOrderGridService, SalesOrderGridService>();
builder.Services.AddScoped<ISalesQuotationGridService, SalesQuotationGridService>();
builder.Services.AddScoped<ISalesDemoGridService, SalesDemoGridService>();  // Add this line
builder.Services.AddScoped<ERP.API.Services.IInvoiceGridService, ERP.API.Services.InvoiceGridService>();
builder.Services.AddScoped<ISalesDemoService, SalesDemoService>();
builder.Services.AddScoped<ISalesDealService, SalesDealService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IDeliveryChallanService, DeliveryChallanService>();
builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<SalesLeadService>(sp =>
    new SalesLeadService(connectionString, sp.GetRequiredService<ILogger<SalesLeadService>>()));
builder.Services.AddScoped<SalesQuotationService>(sp =>
    new SalesQuotationService(connectionString));
builder.Services.AddScoped<QuotationService>(sp =>
{
    var leadService = sp.GetRequiredService<SalesLeadService>();
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var config = sp.GetRequiredService<IConfiguration>();
    return new QuotationService(leadService, env, config);
});

builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
    options.FormatterMappings.SetMediaTypeMappingForFormat("json", "application/json");
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // Recommended
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.WriteIndented = true;
})
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
})
.AddMvcOptions(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
    options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.StringOutputFormatter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressConsumesConstraintForFormFileParameters = true;
    options.SuppressInferBindingSourcesForParameters = true;
    options.InvalidModelStateResponseFactory = context =>
    {
        var result = new BadRequestObjectResult(new
        {
            message = "Invalid model state",
            statusCode = 400,
            errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
        });
        result.ContentTypes.Add("application/json");
        return result;
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
               .WithOrigins(
                   "http://localhost:3000", "https://localhost:3000",
                   "http://127.0.0.1:3000", "https://127.0.0.1:3000",
                   "http://localhost:5104", "https://localhost:5104",
                   "http://127.0.0.1:5104", "https://127.0.0.1:5104"
               )
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .SetIsOriginAllowed(_ => true);
    });
    
    // Add a more permissive policy for development
    options.AddPolicy("Development", builder =>
    {
        builder
               .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Register existing services
builder.Services.AddScoped<SalesQuotationService>(sp =>
    new SalesQuotationService(connectionString));
// builder.Services.AddScoped<SalesLeadService>(sp =>
    // new SalesLeadService(connectionString));
builder.Services.AddScoped<SalesContactService>(sp =>
    new SalesContactService(connectionString));
builder.Services.AddScoped<SalesAddressService>(sp =>
    new SalesAddressService(connectionString));
// Register DemoChecklistService
builder.Services.AddScoped<IDemoChecklistService, DemoChecklistService>();
builder.Services.AddScoped<SalesLeadsBusinessChallengeService>(sp =>
    new SalesLeadsBusinessChallengeService(connectionString));
builder.Services.AddScoped<SalesDemoInventoryService>(sp =>
    new SalesDemoInventoryService(builder.Configuration));

// Register Bank Account service
builder.Services.AddScoped<ISalesBankAccountService, SalesBankAccountService>();
// Register Terms and Conditions service
builder.Services.AddScoped<ISalesTermsAndConditionsService, SalesTermsAndConditionsService>();
builder.Services.AddScoped<SalesTermsAndConditionsService>(sp =>
    new SalesTermsAndConditionsService(builder.Configuration, sp.GetRequiredService<ILogger<SalesTermsAndConditionsService>>()));

// Register TermsConditions service
builder.Services.AddScoped<ITermsConditionsService, TermsConditionsService>();

builder.Services.AddScoped<SalesActivityMeetingService>(sp =>
    new SalesActivityMeetingService(connectionString));
builder.Services.AddScoped<SalesProductsService>(sp =>
    new SalesProductsService(connectionString));
builder.Services.AddScoped<SalesActivityEventService>(sp =>
    new SalesActivityEventService(connectionString));
builder.Services.AddScoped<SalesActivityCallService>(sp =>
    new SalesActivityCallService(connectionString));
builder.Services.AddScoped<SalesActivityTaskService>(sp =>
    new SalesActivityTaskService(connectionString));
builder.Services.AddScoped<InventoryItemService>(sp =>
    new InventoryItemService(connectionString, sp.GetRequiredService<IHttpContextAccessor>()));

// Add SalesExternalCommentService
builder.Services.AddScoped<SalesExternalCommentService>(sp =>
    new SalesExternalCommentService(connectionString));

// Add SalesSummaryService
builder.Services.AddScoped<SalesSummaryService>(sp =>
    new SalesSummaryService(connectionString));

// Register location services
builder.Services.AddScoped<SalesCountryService>(sp =>
    new SalesCountryService(connectionString));
builder.Services.AddScoped<SalesStateService>(sp =>
    new SalesStateService(connectionString));
builder.Services.AddScoped<SalesTerritoryService>(sp =>
    new SalesTerritoryService(connectionString));
builder.Services.AddScoped<SalesDistrictService>(sp =>
    new SalesDistrictService(connectionString));
builder.Services.AddScoped<SalesCityService>(sp =>
    new SalesCityService(connectionString));
builder.Services.AddScoped<SalesAreaService>(sp =>
    new SalesAreaService(connectionString));
builder.Services.AddScoped<SalesPincodeService>(sp =>
    new SalesPincodeService(connectionString));

builder.Services.AddScoped<SalesLocationService>(sp =>
    new SalesLocationService(connectionString));
// Add SalesDocumentService
builder.Services.AddScoped<SalesDocumentService>();

// Add InternalDiscussionService
builder.Services.AddScoped<InternalDiscussionService>(sp =>
    new InternalDiscussionService(connectionString)); 
// Add GeographicalDivisionService
builder.Services.AddScoped<IGeographicalDivisionService, GeographicalDivisionService>();

// Add User Management services
builder.Services.AddUserManagement(options =>
{
    // Configure password policy
    options.PasswordPolicy.MinimumLength = 12;
    options.PasswordPolicy.RequireUppercase = true;
    options.PasswordPolicy.RequireLowercase = true;
    options.PasswordPolicy.RequireDigit = true;
    options.PasswordPolicy.RequireSpecialCharacter = true;
    options.PasswordPolicy.PasswordHistoryCount = 5;
    options.PasswordPolicy.PasswordExpiryDays = 90;
    
    // Configure security settings
    options.Security.MaxFailedLoginAttempts = 5;
    options.Security.LockoutDurationMinutes = 15;
    options.Security.SessionTimeoutMinutes = 30;
    options.Security.PasswordResetTokenExpiryHours = 24;
    options.Security.EnableIpTracking = true;
    options.Security.EnableDeviceTracking = true;
    
    // Configure 2FA settings
    options.TwoFactor.Enabled = true;
    options.TwoFactor.IssuerName = "ERP System";
    options.TwoFactor.TokenValidityPeriod = 30;
    options.TwoFactor.ClockDriftTolerance = 1;
});
// Add SalesDemoAssignmentService
builder.Services.AddScoped<SalesDemoAssignmentService>(sp =>
    new SalesDemoAssignmentService(builder.Configuration));
// Register IPurchaseOrderService and PurchaseOrderService
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
// Register InvoiceService
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<ISalesQuotationService>(sp =>
    new SalesQuotationService(connectionString));

// Register OrderAcceptanceService
builder.Services.AddScoped<IOrderAcceptanceService, OrderAcceptanceService>();

// Add CompanySetup services
builder.Services.AddScoped<ICsCompanyService>(sp =>
    new CsCompanyService(connectionString, sp.GetRequiredService<ILogger<CsCompanyService>>()));
builder.Services.AddScoped<ICsBranchService>(sp =>
    new CsBranchService(connectionString, sp.GetRequiredService<ILogger<CsBranchService>>()));
builder.Services.AddScoped<ICsAccountingPeriodService>(sp =>
    new CsAccountingPeriodService(sp.GetRequiredService<IConfiguration>(), sp.GetRequiredService<ILogger<CsAccountingPeriodService>>()));
builder.Services.AddScoped<ICsChartOfAccountService>(sp =>
    new CsChartOfAccountService(sp.GetRequiredService<IConfiguration>(), sp.GetRequiredService<ILogger<CsChartOfAccountService>>()));
builder.Services.AddScoped<ICsBankAccountService>(sp =>
    new CsBankAccountService(connectionString, sp.GetRequiredService<ILogger<CsBankAccountService>>()));
// Register HSN Code service
builder.Services.AddScoped<ICsHsnCodeService>(sp => 
    new CsHsnCodeService(sp.GetRequiredService<IConfiguration>()));

// Register Email services
builder.Services.AddScoped<IGmailService, GmailService>();

// Register WhatsApp services
builder.Services.AddHttpClient("WhatsAppGraph")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Enforce TLS 1.2+ for Meta API calls
        SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
    });
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

// Register e-Way Bill services
builder.Services.AddHttpClient<IEWayBillService, EWayBillService>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
    });

// Register Chat Service
builder.Services.AddScoped<IChatService>(sp =>
    new ChatService(connectionString, sp.GetRequiredService<ILogger<ChatService>>()));

// Register Storage Service
builder.Services.AddScoped<IStorageService, StorageService>();


// Register CurrencyExchangeRateService for DI
builder.Services.AddScoped<ERP.API.Services.CompanySetup.CurrencyExchangeRateService>();

// Register JournalEntryTemplateService for DI
builder.Services.AddScoped<ERP.API.Services.CompanySetup.JournalEntryTemplateService>();
// Register AssignedToDropdownService for DI
builder.Services.AddScoped<ERP.API.Services.IAssignedToDropdownService, ERP.API.Services.AssignedToDropdownService>();

// Register DesignationService for DI
builder.Services.AddScoped<DesignationService>(sp =>
    new DesignationService(connectionString));

// Register DepartmentService for DI
builder.Services.AddScoped<DepartmentService>(sp =>
    new DepartmentService(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP API v1");
    });
    
    // Use AllowAll CORS policy (with AllowCredentials for SignalR)
    app.UseCors("AllowAll");
}
else
{
    // Enable Swagger and SwaggerUI for all environments
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP API V1");
        c.RoutePrefix = "swagger";
    });
    
    // Use restricted CORS policy in production
    app.UseCors("AllowAll");
}


//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");



var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");