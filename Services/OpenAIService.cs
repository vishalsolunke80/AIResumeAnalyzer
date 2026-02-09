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
            var apiKey = _configuration["OpenAI:ApiKey"]?.Trim();
            
            // Secure diagnostic logging
            if (!string.IsNullOrEmpty(apiKey))
            {
                var maskedKey = apiKey.Length > 15 
                    ? $"{apiKey.Substring(0, 8)}...{apiKey.Substring(apiKey.Length - 4)}" 
                    : "[KEY TOO SHORT]";
                Console.WriteLine($"DEBUG: API Key identified. Length: {apiKey.Length}, Preview: {maskedKey}");
            }

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY") 
            {
                return "Error: OpenAI API Key is missing. Please configure it in your environment variables (OpenAI__ApiKey).";
            }

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

            var requestBody = new
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
                var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions")
                {
                    Content = JsonContent.Create(requestBody)
                };

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                request.Headers.Add("HTTP-Referer", "https://render.com"); // More appropriate for Render
                request.Headers.Add("X-Title", "AI Resume Analyzer");

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error from OpenRouter: {response.StatusCode} ({response.ReasonPhrase}). Details: {errorContent}";
                }

                var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
                return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response from AI.";
            }
            catch (Exception ex)
            {
                return $"Exception calling AI API: {ex.Message}";
            }
        }

        public class OpenAIResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice>? Choices { get; set; }
        }

        public class Choice
        {
            [JsonPropertyName("message")]
            public Message? Message { get; set; }
        }

        public class Message
        {
            [JsonPropertyName("content")]
            public string? Content { get; set; }
        }
    }
}
