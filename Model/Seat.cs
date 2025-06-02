using System.Text.Json.Serialization;
namespace Ticket_Booking_System.Model
{
    public class Seat
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        public bool IsBooked { get; set; } = false;

        public int? BookingId { get; set; }
        public Booking? Booking { get; set; }
    }

}
