using BloodDonationAPI.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BloodDonationAPI.Entities;
using BloodDonationAPI.Service.Impl;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BloodDonationAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Thêm CORS sau nay sua lai cho phu hop voi thuc te
            // /✅ Cho phép tất cả origin
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });


            // ?? Add DbContext with connection string from appsettings.json
            builder.Services.AddDbContext<BloodDonationSystemContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blood Donation API", Version = "v1" });
                
                // Thêm phần cấu hình JWT cho Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

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
                        []
                    }
                });

                // Thêm cấu hình để hiển thị enum dưới dạng string
                c.SchemaFilter<EnumSchemaFilter>();
            });            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<IAppointmentServiece, AppointmentServiece>();
            builder.Services.AddScoped<IDonorSearchService, DonorSearchService>();
            builder.Services.AddScoped<IBloodInventoryService, BloodInventoryService>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"])),
                        ValidateIssuer = false,
                        ValidateAudience = false
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

            // Bật CORS với tat ca các origin, headers và methods
            app.UseCors();
            // toi day het cors
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        // Thêm class EnumSchemaFilter vào project (có thể tạo file mới)
        public class EnumSchemaFilter : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                if (context.Type.IsEnum)
                {
                    schema.Enum.Clear();
                    foreach (var enumName in Enum.GetNames(context.Type))
                    {
                        schema.Enum.Add(new OpenApiString(enumName));
                    }
                }
            }
        }
    }
}
