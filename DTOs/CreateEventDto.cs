namespace Ticket_Booking_System.DTOs
{
    public class CreateEventDto
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }

        public int? TotalTickets { get; set; }
      
        public bool IsUnlimited { get; set; }
    }
}