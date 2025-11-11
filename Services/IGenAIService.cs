using ClamAVMicroservice.Models;

namespace ClamAVMicroservice.Services
{
    public interface IGenAIService
    {
        Task<GenAI> GetVirusInfoAsync(string virusName);
        Task<GenAI> GetGPTResponseAsync(string prompt);
    }
}
