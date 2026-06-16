using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Services;
using Jemar.Infrastructure.Persistence;
using Jemar.Infrastructure.Persistence.Repository;
using Jemar.Presentation.Authorization;
using Jemar.Presentation.Middleware;
using Jemar.Presentation.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var secret = builder.Configuration["Jwt:Secret"] ?? "a-la-grande-le-puse-cuca";
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "JemarApi",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "JemarClients",
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
});


builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IInquiryRepository, InquiryRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IInquiryService, InquiryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseMiddleware<RoleMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
