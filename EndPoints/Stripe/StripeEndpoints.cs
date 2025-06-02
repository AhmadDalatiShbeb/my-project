using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using Ticket_Booking_System.Data;
using Ticket_Booking_System.Model;


namespace Ticket_Booking_System.EndPoints.Stripe
{
    public static class StripeEndpoints
    {
        public static void MapStripeEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapPost("/bookings/checkout/{eventId}", [Authorize] async (int eventId, HttpContext http, AppDbContext db, IConfiguration config) =>
            {
                var userId = int.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                                var existingBooking = await db.Bookings
                    .FirstOrDefaultAsync(b => b.UserId == userId.ToString() && b.EventId == eventId && b.Status == "Paid");

                if (existingBooking != null)
                    return Results.BadRequest("لقد قمت بالحجز مسبقًا لهذا الحدث.");

                var evt = await db.Events.FindAsync(eventId);
                if (evt == null) return Results.NotFound("الفعالية غير موجودة");

                var domain = "https:                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = evt.Price * 100,                     Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = evt.Name
                    }
                },
                Quantity = 1
            }
        },
                    Mode = "payment",
                    SuccessUrl = domain + "/success?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = domain + "/cancel"
                };

                var service = new SessionService();
                Session session = await service.CreateAsync(options);

                                var booking = new Booking
                {
                    EventId = eventId,
                    UserId = userId.ToString(),
                    Status = "Pending",
                    StripeSessionId = session.Id
                };

                db.Bookings.Add(booking);
                await db.SaveChangesAsync();

                return Results.Ok(new { url = session.Url });
            });









            routes.MapPost("/stripe-webhook", async (HttpContext http, AppDbContext db, IConfiguration config) =>
            {
                var webhookSecret = config["Stripe:WebhookSecret"];
                string json = await new StreamReader(http.Request.Body).ReadToEndAsync();

                try
                {
                    var stripeEvent = EventUtility.ConstructEvent(
                        json,
                        http.Request.Headers["Stripe-Signature"],
                        webhookSecret
                    );

                    if (stripeEvent.Type == "checkout.session.completed")
                    {
                        var session = stripeEvent.Data.Object as Session;

                        if (session != null)
                        {
                            var booking = await db.Bookings
                                .Include(b => b.Event)
                                .FirstOrDefaultAsync(b => b.StripeSessionId == session.Id);

                            if (booking != null && booking.Status != "Paid")
                            {
                                booking.Status = "Paid";
                                await db.SaveChangesAsync();
                                Console.WriteLine($"✅ تم تأكيد الدفع للحجز {booking.Id}");

                                var eventEntity = await db.Events.FindAsync(booking.EventId);
                                if (eventEntity == null)
                                    return Results.BadRequest("حدث غير موجود لإنشاء التذكرة.");

                                                                var ticketUrl = $"https:                                var qrBytes = QrCodeGenerator.GenerateQrCode(ticketUrl);

                                var qrFileName = $"qr_{booking.Id}_{DateTime.UtcNow.Ticks}.png";
                                var qrPath = Path.Combine("wwwroot/qrcodes", qrFileName);
                                Directory.CreateDirectory("wwwroot/qrcodes");
                                await System.IO.File.WriteAllBytesAsync(qrPath, qrBytes);

                                                                var pdfBytes = TicketPdfGenerator.GenerateTicket(booking, eventEntity, qrBytes);
                                var fileName = $"ticket_{booking.Id}_{DateTime.UtcNow.Ticks}.pdf";
                                var ticketFilePath = Path.Combine("wwwroot/tickets", fileName);
                                Directory.CreateDirectory("wwwroot/tickets");
                                await System.IO.File.WriteAllBytesAsync(ticketFilePath, pdfBytes);

                                                                var ticket = new Ticket
                                {
                                    BookingId = booking.Id,
                                    FilePath = $"/tickets/{fileName}",
                                    QrPath = $"/qrcodes/{qrFileName}"
                                };

                                db.Tickets.Add(ticket);
                                await db.SaveChangesAsync();
                                Console.WriteLine($"🎫 تم إنشاء التذكرة للحجز {booking.Id}");
                            }
                        }
                    }

                    return Results.Ok();
                }
                catch (StripeException e)
                {
                    Console.WriteLine($"❌ Stripe Webhook Error: {e.Message}");
                    return Results.BadRequest($"Webhook Error: {e.Message}");
                }
            });




        }
    }
}
