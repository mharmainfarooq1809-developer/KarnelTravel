namespace KarnelTravel.Services;

public interface IQrCodeService
{
    byte[] GeneratePng(string text);
}
