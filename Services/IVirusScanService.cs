namespace InsurancePolicyForm.Services
{
    public interface IVirusScanService
    {
        Task<string> ScanFileAsync(IFormFile file);
    }
}
