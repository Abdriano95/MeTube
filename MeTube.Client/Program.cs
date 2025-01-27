using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.Client.ViewModels.LoginViewModels;
using MeTube.Client.ViewModels.SignupViewModels;
using MeTube.Client.Views;
using MeTube.DTO;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
namespace MeTube.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5001/") });
            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            //builder.Services.AddScoped<SignupViewModel>();
            //builder.Services.AddScoped<SignupView>();

            builder.Services.AddSingleton<SignupView>();
            builder.Services.AddSingleton<LoginView>();


            builder.Services.AddTransient<SignupViewModel>();
            builder.Services.AddTransient<LoginViewModel>();

            builder.Services.AddSingleton<ClientService>();
            builder.Services.AddSingleton<IUserService, UserService>();


            builder.Services.AddAutoMapper(typeof(User));
            builder.Services.AddTransient<HttpClient>();
            //builder.Services.AddSingleton<IHttpsClientHandlerService, HttpsClientHandlerService>();



            await builder.Build().RunAsync();
        }
    }
}
