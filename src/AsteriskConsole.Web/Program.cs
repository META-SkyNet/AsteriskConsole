using AsteriskConsole.Web.Hubs;
using AsteriskConsole.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IAsteriskServerService, AsteriskServerService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapHub<AsteriskHub>("/asteriskhub");
app.MapFallbackToPage("/_Host");

app.Run();
