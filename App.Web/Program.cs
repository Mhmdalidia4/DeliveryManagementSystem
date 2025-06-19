using App.Domain.Interfaces;
using App.Domain.Interfaces.Base;
using App.Service.Managers;
using App.Domain.Models;
using App.Infrastructure.Repositories;
using App.Infrastructure.Repositories.Base;
using App.Service.Interface;
using App.Service.Managers;
using App.Services.Managers;
using App.Web.Data;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------- DATABASE CONFIGURATION --------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Identity DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// App Entities DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// -------------------- IDENTITY CONFIGURATION --------------------
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // true in production
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// -------------------- DEPENDENCY INJECTION --------------------
// Repositories
builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderToReviewRepository, OrderToReviewRepository>();
builder.Services.AddScoped<IPickUpRequestRepository, PickUpRequestRepository>();
builder.Services.AddScoped<IOrderAreaRepository, OrderAreaRepository>();
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

// Managers
builder.Services.AddScoped<IShopManager, ShopManager>();
builder.Services.AddScoped<ICompanyManager, CompanyManager>();
builder.Services.AddScoped<IDriverManager, DriverManager>();
builder.Services.AddScoped<IFeedbackManager, FeedbackManager>();
builder.Services.AddScoped<IOrderManager, OrderManager>();
builder.Services.AddScoped<IOrderToReviewManager, OrderToReviewManager>();
builder.Services.AddScoped<IPickUpRequestManager, PickUpRequestManager>();
builder.Services.AddScoped<ISettingsManager, SettingsManager>();
builder.Services.AddScoped<CompanyManager>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<ShopNameResolver>();
builder.Services.AddScoped<DriverNameResolver>();
// -------------------- CORE FRAMEWORK SERVICES --------------------
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

// -------------------- BUILD APP --------------------
var app = builder.Build();

// -------------------- MIDDLEWARE --------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// -------------------- ROLE & USER SEEDING --------------------
//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

//    string[] roles = { "admin", "company", "driver", "shop" };
//    foreach (var role in roles)
//    {
//        if (!await roleManager.RoleExistsAsync(role))
//            await roleManager.CreateAsync(new IdentityRole(role));
//    }

//    async Task CreateUser(string email, string password, string role)
//{
//    var user = await userManager.FindByEmailAsync(email);
//    if (user == null)
//    {
//        user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
//        await userManager.CreateAsync(user, password);
//        await userManager.AddToRoleAsync(user, role);
//    }
//}
//    await CreateUser("admin@test.com", "Admin123!", "admin");
//    await CreateUser("company@test.com", "Company123!", "company");
//    await CreateUser("driver2@test.com", "Driver123!", "driver");
//    await CreateUser("shop2@test.com", "Shop123!", "shop");
//}

app.Run();
