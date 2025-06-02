using System.ComponentModel.DataAnnotations.Schema;

namespace Ticket_Booking_System.Model
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        [Column(TypeName ="decimal(18 , 2)")]
        public decimal Price { get; set; }
        public string OrganizerId { get; set; } = null!;
        public User? Organizer { get; set; }

      
        public int? TotalTickets { get; set; }         public bool IsUnlimited { get; set; }  
        public List<Booking> Bookings { get; set; } = new();
    }

}
