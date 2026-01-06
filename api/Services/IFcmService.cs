using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEstateHubAPI.Services
{
    public interface IFcmService
    {
        Task SendToTokenAsync(string token, string title, string body, Dictionary<string, string>? data = null);
        Task SendToTokensAsync(IEnumerable<string> tokens, string title, string body, Dictionary<string, string>? data = null);
        Task SendToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null);
    }
}


