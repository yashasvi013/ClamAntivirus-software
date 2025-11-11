// EmailService.cs
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ClamAVMicroservice.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailClient _emailClient;
        private readonly string _senderAddress;
        private readonly string _adminEmail;

        public EmailService(IConfiguration configuration)
        {
            // Retrieve settings from appsettings.json
            var connectionString = configuration["EmailSettings:AcsConnectionString"];
            _senderAddress = configuration["EmailSettings:SenderAddress"];
            _adminEmail = configuration["EmailSettings:AdminEmail"];

            // Fail-fast validation: Ensure all settings are present.
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(_senderAddress) || string.IsNullOrEmpty(_adminEmail))
            {
                throw new InvalidOperationException("Email settings (AcsConnectionString, SenderAddress, AdminEmail) are not configured correctly in appsettings.json.");
            }

            // Create the client used to send emails.
            _emailClient = new EmailClient(connectionString);
        }

        public async Task SendInfectedFileAlertAsync(string fileName, string virusName, string virusInfo)
        {
            var subject = $"SECURITY ALERT: {virusName} Detected - {fileName}";
            var htmlContent = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .header {{ color: #D8000C; border-bottom: 2px solid #f0f0f0; padding-bottom: 10px; margin-bottom: 20px; }}
        .details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .virus-info {{ background-color: #fff3f3; padding: 15px; border-left: 4px solid #D8000C; margin: 15px 0; }}
        .footer {{ margin-top: 20px; font-size: 0.9em; color: #666; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Security Alert: {virusName} Detected</h1>
    </div>
    
    <p>An attempt to upload an infected file to the system was detected and has been blocked.</p>
    
    <div class='details'>
        <h3>Threat Details</h3>
        <p><strong>File Name:</strong> {fileName}</p>
        <p><strong>Threat Name:</strong> {virusName}</p>
        <p><strong>Detected At (UTC):</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
    </div>
    
    <div class='virus-info'>
        <h3>About This Threat</h3>
        {virusInfo}
    </div>
    
    <div class='footer'>
        <p>This is an automated security alert. No immediate action is required, but please review this threat and take appropriate measures.</p>
        <p>For security reasons, please do not reply to this email.</p>
    </div>
</body>
</html>";

            var plainTextContent = $"SECURITY ALERT: {virusName} DETECTED\n\n" +
                               $"File: {fileName}\n" +
                               $"Detected At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n\n" +
                               $"THREAT INFORMATION:\n{virusInfo}\n\n" +
                               "This is an automated security alert. Please review this threat and take appropriate measures.";

            try
            {
                // Send the email. WaitUntil.Started returns once the operation has been queued for sending.
                await _emailClient.SendAsync(
                    WaitUntil.Started,
                    _senderAddress,
                    _adminEmail,
                    subject,
                    htmlContent,
                    plainTextContent);
            }
            catch (RequestFailedException ex)
            {
                // In a real application, you would use ILogger to log this exception.
                // For now, we write to the console. This prevents the app from crashing if email fails.
                Console.WriteLine($"Error sending security alert email: {ex.Message}");
            }
        }
    }
}