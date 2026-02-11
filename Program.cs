using DataGen_v1.Components;
using DataGen_v1.Data;
using DataGen_v1.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// AREA 1: SERVICE REGISTRATION (Do this BEFORE Build)
// ==========================================

// 1. Add Razor/Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. Add Fluent UI
builder.Services.AddFluentUIComponents();
builder.Services.AddScoped<IDialogService, DialogService>();

// 3. Add Generator Service
builder.Services.AddScoped<GeneratorService>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpSenderService>();

builder.Services.AddScoped<DbInsertService>();

// 4. Add Database Context (The part causing your error)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ==========================================
// END OF REGISTRATION
// ==========================================

var app = builder.Build(); // <--- STOP! The container locks here.

// ==========================================
// AREA 2: PIPELINE & RUNTIME (Do this AFTER Build)
// ==========================================

// 5. Auto-Migration Logic (Uses the app scope, so it must be AFTER Build)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

// 6. HTTP Pipeline configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();