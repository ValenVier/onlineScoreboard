using System.Net.Http.Json;

namespace ScoreboardClient;

//NOT IN USE ANYMORE
// Backend using the assignment's endpoints
public class ScoreboardApi : IScoreboardApi
{
    private readonly HttpClient _client = new HttpClient();

    private const string PostUrl = "https://hooks.zapier.com/hooks/catch/8338993/ujs9jj9/";

    private const string GetUrl = "https://script.google.com/macros/s/AKfycbys5aEPMvNCutyhNYYCcQcCjzsi2UtqNspmKyCH-AicJxJbCJMrAoT0LUaYaXhTWA8n/exec";

    // POST a new score
    public async Task<HttpResponseMessage> SubmitScoreAsync(ScoreEntry entry)
    {
        return await _client.PostAsJsonAsync(PostUrl, entry);
    }

    // GET all scores as a list
    public async Task<List<ScoreEntry>> GetScoreboardAsync()
    {
        List<ScoreEntry>? scores =
            await _client.GetFromJsonAsync<List<ScoreEntry>>(GetUrl);

        return scores ?? new List<ScoreEntry>();
    }

    // Not supported: this backend has no way to target one row
    public Task<HttpResponseMessage> UpdateScoreAsync(ScoreEntry entry)
    {
        throw new NotSupportedException(
            "Not supported here. This backend only does POST and GET. Use FirebaseScoreboardApi for PUT/DELETE.");
    }

    // Not supported: same reason
    public Task<HttpResponseMessage> DeleteScoreAsync(string id)
    {
        throw new NotSupportedException(
            "Not supported here. This backend only does POST and GET. Use FirebaseScoreboardApi for PUT/DELETE.");
    }
}