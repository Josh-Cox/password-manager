using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Graph;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PasswordManager.API.Services;
using System.IdentityModel.Tokens.Jwt;

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();

var auth = builder.Configuration.GetSection("Auth");
var tenantId = auth["TenantId"];
var authority = auth["Authority"] ?? $"https://{tenantId}.ciamlogin.com/{tenantId}/v2.0";
var audience = auth["Audience"];
var audienceClientId = audience?.Replace("api://", string.Empty);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.MetadataAddress = $"{authority.TrimEnd('/')}/.well-known/openid-configuration";
        options.Audience = audienceClientId;
        options.MapInboundClaims = false;
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = true,
            ValidAudiences = string.IsNullOrWhiteSpace(audience)
                ? []
                : [audience, audienceClientId],
            ValidateLifetime = true,
            NameClaimType = "oid"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<VaultStorageService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var url = config["AzureStorageAccountUrl"]
        ?? throw new InvalidOperationException("AzureStorageAccountUrl is not configured.");
    return new VaultStorageService(url);
});

builder.Services.AddSingleton<GraphServiceClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var tenantId = config["Graph:TenantId"]
        ?? throw new InvalidOperationException("Graph:TenantId is not configured.");
    var clientId = config["Graph:ClientId"]
        ?? throw new InvalidOperationException("Graph:ClientId is not configured.");
    var clientSecret = config["Graph:ClientSecret"]
        ?? throw new InvalidOperationException("Graph:ClientSecret is not configured.");

    var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
    return new GraphServiceClient(credential);
});

var app = builder.Build();

// Middleware pipeline
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map controllers to routes
app.MapControllers();

app.Run();
