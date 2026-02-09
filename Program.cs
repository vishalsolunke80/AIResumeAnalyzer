using AIResumeAnalyzer.Data;
using AIResumeAnalyzer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<OpenAIService>();
builder.Services.AddScoped<ResumeAnalyzer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


try
{
    // Ensure database is created/migrated
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine($"DEBUG: Database Provider: {db.Database.ProviderName}");
        db.Database.Migrate();
    }

    Console.WriteLine("Application starting...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Critical failure: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
