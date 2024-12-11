using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Rekognition;
using Amazon.S3;
using FAL.Authors;
using FAL.Services;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Security;
using System.Text;
using Amazon.SQS;
using Microsoft.AspNetCore.HttpOverrides;
using FAL.MiddleWare;
using Share.Utils;
using FAL.DataInitial;
using Amazon.Extensions.NETCore.Setup;
using Share.Model;

namespace FAL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<CustomAuthorizationFilter>();
            });
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
            string pathFileLog = builder.Configuration["PathFileLog"] ?? "";
            builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
            builder.Services.AddAWSService<IAmazonS3>();
            builder.Services.AddAWSService<IAmazonRekognition>();
            builder.Services.AddAWSService<IAmazonDynamoDB>();
            builder.Services.AddAWSService<IAmazonSQS>();
            builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
            builder.Services.AddSingleton<CustomLog>(new CustomLog(pathFileLog));
            builder.Services.AddSingleton<ICollectionService, CollectionService>();
            builder.Services.AddSingleton<IS3Service, S3Service>();
            builder.Services.AddSingleton<IPermissionService, PermissionService>();
            builder.Services.AddSingleton<DefaultDataInitializer>();
            builder.Services.AddSingleton<CustomAuthorizationFilter>();

            var key = builder.Configuration["Jwt:Key"] ?? "";
            var issuer = builder.Configuration["Jwt:Issuer"] ?? "";
            var audience = builder.Configuration["Jwt:Audience"] ?? "";

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

            builder.Services.AddSingleton<IDynamoDBService, DynamoDBService>();
            //builder.Services.Configure<IISServerOptions>(options =>
            //{
            //    options.MaxRequestBodySize = 100000000; // 50 MB, or set to any desired size
            //});

            //builder.Services.Configure<KestrelServerOptions>(options =>
            //{
            //    options.Limits.MaxRequestBodySize = 100000000; // 50 MB, or set to any desired size
            //});
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.MapGet("/", () => "OK");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            using (var scope = app.Services.CreateScope())
            {
                var dataInitializer = scope.ServiceProvider.GetRequiredService<DefaultDataInitializer>();
                try
                {
                    await dataInitializer.SeedDefaultDataAsync();
                    Console.WriteLine("Dữ liệu mặc định đã được khởi tạo thành công.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi khởi tạo dữ liệu mặc định: {ex.Message}");
                }
            }
            app.Run();


        }
    }
}
