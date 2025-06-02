using Ticket_Booking_System.Model;


namespace Ticket_Booking_System.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user, IList<string> roles);
    }
}
