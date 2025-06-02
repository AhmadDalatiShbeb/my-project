using QRCoder;



namespace Ticket_Booking_System.Model
{
    public class QrCodeGenerator
    {
        public static byte[] GenerateQrCode(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var pngQrCode = new PngByteQRCode(qrCodeData);
            return pngQrCode.GetGraphic(20); 
            
        }
    }
}
