using Microsoft.AspNetCore.Authentication.Cookies;
using Musical.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages()
    .AddRazorPagesOptions(o =>
    {
        o.Conventions.AuthorizePage("/Scores/Upload");
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtTokenHandler>();

builder.Services.AddHttpClient("MusicalApi", client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7136";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly  = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout      = TimeSpan.FromHours(24);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath  = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
