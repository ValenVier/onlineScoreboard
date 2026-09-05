using System.Text.Json.Serialization;

namespace ScoreboardClient;

// Score data model. Used for POST, PUT, and GET
public class ScoreEntry
{
    public string Name { get; set; } = "";
    public int Score { get; set; }

    // Firebase key for this entry. Not sent to the server
    [JsonIgnore]
    public string Id { get; set; } = "";
}