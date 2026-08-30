using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PostIn.Components;
using PostIn.Data;
using PostIn.Endpoints;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Servizi Blazor e Radzen
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<DialogService>();




// Connessione Database (SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "Data Source=postin.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
/*
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString) // o UseNpgsql / UseSqlite
           .ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
);*/

// Autenticazione e Autorizzazione (Cookie)
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/AccessDenied";
});

var app = builder.Build();

// Migrazione automatica e Seeding iniziale del DB
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    await DbSeeder.SeedAsync(dbContext);
}

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection(); // Solo in produzione per evitare warning locali
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Registrazione Endpoint e Componenti
app.MapCoverEndpoints(); // Endpoint protetto
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();