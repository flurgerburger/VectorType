using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VectorType;
using VectorType.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// VectorType Game Services
builder.Services.AddSingleton<IWordBankService, WordBankService>();
builder.Services.AddScoped<ISoundService, SoundService>();
builder.Services.AddScoped<GameEngine>();

await builder.Build().RunAsync();
