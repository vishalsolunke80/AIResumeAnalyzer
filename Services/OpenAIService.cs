using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace AIResumeAnalyzer.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> AnalyzeResumeAsync(string resumeText, string jobDescription)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY") 
            {
                return "Error: OpenAI API Key is missing. Please configure it in appsettings.json.";
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5083"); // Required by OpenRouter

            var prompt = $@"Compare this resume with the job description.

Return:
1. Match score (0–100)
2. Strengths
3. Missing skills
4. Suggestions.

Resume:
{resumeText}

Job Description:
{jobDescription}";

            var request = new
            {
                model = "openrouter/free",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful ATS resume analyzer." },
                    new { role = "user", content = prompt }
                }
            };

            try 
            {
                var response = await _httpClient.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error from OpenRouter: {response.ReasonPhrase}. Details: {errorContent}";
                }

                var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
                return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response from AI.";
            }
            catch (Exception ex)
            {
                return $"Exception calling OpenAI: {ex.Message}";
            }
        }

        public class OpenAIResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; }
        }

        public class Choice
        {
            [JsonPropertyName("message")]
            public Message Message { get; set; }
        }

        public class Message
        {
            [JsonPropertyName("content")]
            public string Content { get; set; }
        }
    }
}
