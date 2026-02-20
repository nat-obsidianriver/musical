var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpClient("MusicalApi", client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7200";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
