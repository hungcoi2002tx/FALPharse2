using Amazon.Rekognition;
using Amazon.S3;
using FAL.Services;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Share.SystemModel;
using System.Text;

namespace FAL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                };

                c.AddSecurityDefinition("Bearer", securityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
            builder.Services.AddAWSService<IAmazonS3>();
            builder.Services.AddAWSService<IAmazonRekognition>();
            builder.Services.AddSingleton<CustomLog>(new CustomLog(Path.Combine(Directory.GetCurrentDirectory(), "bin", "ProgramLogs", "Log.txt")));
            builder.Services.AddSingleton<ICollectionService, CollectionService>();
            builder.Services.AddSingleton<IS3Service, S3Service>();

            // Cấu hình thông tin JWT từ appsettings.json hoặc môi trường
            var key = builder.Configuration["Jwt:Key"];
            var issuer = builder.Configuration["Jwt:Issuer"];
            var audience = builder.Configuration["Jwt:Audience"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.Zero // Loại bỏ thời gian trễ của token
                };
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication(); // Phải đặt trước app.UseAuthorization()
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
