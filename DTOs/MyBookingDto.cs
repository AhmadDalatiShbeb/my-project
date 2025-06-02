namespace Ticket_Booking_System.DTOs
{
    using System;

    namespace Ticket_Booking_System.DTOs
    {
                                public class MyBookingDto
        {
                        public string Username { get; set; } = string.Empty;             public int Id { get; set; }             public string Name { get; set; } = string.Empty;             public String Location { get; set; } = String.Empty;             public DateTime Date { get; set; } 
           
            public int NumberOfTickets { get; set; }           

                        public decimal Price { get; set; }             public decimal TotalAmount { get; set; }             public DateTime BookingDate { get; set; }             public string OrganizerUsername { get; set; } = string.Empty; 
            public string Status { get; set; } = "Panding";

                                }
    }

}
