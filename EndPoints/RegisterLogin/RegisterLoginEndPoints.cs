using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Ticket_Booking_System.DTOs;
using Ticket_Booking_System.Enums;
using Ticket_Booking_System.Model;
using Ticket_Booking_System.Services;

namespace Ticket_Booking_System.EndPoints.RegisterLogin
{

    public static class RegisterLoginEndPoints
    {

        public static void MapRegisterLoginEndPoints(this IEndpointRouteBuilder routes)
        {
            routes.MapPost("/register", async (RegisterDto dto, UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService) =>
            {
                if (!string.Equals(dto.Password, dto.ConfirmPassword, StringComparison.Ordinal))
                {
                    return Results.BadRequest(new { message = "كلمة المرور وتأكيد كلمة المرور غير متطابقين." });
                }

                var newUser = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    UserName = dto.Email,                     RegistrationDate = DateTime.UtcNow
                    
                };

                                var result = await userManager.CreateAsync(newUser, dto.Password);

                if (result.Succeeded)
                {
                                        await userManager.AddToRoleAsync(newUser,UserRole.User.ToString());

                    return Results.Created($"/user/{newUser.Id}", new { message = "تم التسجيل بنجاح!" });
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return Results.BadRequest(new { message = "فشل التسجيل", errors = errors });
                }
            })
               .WithName("Register")
               .AllowAnonymous();



            routes.MapPost("/login", async (
                UserLoginDto credentials,
                ILogger<Program> logger ,UserManager<User> userManager, SignInManager<User>signInManager,ITokenService tokenService ) =>
            {
                logger.LogInformation("بدأت عملية تسجيل الدخول للمستخدم: {Username}", credentials.Email);
                try
                {
                    var user = await userManager.FindByEmailAsync(credentials.Email);

                    if (user == null)
                    {
                        return Results.Unauthorized();
                    }
                    var result = await signInManager.CheckPasswordSignInAsync(user, credentials.Password, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        var token = tokenService.GenerateJwtToken(user, roles);

                        return Results.Ok(new { message = "تم تسجيل الدخول بنجاح!", token = token });
                    }
                    else if (result.IsLockedOut)
                    {
                        return Results.BadRequest(new { message = "تم قفل الحساب بسبب محاولات تسجيل دخول فاشلة كثيرة." });
                    }
                    else if (result.IsNotAllowed)
                    {
                        return Results.BadRequest(new { message = "تسجيل الدخول غير مسموح به لهذا الحساب. يرجى التحقق من بريدك الإلكتروني أو الاتصال بالدعم." });
                    }
                    else
                    {
                        return Results.Unauthorized();
                    }
                }
                catch (Exception ex)
                {
                                        logger.LogError(ex, "حدث خطأ أثناء تسجيل الدخول للمستخدم: {Username}", credentials.Email);
                    return Results.Problem("حدث خطأ داخلي في الخادم");
                }
            }).WithName("Login")
            .AllowAnonymous();


            routes.MapGet("/user/me", async (ClaimsPrincipal userPrincipal, UserManager<User> userManager) =>
            {
                if (userPrincipal.Identity?.IsAuthenticated == true)
                {
                    var user = await userManager.GetUserAsync(userPrincipal);
                    if (user != null)
                    {
                        return Results.Ok(new
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            RegistrationDate = user.RegistrationDate
                        });
                    }
                }
                return Results.Unauthorized();
            })
           .RequireAuthorization();

            routes.MapPost("/logout", async (SignInManager<User> signInManager) =>
            {
                                                                await signInManager.SignOutAsync();
                return Results.Ok(new { message = "تم تسجيل الخروج بنجاح." });
            })
            .WithName("Logout")
            .RequireAuthorization();
        }
    }
}
