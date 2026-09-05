namespace ScoreboardClient;

// Interface
// POST -> SubmitScoreAsync
// GET -> GetScoreboardAsync
// PUT -> UpdateScoreAsync
// DELETE -> DeleteScoreAsync

public interface IScoreboardApi
{
    Task<HttpResponseMessage> SubmitScoreAsync(ScoreEntry entry);
    Task<List<ScoreEntry>> GetScoreboardAsync();
    Task<HttpResponseMessage> UpdateScoreAsync(ScoreEntry entry);
    Task<HttpResponseMessage> DeleteScoreAsync(string id);
}