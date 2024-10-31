using System.Configuration;
using System.Text;
using MySql.EntityFrameworkCore;
using Cokee.ClassService.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;

internal partial class Program
{
    private static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddUserSecrets(Assembly.GetExecutingAssembly())
                    .Build();
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var builder = WebApplication.CreateBuilder(args);
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JWTKey"]));    //key不能低于16位
        builder.Services.AddControllers(); // Add services to the container.
        builder.Services.AddDbContext<GlobalDbContext>(options =>
            options.UseMySQL(configuration.GetConnectionString(("DefaultConnection")))
        );
        builder.Services.AddAuthentication("Bearer").AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = "Cokee",
                ValidateAudience = true,
                ValidAudience = "CokeeClassService",
                RequireExpirationTime = true,
                ValidateLifetime = true,
            };
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cokee.ClassService.Server", Version = "v1" });

            // 添加身份验证
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            // 添加身份验证需求
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new string[] { }
                }
            });
        });
        var app = builder.Build(); // Configure the HTTP request pipeline.

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CokeeClass");
                // 添加身份验证界面
                c.InjectJavascript("/swagger-ui/custom.js");
            });
        }
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run("http://127.0.0.1:15043");
    }
}