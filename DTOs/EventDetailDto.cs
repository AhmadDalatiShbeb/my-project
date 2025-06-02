namespace Ticket_Booking_System.DTOs
{
    public class EventDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }

               public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
          }

}
