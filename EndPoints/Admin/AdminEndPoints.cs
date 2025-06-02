using Microsoft.AspNetCore.Identity;
using Ticket_Booking_System.Enums;
using Ticket_Booking_System.Model;
namespace Ticket_Booking_System.EndPoints.Admin
{
    public static class AdminEndPoints
    {
        public static  void MapAdminEndPoints(this IEndpointRouteBuilder routes)
        {

            routes.MapPost("/assign-organizer-role/{userId}", async (string userId, UserManager<User> userManger) =>

                {
                    var user = await userManger.FindByIdAsync(userId);

                    if (user == null)
                     {
                        return Results.NotFound("المستخدم غير موجود");
                    }


                    if (await userManger.IsInRoleAsync(user, UserRole.Organizer.ToString()))
                    {
                        Results.Conflict("المستخدم لديه الدور بالفعل");
                    }

                    var result = await userManger.AddToRoleAsync(user, UserRole.Organizer.ToString());


                    if (result.Succeeded)
                    {
                        return Results.Ok(new { message = $" تم تعيين منظم للمستخدم  {user.UserName} بنجاح." });
                    }
                    else
                    {
                        return Results.BadRequest(new { message = $"فشل تعيين الدور ", errors = result.Errors.Select(e => e.Description) });
                    }

                })
                .RequireAuthorization(Ticket_Booking_System.Enums.UserRole.Admin.ToString());

           
            routes.MapPost("/assign-admin-role/{userId}", async (string userId, UserManager<User> userManger) =>
            {
            var user = await userManger.FindByIdAsync(userId);

            if (user == null)
            {
                return Results.NotFound("المستخدم غير موجود");
            }

            if (await userManger.IsInRoleAsync(user, UserRole.Admin.ToString()))
            {
                Results.Conflict("المستخدم لديه الدور بالفعل");
            }
            var result = await userManger.AddToRoleAsync(user, UserRole.Admin.ToString());

                if (result.Succeeded)
                {
                    return Results.Ok(new { message = $"تم تعيين أدمن للمتسخدم {user.UserName}بنحاح." });
                }
                else
                {
                    return Results.BadRequest(new { message = $"فشل تعينن الدور.", errors = result.Errors.Select(e => e.Description) });
                }
            })
              .RequireAuthorization(Ticket_Booking_System.Enums.UserRole.Admin.ToString());
           
        }
    }
}

