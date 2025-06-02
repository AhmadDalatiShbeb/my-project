using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Ticket_Booking_System.Model;


public static class TicketPdfGenerator
{
    public static byte[] GenerateTicket(Booking booking, Event @event, byte[] qrCodeImage)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A5);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(16));

                page.Content()
                    .Column(col =>
                    {
                        col.Item().Text("تذكرة الدخول").Bold().FontSize(24).AlignCenter();
                        col.Item().Text($"رقم الحجز: {booking.Id}");
                        col.Item().Text($"اسم الفعالية: {@event.Name}");
                        col.Item().Text($"عدد التذاكر: {booking.NumberOfTickets}");
                        col.Item().Text($"تاريخ الحجز: {booking.BookingDate.ToShortDateString()}");
                        col.Item().Image(qrCodeImage);
                    });
            });
        });

        return document.GeneratePdf();
    }
}
