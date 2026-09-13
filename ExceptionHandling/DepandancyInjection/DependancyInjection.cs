using ExceptionHandling.Data;
using ExceptionHandling.Models;
using ExceptionHandling.Service;
using JwtAuthApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.ComponentModel.Design;
using System.Text;

namespace ExceptionHandling.DepandancyInjection
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddTodatabase(this  IServiceCollection services,IConfiguration configuration)
        {
             services.AddDbContext<ApplicationDbContext>(
             options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentityCore<User>()
                           .AddRoles<IdentityRole<int>>()
                           .AddSignInManager()
                            .AddEntityFrameworkStores<ApplicationDbContext>();
            return services;
        }
        public static IServiceCollection AddServices (this IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddScoped<IAuthService, AuthService>();
            return serviceDescriptors;
        }
     
        public static IServiceCollection JwtAuthenticationService(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters//ինչ կանոներով ստուգենք JWT Token ը և ստեղծում ենք նոր Object
                    {
                        ValidateIssuerSigningKey = true,//«Ստուգիր՝ JWT Token-ի signature-ը (ստորագրությունը) արդյո՞ք ճիշտ է և ստեղծվա՞ծ է մեր Secret Key-ով»։
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"])//Վերցնում է Secret key և դարձնում է byte զանգված և դրանով ստուգում
                        ),
                        ValidateIssuer = true,//Ստուգի՞ր, թե ով է թողարկել Token-ը։
                        ValidIssuer = configuration["Jwt:Issuer"],//Ո՞վ պետք է լինի թողարկողը։

                        ValidateAudience = true,//→ Ո՞ւմ համար է նախատեսված Token-ը։
                        ValidAudience = configuration["Jwt:Audience"],//Ահա այն Audience-ը, որը պետք է լինի Token-ի մեջ։

                        ValidateLifetime = true,//
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();
            return services;
        }
    }
}
