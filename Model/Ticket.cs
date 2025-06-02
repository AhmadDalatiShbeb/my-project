namespace Ticket_Booking_System.Model
{
    public class Ticket
    {
       
            public int Id { get; set; }
            public int BookingId { get; set; }
            public Booking Booking { get; set; } = null!;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public string TicketUrl { get; set; } = string.Empty;
            public string? QrPath { get; set; }
            public string FilePath { get; set; } = string.Empty;



    }
}
