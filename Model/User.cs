using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using Ticket_Booking_System.Enums;


namespace Ticket_Booking_System.Model
{
    public class User : IdentityUser
    {
        [JsonIgnore]
        
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; }  = null!;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public List<Event> Events { get; set; } = new();
        public UserRole Role { get; set; } = UserRole.User;

    }

}
