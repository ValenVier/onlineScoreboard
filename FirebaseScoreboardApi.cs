using System.Net.Http.Json;

namespace ScoreboardClient;

// Backend using Firebase Realtime Database
// Supports all 4 verbs, because each entry has its own key (id)
//
// note: Firebase GET returns an OBJECT, not an array: { "-Oabc123": { "name": "Alice", "score": 9000 }, ... }
// so it is deserialized as Dictionary<string, ScoreEntry> and copy each key into entry.Id

public class FirebaseScoreboardApi : IScoreboardApi
{
    private readonly HttpClient _client = new HttpClient();

    private const string DatabaseUrl = "https://scoreboard-prj-default-rtdb.europe-west1.firebasedatabase.app";

    private const string ScoresPath = "scores";

    private static string ScoresUrl => $"{DatabaseUrl}/{ScoresPath}.json";
    private static string ScoreUrl(string id) => $"{DatabaseUrl}/{ScoresPath}/{id}.json";

    // POST creates a new entry with an auto-generated key from Firebase
    public async Task<HttpResponseMessage> SubmitScoreAsync(ScoreEntry entry)
    {
        return await _client.PostAsJsonAsync(ScoresUrl, entry);
    }

    // GET all entries, flattened into a list with Id set
    public async Task<List<ScoreEntry>> GetScoreboardAsync()
    {
        // If empty, Firebase returns "null"
        Dictionary<string, ScoreEntry>? raw =
            await _client.GetFromJsonAsync<Dictionary<string, ScoreEntry>>(ScoresUrl);

        if (raw == null)
        {
            return new List<ScoreEntry>();
        }

        List<ScoreEntry> entries = new List<ScoreEntry>();

        foreach (KeyValuePair<string, ScoreEntry> pair in raw)
        {
            pair.Value.Id = pair.Key;
            entries.Add(pair.Value);
        }

        return entries;
    }

    // PUT replaces one entry, entry.Id must be set
    public async Task<HttpResponseMessage> UpdateScoreAsync(ScoreEntry entry)
    {
        if (string.IsNullOrEmpty(entry.Id))
        {
            throw new ArgumentException("entry.Id must be set.", nameof(entry));
        }

        return await _client.PutAsJsonAsync(ScoreUrl(entry.Id), entry);
    }

    // DELETE removes one entry by id
    public async Task<HttpResponseMessage> DeleteScoreAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("id must not be empty.", nameof(id));
        }

        return await _client.DeleteAsync(ScoreUrl(id));
    }
}