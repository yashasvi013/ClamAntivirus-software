using System.Text;

namespace InsurancePolicyForm.Services
{
    public class VirusScanService : IVirusScanService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public VirusScanService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> ScanFileAsync(IFormFile file)
        {
            try
            {
                // Get the ClamAV microservice URL from configuration
                var clamAvBaseUrl = _configuration["ClamAVMicroservice:BaseUrl"] ?? "http://localhost:5000";
                var scanEndpoint = $"{clamAvBaseUrl}/api/Scan/scan";

                using var content = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);
                
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                content.Add(streamContent, "File", file.FileName);

                var response = await _httpClient.PostAsync(scanEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return result.Trim('"'); // Remove quotes if present
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error: {response.StatusCode} - {errorContent}";
                }
            }
            catch (HttpRequestException ex)
            {
                return $"Network Error: Unable to connect to virus scanning service. {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
