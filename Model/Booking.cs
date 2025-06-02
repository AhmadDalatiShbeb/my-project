using System.Text.Json.Serialization;
namespace Ticket_Booking_System.Model
{
    public class Booking
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }

        public string UserId { get; set; } = null!;
        public User? User { get; set; }


        public string Status { get; set; } = "Panding";
        public String? StripeSessionId { get; set; }

        public DateTime BookingDate { get; set; }
        public int NumberOfTickets { get; set; }

        public Ticket? Ticket { get; set; }

    }

}
