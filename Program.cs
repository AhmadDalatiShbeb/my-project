using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;
using Ticket_Booking_System.Data;
using Ticket_Booking_System.EndPoints.Admin;
using Ticket_Booking_System.EndPoints.Organizers;
using Ticket_Booking_System.EndPoints.RegisterLogin;
using Ticket_Booking_System.EndPoints.Stripe;
using Ticket_Booking_System.EndPoints.Users;
using Ticket_Booking_System.Mapping;
using Ticket_Booking_System.Model;
using Ticket_Booking_System.Services;



public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAutoMapper(typeof(MappingProfile));

        


        builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<User, IdentityRole>(options =>
      {             options.Password.RequireDigit = true;
          options.Password.RequireLowercase = true;
          options.Password.RequireNonAlphanumeric = false; 
          options.Password.RequireUppercase = true;
          options.Password.RequiredLength = 6;
          options.Password.RequiredUniqueChars = 1;

                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
          options.Lockout.MaxFailedAccessAttempts = 5;
          options.Lockout.AllowedForNewUsers = true;

                    options.User.RequireUniqueEmail = true;
      })
          .AddEntityFrameworkStores<AppDbContext>()
          .AddDefaultTokenProviders();


                builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();


        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            });




                builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine("Authentication Failed: " + context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("Token Validated");
                    return Task.CompletedTask;
                }
            };
        });


                builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Ticket_Booking_System.Enums.UserRole.Admin.ToString(), policy => policy.RequireRole(Ticket_Booking_System.Enums.UserRole.Admin.ToString()));
            options.AddPolicy(Ticket_Booking_System.Enums.UserRole.Organizer.ToString(), policy => policy.RequireRole(Ticket_Booking_System.Enums.UserRole.Organizer.ToString()));
            options.AddPolicy(Ticket_Booking_System.Enums.UserRole.User.ToString(), policy => policy.RequireRole(Ticket_Booking_System.Enums.UserRole.User.ToString()));      
        });
        builder.Services.AddEndpointsApiExplorer();


        builder.Services.AddScoped<ITokenService, Ticket_Booking_System.Services.TokenService>();



                builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ticket Booking API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "أدخل التوكن على الشكل التالي: Bearer {your token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
            });
        });


        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();

        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapUserEndpoints();
        app.MapOrganizerEndpoints();
        app.MapStripeEndpoints();
        app.MapRegisterLoginEndPoints();
        app.MapAdminEndPoints();
        await SeedDataAsync();

        //var stripeSecretKey = app.Configuration["Stripe:SecretKey"];

        async Task SeedDataAsync()
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

              
                string[] roleNames = Enum.GetNames(typeof(Ticket_Booking_System.Enums.UserRole)); 
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        Console.WriteLine($"Role '{roleName}' created successfully."); 
                    }
                }

                
               var adminEmail = ""; 
              var adminPassword = ""; 

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        UserName = adminEmail, 
                        Email = adminEmail,
                        FirstName = "Ahmad",
                        LastName = "Dalati",
                        RegistrationDate = DateTime.UtcNow
                    };
                    var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);

                    if (createAdminResult.Succeeded)
                    {
                        
                        await userManager.AddToRoleAsync(adminUser, Ticket_Booking_System.Enums.UserRole.Admin.ToString());
                        Console.WriteLine($"Admin user '{adminEmail}' created and assigned Admin role.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create admin user: {string.Join(", ", createAdminResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

        }


        app.Run();
    }
}