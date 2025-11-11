using Microsoft.AspNetCore.Mvc;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Azure.AI.OpenAI.Chat;
using System.ClientModel;
using Azure;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text.Json;
using ClamAVMicroservice.Models;

namespace ClamAVMicroservice.Services
{
    public class GenAIService : IGenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly string _endpoint;
        private readonly string _deploymentName;
        private readonly string _openAiApiKey;

        public GenAIService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            // Retrieve settings from appsettings.json
            _endpoint = _configuration["Settings:OpenAIEndpoint"];
            _deploymentName = _configuration["Settings:DeploymentName"];
            _openAiApiKey = _configuration["Settings:OpenAIAPIKey"];

            // Fail-fast validation: Ensure all settings are present
            if (string.IsNullOrEmpty(_endpoint) || string.IsNullOrEmpty(_deploymentName) || string.IsNullOrEmpty(_openAiApiKey))
            {
                throw new InvalidOperationException("GenAI settings (OpenAIEndpoint, DeploymentName, OpenAIAPIKey) are not configured correctly in appsettings.json.");
            }
        }

        public async Task<GenAI> GetVirusInfoAsync(string virusName)
        {
            var prompt = $"Provide detailed information about the virus/malware named '{virusName}'. " +
                        $"Include: 1) What it does, 2) How it spreads, 3) Potential impact, and 4) Recommended actions. " +
                        $"If the virus name is not recognized, state that the virus is unknown and provide general security recommendations.";
            
            return await GetGPTResponseAsync(prompt);
        }

        public async Task<GenAI> GetGPTResponseAsync(string prompt)
        {
            var response = new GenAI();

            try
            {
                AzureKeyCredential credential = new AzureKeyCredential(_openAiApiKey);

                // Initialize the AzureOpenAIClient
                AzureOpenAIClient azureClient = new(new Uri(_endpoint), credential);

                // Initialize the ChatClient with the specified deployment name
                ChatClient chatClient = azureClient.GetChatClient(_deploymentName);

                // Create a list of chat messages
                var messages = new List<ChatMessage>
                {
                    new UserChatMessage("Give a quick and short summary in plain English for Non-Technical Stakeholders around " + prompt),
                };

                // Create chat completion options
                var options = new ChatCompletionOptions
                {
                    Temperature = (float)1,
                    MaxOutputTokenCount = 800,
                    TopP = (float)1,
                    FrequencyPenalty = (float)0,
                    PresencePenalty = (float)0
                };

                // Create the chat completion request
                ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

                foreach (var content in completion?.Content ?? [])
                {
                    response.Content += $"{content.Text}";
                }

                if (response.Content != null && response.Content.Contains("The requested information is not available in the retrieved data"))
                {
                    response.Content = "I do not have enough information regarding this error to guide you. Please contact the Engineering team for more details.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred in GenAI service: {ex.Message}");
                response.Content = "Unable to generate AI response at this time. Please try again later.";
            }

            return response;
        }
    }
}
