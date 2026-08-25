using System.Net;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Services;
using Jemar.Aplication.Validation;
using FluentValidation;
using Jemar.Infrastructure.Persistence;
using Jemar.Infrastructure.Persistence.Repository;
using Jemar.Infrastructure.Services;
using Jemar.Presentation.Authorization;
using Jemar.Presentation.Middleware;
using Jemar.Presentation.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

const string FrontendCors = "FrontendDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy =>
        policy.WithOrigins(
                  "http://localhost:5174",
                  "https://nice-mushroom-00666e20f.7.azurestaticapps.net")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document), [] }
    });
});

builder.Services.AddDbContext<JemarDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

var secret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("La configuraci�n 'Jwt:Secret' no existe.");

var issuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("La configuraci�n 'Jwt:Issuer' no existe.");

var audience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("La configuraci�n 'Jwt:Audience' no existe.");

_ = builder.Configuration["MercadoPago:AccessToken"]
    ?? throw new InvalidOperationException("La configuraci�n 'MercadoPago:AccessToken' no existe.");

if (secret.Length < 32)
{
    secret = secret.PadRight(32, '!');
}

var key = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ValidateIssuer = true,
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.SuperAdminOnly,
        policy => policy.RequireRole("SuperAdmin"));

    options.AddPolicy(Policies.EmployeeOrAbove,
        policy => policy.RequireRole("Employee", "SuperAdmin"));

    options.AddPolicy(Policies.ClientOrAbove,
        policy => policy.RequireRole("Client", "Employee", "SuperAdmin"));
});

builder.Services.AddHttpClient<IOpenStreetMapService, OpenStreetMapService>(client =>
{
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org");
    client.DefaultRequestHeaders.Add("User-Agent", "JemarEnviosApp/1.0 (contacto@tu-correo.com)");
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddResilienceHandler("nominatim-resilience", (pipeline, context) =>
{
    var logger = context.ServiceProvider.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("Nominatim.Resilience");

    pipeline.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay            = TimeSpan.FromSeconds(2),
        BackoffType      = DelayBackoffType.Exponential,
        UseJitter        = true,
        ShouldHandle     = args => args.Outcome switch
        {
            { Exception: HttpRequestException }                          => PredicateResult.True(),
            { Exception: TaskCanceledException }                         => PredicateResult.True(),
            { Result.StatusCode: >= HttpStatusCode.InternalServerError } => PredicateResult.True(),
            { Result.StatusCode: HttpStatusCode.TooManyRequests }        => PredicateResult.True(),
            _                                                            => PredicateResult.False()
        },
        OnRetry = args =>
        {
            logger.LogWarning(
                "[Nominatim] Reintento #{Attempt} - motivo: {Outcome}",
                args.AttemptNumber + 1,
                args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()
            );
            return ValueTask.CompletedTask;
        }
    });

    pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        SamplingDuration  = TimeSpan.FromSeconds(60),
        FailureRatio      = 0.5,
        MinimumThroughput = 5,
        BreakDuration     = TimeSpan.FromSeconds(30),
        ShouldHandle      = args => args.Outcome switch
        {
            { Exception: HttpRequestException }                          => PredicateResult.True(),
            { Exception: TaskCanceledException }                         => PredicateResult.True(),
            { Result.StatusCode: >= HttpStatusCode.InternalServerError } => PredicateResult.True(),
            _                                                            => PredicateResult.False()
        },
        OnOpened = args =>
        {
            logger.LogError(
                "[Nominatim] Circuito ABIERTO por {Duration}s - demasiados errores consecutivos.",
                args.BreakDuration.TotalSeconds
            );
            return ValueTask.CompletedTask;
        },
        OnClosed = args =>
        {
            logger.LogInformation("[Nominatim] Circuito CERRADO - servicio recuperado.");
            return ValueTask.CompletedTask;
        },
        OnHalfOpened = args =>
        {
            logger.LogInformation("[Nominatim] Circuito SEMI-ABIERTO - probando recuperacion.");
            return ValueTask.CompletedTask;
        }
    });
});
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITrustedDeviceRepository, TrustedDeviceRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IInquiryRepository, InquiryRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IInquiryService, InquiryService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMercadoPagoService, MercadoPagoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IValidator<SignUpRequest>, SignUpRequestValidator>();
builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateProfileRequest>, UpdateProfileRequestValidator>();
builder.Services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();
if (!string.IsNullOrWhiteSpace(builder.Configuration["Communication:ConnectionString"]))
    builder.Services.AddScoped<IEmailService, AcsEmailService>();
else
    builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("AuthSensitive", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{httpContext.Request.Path}:{httpContext.Connection.RemoteIpAddress}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(FrontendCors);

app.UseRateLimiter();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();

app.UseMiddleware<RoleMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();