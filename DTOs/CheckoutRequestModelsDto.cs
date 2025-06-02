namespace Ticket_Booking_System.DTOs
{
    public record CreateCheckoutSessionRequest (decimal Amount ,String Currency = "usd");
    
}
