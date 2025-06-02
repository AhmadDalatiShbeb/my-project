using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Ticket_Booking_System.Data;
using Ticket_Booking_System.DTOs;


namespace Ticket_Booking_System.EndPoints.Organizers
{
    public static class OrganizerEndpoints
    {
        public static RouteGroupBuilder MapOrganizerEndpoints(this IEndpointRouteBuilder routes)
        {

            var group = routes.MapGroup("/organizer").RequireAuthorization();



                        group.MapGet("/organizer-events", async (
                HttpContext httpContext,
                AppDbContext db,
                IMapper mapper) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Results.Unauthorized();

                var events = await db.Events
                    .Where(e => e.OrganizerId == userId.ToString())
                    .Include(e => e.Bookings)
                    .Include(e => e.Organizer)
                    
                    .ToListAsync();

                if (events == null || !events.Any())
                    return Results.NotFound("لا توجد فعاليات لهذا المنظم");

                var eventDtos = mapper.Map<List<EventDetailDto>>(events);
                return Results.Ok(eventDtos);
            })
                .WithName("View Event")
                .RequireAuthorization(Ticket_Booking_System.Enums.UserRole.Organizer.ToString());



                        group.MapPost("/events", async (
                CreateEventDto createEventDto,
                AppDbContext db,
                IMapper mapper,
                HttpContext httpContext) =>
            {

                if (!createEventDto.IsUnlimited && (!createEventDto.TotalTickets.HasValue || createEventDto.TotalTickets <= 0))
                {
                    return Results.BadRequest("عدد التذاكر مطلوب إذا لم تكن الفعالية غير محدودة.");
                }

                
                var newEvent = mapper.Map<Ticket_Booking_System.Model.Event>(createEventDto);
                newEvent.OrganizerId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value; 

                db.Events.Add(newEvent);
                await db.SaveChangesAsync();

                var eventDto = mapper.Map<EventDetailDto>(newEvent);
                return Results.Created($"/events/{newEvent.Id}", eventDto);
            })
                .WithName("Create Event")
                .RequireAuthorization(Ticket_Booking_System.Enums.UserRole.Organizer.ToString());



            group.MapPut("/events/{id}", async (
           int id,
           CreateEventDto updatedEvent,
           AppDbContext db,
           HttpContext httpContext) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var ev = await db.Events.FindAsync(id);

                if (ev == null || ev.OrganizerId != userId.ToString())
                    return Results.NotFound("حدث غير موجود أو ليس لك");

                ev.Name = updatedEvent.Name;
                ev.Location = updatedEvent.Location;
                ev.Date = updatedEvent.Date;
                ev.Price = updatedEvent.Price;

                await db.SaveChangesAsync();
                return Results.Ok(ev);
            })
                .WithName("Edit Event")
                .RequireAuthorization(Ticket_Booking_System.Enums.UserRole.Organizer.ToString());

                        group.MapDelete("/events/{id}", async (
                int id,
                AppDbContext db,
                HttpContext httpContext) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var ev = await db.Events.FindAsync(id);

                if (ev == null || ev.OrganizerId != userId.ToString())
                    return Results.NotFound("حدث غير موجود أو ليس لك");

                db.Events.Remove(ev);
                await db.SaveChangesAsync();
                return Results.Ok("تم حذف الفعالية");
            })
                .WithName("Delete Event")
                .RequireAuthorization(Ticket_Booking_System.Enums.UserRole.Organizer.ToString());

            return group;

        }

    }
}
