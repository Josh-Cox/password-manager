var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();

var app = builder.Build();

// Middleware pipeline
//app.UseHttpsRedirection();

// (Auth will go here later)
app.UseAuthorization();

// Map controllers to routes
app.MapControllers();

app.Run();