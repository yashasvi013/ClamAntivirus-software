namespace ClamAVMicroservice.Services
{
    public interface IEmailService
    {
        Task SendInfectedFileAlertAsync(String filename, string virusName, string virusInfo);
    }
}
