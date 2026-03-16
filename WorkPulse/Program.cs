using Azure.API.Config;
using Azure.API.Services;
using Azure.API.Services.Interfaces;
using CertMonitor.API.Interfaces;
using CertMonitor.API.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using MudBlazor.Services;
using System.Net.Http.Headers;
using System.Text;
using WorkPulse.Components;
using WorkPulse.Services;
using WorkPulse.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.Configure<AzureDevOpsOptions>(builder.Configuration.GetSection("AzureDevOpsOptions"));

builder.Services.AddScoped<IWorkItemService, WorkItemService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAzureBoardService, AzureBoardService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddHttpClient("AzureDevOps", client =>
{
    var wiUri = builder.Configuration.GetSection("AzureDevOpsOptions").Get<AzureDevOpsOptions>();
    var pat = wiUri?.Pat;
    if (!string.IsNullOrEmpty(pat))
    {
        var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
    }
    
    client.BaseAddress = new Uri($"{wiUri?.BaseUrl}");
});

builder.Services.AddHttpClient();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
