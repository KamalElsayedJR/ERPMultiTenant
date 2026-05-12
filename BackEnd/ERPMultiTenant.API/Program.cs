
using ERPMultiTenant.API.Middleware;
using ERPMultiTenant.API.Responses;
using ERPMultiTenant.Application;
using ERPMultiTenant.Application.Authentication;
using ERPMultiTenant.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

namespace ERPMultiTenant.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, loggerConfiguration) =>
                loggerConfiguration.ReadFrom.Configuration(context.Configuration));
            
            builder.Services.AddCors();
            builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                        RoleClaimType = "role",
                        NameClaimType = "userId",
                        ClockSkew = TimeSpan.Zero
                    };
                    options.MapInboundClaims = false;
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(ApiResponse<string>.Fail("Unauthorized"));
                        },
                        OnForbidden = async context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(ApiResponse<string>.Fail("Forbidden"));
                        }
                    };
                });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseCors(policy =>
    policy.AllowAnyHeader()
          .AllowAnyMethod()
          .AllowAnyOrigin());
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
