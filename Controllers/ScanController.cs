using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;
using ClamAVMicroservice.Models;
using System.Net;
using ClamAVMicroservice.Services;

namespace ClamAVMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScanController : ControllerBase
    {
        private const string ClamAVHost = "localhost";
        private const int ClamAVPort = 3310;

        private readonly IEmailService _emailService;
        private readonly IGenAIService _genAIService;

        public ScanController(IEmailService emailService, IGenAIService genAIService)
        {
            _emailService = emailService;
            _genAIService = genAIService;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanFile([FromForm] ScanRequest scanRequest)
        {
            var file = scanRequest.File;
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var tempFilePath = Path.GetTempFileName();

            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var scanResult = await ScanWithClamAV(tempFilePath, file.FileName);

                return Ok(scanResult);
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }

        private async Task<string> ScanWithClamAV(string filePath, string originalFileName)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ClamAVHost, ClamAVPort);

                using var stream = client.GetStream();

                var instreamCommand = Encoding.ASCII.GetBytes("zINSTREAM\0");
                await stream.WriteAsync(instreamCommand, 0, instreamCommand.Length);

                using var fileStream = System.IO.File.OpenRead(filePath);
                var buffer = new byte[2048];
                int bytesRead;

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    var sizeBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(bytesRead));
                    await stream.WriteAsync(sizeBytes, 0, 4);
                    await stream.WriteAsync(buffer, 0, bytesRead);
                }

                var zeroBytes = BitConverter.GetBytes(0);
                await stream.WriteAsync(zeroBytes, 0, 4);

                var responseBuffer = new byte[256];
                var responseLength = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                var response = Encoding.ASCII.GetString(responseBuffer, 0, responseLength).Trim();

                if (response.Contains("OK"))
                {
                    return "Clean";
                }
                else if (response.Contains("FOUND"))
                {
                    var virusName = response.Replace("stream: ", "").Trim();
                    
                    var aiResponse = await _genAIService.GetVirusInfoAsync(virusName);
                    var virusInfo = aiResponse?.Content ?? "No additional information available.";
                    
                    await _emailService.SendInfectedFileAlertAsync(originalFileName, virusName, virusInfo);

                    return $"Infected: {virusName + virusInfo}";
             
                }
                else
                {
                    return $"Unknown response: {response}";
                }
            }
            catch (SocketException ex)
            {
                return $"Error: Cannot connect to ClamAV daemon. Make sure ClamAV is running on {ClamAVHost}:{ClamAVPort}. Details: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ClamAVHost, ClamAVPort);

                using var stream = client.GetStream();

                var pingCommand = Encoding.ASCII.GetBytes("zPING\0");
                await stream.WriteAsync(pingCommand, 0, pingCommand.Length);

                var responseBuffer = new byte[256];
                var responseLength = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                var response = Encoding.ASCII.GetString(responseBuffer, 0, responseLength).Trim();

                if (response.Contains("PONG"))
                {
                    return Ok(new { status = "Healthy", message = "ClamAV daemon is responsive" });
                }
                else
                {
                    return StatusCode(503, new { status = "Unhealthy", message = $"Unexpected response: {response}" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "Unhealthy", message = $"Cannot connect to ClamAV: {ex.Message}" });
            }
        }
    }
}