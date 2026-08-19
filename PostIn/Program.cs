using PostIn.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using PostIn.Data;
using PostIn.Data.Entities;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// DB Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=postin.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Enable notifications
builder.Services.AddScoped<NotificationService>();

// Auth handler
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LogoutPath = "/logout";
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/AccessDenied";
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    await DbSeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Force auth redirect
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// 7. Endpoint Protetto per il Download delle Copertine
app.MapGet("/uploads/covers/{fileName}", (string fileName, IWebHostEnvironment env) =>
{
    // Sanificazione del nome file per impedire navigazione arbitraria nel disco
    var safeFileName = Path.GetFileName(fileName);
    var filePath = Path.Combine(env.ContentRootPath, "Uploads", "Covers", safeFileName);

    if (!File.Exists(filePath))
    {
        return Results.NotFound();
    }

    var extension = Path.GetExtension(safeFileName).ToLowerInvariant();
    var contentType = extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };

    return Results.File(filePath, contentType);
}).RequireAuthorization(); // Blocca le richieste anonime reindirizzando al Login


app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
