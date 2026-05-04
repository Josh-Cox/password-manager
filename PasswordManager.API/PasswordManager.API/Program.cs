using PasswordManager.API.Services;
using PasswordManager.API.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();

builder.Services.AddSingleton<VaultStorageService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var conn = config["AzureBlobStorage"];
    return new VaultStorageService(conn);
});

var app = builder.Build();

// Middleware pipeline
//app.UseHttpsRedirection();

// (Auth will go here later)
app.UseAuthorization();

// Map controllers to routes
app.MapControllers();

app.Run();