using System.Net.Http.Json;
using System.Text.Json;

namespace PC_Fix_AI_Assistant.Services;

public class GeminiService(HttpClient http)
{
    public async Task<string> SendMessageAsync(List<ChatMessage> history)
    {
        var contents = history.Select(m =>
        {
            var parts = new List<object>();
            if (m.ImageBase64 != null)
                parts.Add(new { inline_data = new { mime_type = m.ImageMediaType, data = m.ImageBase64 } });
            parts.Add(new { text = m.Content });
            return new { role = m.Role == "assistant" ? "model" : "user", parts };
        }).ToList();

        var payload = new { contents };

        var response = await http.PostAsJsonAsync("http://localhost:5000/api/chat", payload);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
        {
            var error = json.TryGetProperty("error", out var e) ? e.GetProperty("message").GetString() : "Error desconocido";
            throw new Exception(error ?? "Error desconocido");
        }

        return json.GetProperty("candidates")[0]
                   .GetProperty("content")
                   .GetProperty("parts")[0]
                   .GetProperty("text").GetString() ?? "";
    }
}

public class ChatMessage
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = "";
    public string? ImageBase64 { get; set; }
    public string? ImageMediaType { get; set; }
}
