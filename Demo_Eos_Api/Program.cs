
using AuthenExamReceiveData.Service.Implement;
using AuthenExamReceiveData.Service.Interface;
using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;

namespace AuthenExamReceiveData
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigurationManager configuration = builder.Configuration;
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddOptions(); // Kích ho?t Options
            var fileConfig = configuration.GetSection("FileConfig");
            builder.Services.Configure<FileConfig>(fileConfig);
            builder.Services.AddScoped(typeof(IImageService), typeof(ImageService));
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
