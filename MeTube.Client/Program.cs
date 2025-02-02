using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.Client.ViewModels.LoginViewModels;
using MeTube.Client.ViewModels.ManageUsersViewModels;
using MeTube.Client.ViewModels.SignupViewModels;
using MeTube.Client.ViewModels.VideoViewModels;
using MeTube.Client.Views;
using MeTube.DTO;
using Microsoft.AspNetCore.Components;
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

            //builder.Services.AddScoped(AuthenticationStateProvider);
            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            //builder.Services.AddScoped<SignupViewModel>();
            //builder.Services.AddScoped<SignupView>();

            //builder.Services.AddSingleton<SignupView>();
            builder.Services.AddSingleton<LoginView>();
            builder.Services.AddSingleton<ManageUsersView>();
            builder.Services.AddSingleton<VideoView>();
            builder.Services.AddSingleton<ManageVideos>();
            builder.Services.AddSingleton<EditVideo>();
            builder.Services.AddSingleton<Home>();


            builder.Services.AddTransient<SignupViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<ManageUsersViewModel>();
            builder.Services.AddTransient<VideoViewModel>();
            builder.Services.AddTransient<VideoListViewModel>();

            builder.Services.AddSingleton<ClientService>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddScoped<IVideoService, VideoService>();

            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddAutoMapper(typeof(User));
            builder.Services.AddAutoMapper(typeof(Video));
            builder.Services.AddAutoMapper(typeof(Program).Assembly);
            builder.Services.AddTransient<HttpClient>();
            //builder.Services.AddSingleton<IHttpsClientHandlerService, HttpsClientHandlerService>();



            await builder.Build().RunAsync();
        }
    }
}
