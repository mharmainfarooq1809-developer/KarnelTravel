using QRCoder;

namespace KarnelTravel.Services;

public sealed class QrCodeService : IQrCodeService
{
    public byte[] GeneratePng(string text)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(data);
        return qr.GetGraphic(10);
    }
}
