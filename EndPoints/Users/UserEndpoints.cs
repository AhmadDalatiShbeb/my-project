using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Ticket_Booking_System.Data;
using Ticket_Booking_System.DTOs;
using Ticket_Booking_System.DTOs.Ticket_Booking_System.DTOs;
using Ticket_Booking_System.Enums;
using Ticket_Booking_System.Model;

namespace Ticket_Booking_System.EndPoints.Users
{
    public static class UserEndpoints
    {
        public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
        {

            var group = routes.MapGroup("/user").RequireAuthorization();
            group.MapGet("/events", [Authorize] async (
    AppDbContext db,
    IMapper mapper,
    HttpContext httpContext) =>
{
    var role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

    if (string.IsNullOrEmpty(role) || role != "User" && role != "Organizer")
        return Results.Forbid();

    var events = await db.Events
        .Include(e => e.Organizer)
        .ToListAsync();

    var eventListDtos = mapper.Map<List<EventListDto>>(events);
    return Results.Ok(eventListDtos);
})
    .WithName("View Event to User")
    .RequireAuthorization(Ticket_Booking_System.Enums.UserRole.User.ToString());





            group.MapGet("/events/{id}", [Authorize] async (int id, HttpContext httpContext, AppDbContext db, IMapper mapper) =>
            {
                var evt = await db.Events
                    .Include(e => e.Organizer)
                    .Include(e => e.Bookings)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (evt is null)
                    return Results.NotFound("الفعالية غير موجودة");

                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(userIdClaim, out int userId);
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

                var eventDetailsDto = mapper.Map<EventDetailDto>(evt);

                int total = evt.TotalTickets ?? 0;
                int booked = evt.Bookings.Count;
                var totalBookedTickets = evt.Bookings != null ? evt.Bookings.Sum(b => b.NumberOfTickets) : 0;
                var availableSeats = (evt.TotalTickets ?? 0) - totalBookedTickets;

                eventDetailsDto.TotalSeats = evt.IsUnlimited ? -1 : total;
                eventDetailsDto.AvailableSeats = evt.IsUnlimited ? -1 : availableSeats;




                return Results.Ok(eventDetailsDto);
            })
                .WithName("View Event to User BY Id")
                .RequireAuthorization(Ticket_Booking_System.Enums.UserRole.User.ToString());


            group.MapPost("/bookings", [Authorize] async (
    HttpContext httpContext,
    BookingRequestDto bookingDto,
    AppDbContext db) =>
{
    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        return Results.Unauthorized();

    var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
    if (userRole == UserRole.Organizer.ToString())
        return Results.BadRequest("لا يمكن للمنظم إجراء الحجز.");





    var eventEntity = await db.Events
        .Include(e => e.Bookings)
        .FirstOrDefaultAsync(e => e.Id == bookingDto.EventId);


    if (eventEntity == null)
        return Results.NotFound("الفعالية غير موجودة.");


    var hasUserAlreadyBooked = eventEntity.Bookings.Any(b => b.UserId == userId.ToString());
    if (hasUserAlreadyBooked)
        return Results.BadRequest("لقد قمت بالفعل بحجز تذاكر لهذه الفعالية.");


    if (bookingDto.NumberOfTickets <= 0)
        return Results.BadRequest("يرجى تحديد عدد تذاكر صالح.");

    if (!eventEntity.IsUnlimited)
    {
        var totalBooked = eventEntity.Bookings.Sum(b => b.NumberOfTickets);
        var available = eventEntity.TotalTickets - totalBooked;

        if (bookingDto.NumberOfTickets > available)
            return Results.BadRequest("عدد التذاكر المطلوبة غير متوفر.");
    }

    var booking = new Booking
    {
        EventId = eventEntity.Id,
        UserId = userId.ToString(),
        BookingDate = DateTime.UtcNow,
        NumberOfTickets = bookingDto.NumberOfTickets
    };

    db.Bookings.Add(booking);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        message = "تم الحجز بنجاح.",
        bookingId = booking.Id,
        totalAmount = bookingDto.NumberOfTickets * eventEntity.Price
    });
})
    .WithName("Booking Event to User")
    .RequireAuthorization(Ticket_Booking_System.Enums.UserRole.User.ToString());



            group.MapGet("/my-bookings", [Authorize] async (HttpContext httpContext, AppDbContext db) =>
{
    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
    {
        return Results.Unauthorized();
    }

    var bookings = await db.Bookings
    .Where(b => b.UserId == userId.ToString())
    .Include(b => b.Event)
    .ThenInclude(e => e!.Organizer)
    .ToListAsync();

    if (bookings == null || !bookings.Any())
    {
        return Results.Ok(new List<MyBookingDto>());
    }

    var myBookingsDtoList = bookings.Select(b => new MyBookingDto
    {
        Id = b.Id,
        BookingDate = b.BookingDate,
        NumberOfTickets = b.NumberOfTickets,
        Name = b.Event?.Name ?? string.Empty,
        Location = b.Event?.Location ?? string.Empty,
        Date = b.Event?.Date ?? default,
        Price = b.Event?.Price ?? default,
        OrganizerUsername = b.Event?.Organizer?.UserName ?? string.Empty,
        TotalAmount = b.NumberOfTickets * b.Event!.Price,
        Username = httpContext.User?.Identity?.Name ?? string.Empty,
        Status = b.Status
    }).ToList();
    return Results.Ok(myBookingsDtoList);
})
.WithName("View Booking to User")
.RequireAuthorization(Ticket_Booking_System.Enums.UserRole.User.ToString());


            group.MapPut("/bookings/{id}", async (
int id,
[FromBody] BookingUpdateRequest updatedRequest, AppDbContext db,
HttpContext httpContext) =>
{
    var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var booking = await db.Bookings.FindAsync(id);

    if (booking == null || booking.UserId != userId.ToString())
        return Results.NotFound("الحجز غير موجود أو ليس لك");

    booking.NumberOfTickets = updatedRequest.NumberOfTicket;
    booking.BookingDate = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(booking);
})
    .WithName("Edit Event to User")
    .RequireAuthorization(Ticket_Booking_System.Enums.UserRole.User.ToString());


            group.MapDelete("/bookings/{id}", async (
    int id,
    AppDbContext db,
    HttpContext httpContext) =>
{
    var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var booking = await db.Bookings.FindAsync(id);

    if (booking == null || booking.UserId != userId.ToString())
        return Results.NotFound("الحجز غير موجود أو ليس لك");

    db.Bookings.Remove(booking);
    await db.SaveChangesAsync();
    return Results.Ok("تم إلغاء الحجز");
})
    .WithName("Delete Event to User")
    .RequireAuthorization(Ticket_Booking_System.Enums.UserRole.User.ToString());

            return group;
        }
    }
}
