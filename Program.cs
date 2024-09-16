using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

/*/ Add authentication services
builder.Services.AddAuthentication("CookieAuth").AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Account/LoginRegister";
    });*/

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, config =>
    {
        config.LoginPath = "/Account/LoginRegister"; // Path to the login page
        config.AccessDeniedPath = "/Account/LoginRegister"; // Path to the access denied page (optional)
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

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

//test
MySQLiteApp.WordDataAccess.readWords();
//test ende
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LoginRegister}/{id?}");

app.Run();