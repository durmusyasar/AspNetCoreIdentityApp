using AspNetCoreIdentityApp.Core.OptionsModels;
using AspNetCoreIdentityApp.Core.Permissions;
using AspNetCoreIdentityApp.Web.ClaimProvider;
using AspNetCoreIdentityApp.Web.Extenisons;
using AspNetCoreIdentityApp.Repository.Models;
using AspNetCoreIdentityApp.Web.Requirements;
using AspNetCoreIdentityApp.Web.Seeds;
using AspNetCoreIdentityApp.Service.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlCon"), opt =>
{
    opt.MigrationsAssembly("AspNetCoreIdentityApp.Repository");
}));

builder.Services.AddIdentityWithExt();

builder.Services.ConfigureApplicationCookie(opt =>
{
    var cookieBuidler = new CookieBuilder
    {
        Name = "IdentityAppCookie"
    };
    opt.LoginPath = new PathString("/Home/Signin");
    opt.LogoutPath = new PathString("/Member/Logout");
    opt.AccessDeniedPath = new PathString("/Member/AccessDenied");
    opt.ExpireTimeSpan = TimeSpan.FromDays(1);
    opt.SlidingExpiration = true;
});
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
builder.Services.AddScoped<IClaimsTransformation, UserClaimProvider>();
builder.Services.AddScoped<IAuthorizationHandler, ExchangeExpireRequirementHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ViolenceRequirementHandler>();
builder.Services.AddScoped<IMemberService, MemberService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TrabzonPolicy", policy =>
    {
        policy.RequireClaim("city", "Trabzon");
    });

    options.AddPolicy("ExchangePolicy", policy =>
    {
        policy.AddRequirements(new ExchangeExpireRequirement());
    });

    options.AddPolicy("ViolencePolicy", policy =>
    {
        policy.AddRequirements(new ViolenceRequirement() { ThresholdAge = 18 });
    });

    options.AddPolicy("OrderPermissionReadAndDelete", policy =>
    {
        policy.RequireClaim("permission", Permissions.Order.Read);
        policy.RequireClaim("permission", Permissions.Order.Delete);
        policy.RequireClaim("permission", Permissions.Stock.Delete);
    });

    options.AddPolicy("Permissions.Order.Read", policy =>
    {
        policy.RequireClaim("permission", Permissions.Order.Read);
    });

    options.AddPolicy("Permissions.Order.Delete", policy =>
    {
        policy.RequireClaim("permission", Permissions.Order.Delete);
    });


    options.AddPolicy("Permissions.Stock.Delete", policy =>
    {
        policy.RequireClaim("permission", Permissions.Stock.Delete);
    });
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

    await PermissionSeed.Seed(roleManager);
}

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
