using System.Configuration;
using AIGuesserGame.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("CookieAuthentication").AddCookie("CookieAuthentication", config =>
{
    config.Cookie.Name = "Access.Cookie";
    config.LoginPath = "/Home/Authenticate";
});

// Add object caching
builder.Services.AddDistributedMemoryCache();

// Add session variables
builder.Services.AddSession(
    options => options.IdleTimeout = TimeSpan.FromMinutes(10)
    ); ;

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

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
