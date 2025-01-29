using MeTube.Data.Repository;
using MeTube.API.Profiles;
using Microsoft.EntityFrameworkCore;
using MeTube.Data;
using MeTube.API.Services;

namespace MeTube.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MeTubeDB")));

            // Add UnitOfWork 
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add AutoMapper
            builder.Services.AddAutoMapper(typeof(UserProfile), typeof(VideoProfile));

            builder.Services.AddCors(options => 
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("https://locolhost:7248")
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod();
            }));

            // Add VideoService
            builder.Services.AddScoped<VideoService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MeTube API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseCors();

            app.Run();
        }
    }
}


