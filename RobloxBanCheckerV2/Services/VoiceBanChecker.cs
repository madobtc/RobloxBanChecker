using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RobloxBanCheckerV2.Models;

namespace RobloxBanCheckerV2.Services
{
    public class VoiceBanChecker
    {
        private readonly HttpClient httpClient;

        public VoiceBanChecker()
        {
            var handler = new HttpClientHandler()
            {
                UseCookies = false
            };

            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        public void SetCookie(string cookie)
        {
            httpClient.DefaultRequestHeaders.Remove("Cookie");
            httpClient.DefaultRequestHeaders.Add("Cookie", $".ROBLOSECURITY={cookie}");
        }

        public async Task<UserInfo> TestConnection()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://users.roblox.com/v1/users/authenticated");
                var response = await httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("Cookie ungültig oder abgelaufen");

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"HTTP Fehler: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserInfo>(content);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Netzwerkfehler: {ex.Message}");
            }
            catch (JsonException ex)
            {
                throw new Exception($"JSON Parse Fehler: {ex.Message}");
            }
        }

        public async Task<VoiceStatusResponse> CheckVoiceBanStatus()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://voice.roblox.com/v1/settings");
                var response = await httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("Cookie ungültig oder abgelaufen");

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"HTTP Fehler: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VoiceStatusResponse>(content);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Netzwerkfehler: {ex.Message}");
            }
            catch (JsonException ex)
            {
                throw new Exception($"JSON Parse Fehler: {ex.Message}");
            }
        }
    }
}