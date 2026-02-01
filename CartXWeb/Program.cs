using CartX.DataAccess.Data;
using CartX.DataAccess.Repository;
using CartX.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CartX.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using CartX.DataAccess.DbInitializer;
using Azure.Storage.Blobs;
using CartXWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions
            .EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelaySeconds: 10,
                errorNumbersToAdd: null)
            .CommandTimeout(180)));

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddIdentity<IdentityUser,IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
builder.Services.AddScoped<IEmailSender,EmailSender>();

// Configure Storage Service based on Azure Blob Storage availability and environment
var blobConnectionString = builder.Configuration.GetConnectionString("AzureBlobStorage");
var blobContainerName = builder.Configuration.GetSection("AzureStorage:ContainerName").Get<string>();

if (!string.IsNullOrEmpty(blobConnectionString) && builder.Environment.IsProduction())
{
    // Use Azure Blob Storage in Production (only if connection string is configured)
    try
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
        builder.Services.AddSingleton(blobContainerClient);
        builder.Services.AddSingleton<IStorageService, AzureBlobStorageService>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to configure Azure Blob Storage: {ex.Message}. Falling back to local storage.");
        builder.Services.AddScoped<IStorageService, LocalStorageService>();
    }
}
else
{
    // Use Local Storage in Development or when Azure is not configured
    builder.Services.AddScoped<IStorageService, LocalStorageService>();
}

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
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
SeedDatabase();
app.MapStaticAssets();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

void SeedDatabase()
{
    using(var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}
