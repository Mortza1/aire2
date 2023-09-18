using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace aire2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatBotController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ChatBotController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        [HttpPost]
        public async Task<IActionResult> GenerateResponse([FromBody] PromptModel prompt)
        {
            try
            {
                string apiKey = "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";
                string apiUrl = "https://api.openai.com/v1/chat/completions";

                // Create a request object
                var requestModel = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                new { role = "system", content = "You are an educational chatbot in an app called Aire. You will help solve queries according to A level syllabus. Our app only provides physics for now. We also have basic and premium subscriptions which have a stronger chatbot. 50 word limit." },
                new { role = "user", content = prompt.Text }
            }
                };
                string requestBody = JsonSerializer.Serialize(requestModel);

                // Prepare the HTTP request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Headers.Add("Authorization", "Bearer " + apiKey);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                // Send the HTTP request
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                // Process the response
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    if (responseObject.TryGetProperty("choices", out JsonElement choicesElement) && choicesElement.ValueKind == JsonValueKind.Array)
                    {
                        var contents = choicesElement.EnumerateArray()
                            .Select(choice => choice.GetProperty("message").GetProperty("content").GetString())
                            .ToList();

                        return Ok(contents);
                    }
                    else
                    {
                        return BadRequest("Invalid response format");
                    }
                }
                else
                {
                    return StatusCode((int)response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                return StatusCode(500, ex.Message);
            }
        }

        public class PromptModel
        {
            public string Text { get; set; }
        }


    }
}

