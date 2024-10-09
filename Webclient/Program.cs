using Microsoft.EntityFrameworkCore;
using Webclient.Hubs;
using Webclient.MiddlewareExtensions;
using Webclient.Models;
using Webclient.SubscribeTableDependencies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//    .AddJsonOptions(options =>
//{
//    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
//});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Add SignalR
builder.Services.AddSingleton<ChartHub>();
builder.Services.AddSingleton<SubscribeOrderTableDependency>();
builder.Services.AddSignalR();

// Add DbContext before building the app
var connectionString = builder.Configuration.GetConnectionString("MyCnn") ?? throw new InvalidOperationException("Connection string DbContext not found.");
builder.Services.AddDbContext<HikariYumeContext>(options =>
{
    options.UseSqlServer(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapHub<ChartHub>("/chartHub");
//app.UseOrderTableDependency();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
