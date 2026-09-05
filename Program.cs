using ScoreboardClient;

// Ben's backend
//IScoreboardApi api = new ScoreboardApi();

IScoreboardApi api = new FirebaseScoreboardApi();
bool running = true;

while (running)
{
    Console.WriteLine();
    Console.WriteLine("=== ONLINE SCOREBOARD ===");
    Console.WriteLine();
    Console.WriteLine("1. Submit Score");
    Console.WriteLine("2. View Scoreboard");
    Console.WriteLine("3. Exit");
    Console.WriteLine();
    Console.Write("Choose: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await SubmitScoreFlow(api);
            break;

        case "2":
            await ViewScoreboardFlow(api);
            break;

        case "3":
            running = false;
            break;

        default:
            Console.WriteLine("Invalid option, please choose 1, 2 or 3.");
            break;
    }
}

Console.WriteLine();
Console.WriteLine("Goodbye!");

// POST a new score. Validates name and score first
static async Task SubmitScoreFlow(IScoreboardApi api)
{
    Console.WriteLine();
    Console.Write("Name: ");
    string? name = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Name must not be empty.");
        return;
    }

    Console.Write("Score: ");
    string? scoreInput = Console.ReadLine();

    if (!int.TryParse(scoreInput, out int score))
    {
        Console.WriteLine("Score must be a whole number.");
        return;
    }

    if (score < 0)
    {
        Console.WriteLine("Score must not be negative.");
        return;
    }

    ScoreEntry entry = new ScoreEntry { Name = name.Trim(), Score = score };

    Console.WriteLine();
    Console.WriteLine("Submitting...");

    try
    {
        HttpResponseMessage response = await api.SubmitScoreAsync(entry);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Score submitted!");
        }
        else
        {
            Console.WriteLine($"The server rejected the score ({(int)response.StatusCode} {response.StatusCode}).");
        }
    }
    catch (HttpRequestException)
    {
        Console.WriteLine("Could not connect to the server.");
    }
}

// GET the scoreboard, show the top 10, then a small submenu
static async Task ViewScoreboardFlow(IScoreboardApi api)
{
    Console.WriteLine();
    Console.WriteLine("Loading scoreboard...");

    List<ScoreEntry> scores;

    try
    {
        scores = await api.GetScoreboardAsync();
    }
    catch (HttpRequestException)
    {
        Console.WriteLine("Could not connect to the server.");
        return;
    }

    if (scores.Count == 0)
    {
        Console.WriteLine("No scores submitted yet.");
        return;
    }

    List<ScoreEntry> topScores = scores
        .OrderByDescending(entry => entry.Score)
        .Take(10)
        .ToList();

    Console.WriteLine();
    Console.WriteLine($"=== LEADERBOARD (Top {topScores.Count} of {scores.Count}) ===");
    Console.WriteLine();

    int rank = 1;
    foreach (ScoreEntry entry in topScores)
    {
        Console.WriteLine($"{rank}. {entry.Name,-15}{entry.Score}");
        rank++;
    }

    bool viewingScoreboard = true;

    while (viewingScoreboard)
    {
        Console.WriteLine();
        Console.WriteLine("1. View All Scores");
        Console.WriteLine("2. Cheat Mode");
        Console.WriteLine("3. Back to Main Menu");
        Console.Write("Choose: ");

        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                PrintAllScores(scores);
                break;

            case "2":
                await CheatModeFlow(api, scores);

                // Re-fetch so View All Scores / Cheat Mode show the real current state
                try
                {
                    scores = await api.GetScoreboardAsync();
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("Could not refresh from the server. Showing previous data.");
                }
                break;

            case "3":
                viewingScoreboard = false;
                break;

            default:
                Console.WriteLine("Invalid option, please choose 1, 2 or 3.");
                break;
        }
    }
}

// Print every score, sorted, no ids
static void PrintAllScores(List<ScoreEntry> scores)
{
    if (scores.Count == 0)
    {
        Console.WriteLine();
        Console.WriteLine("No scores left.");
        return;
    }

    List<ScoreEntry> sorted = scores
        .OrderByDescending(entry => entry.Score)
        .ToList();

    Console.WriteLine();
    Console.WriteLine($"=== ALL SCORES ({sorted.Count}) ===");
    Console.WriteLine();

    int rank = 1;
    foreach (ScoreEntry entry in sorted)
    {
        Console.WriteLine($"{rank}. {entry.Name,-15}{entry.Score}");
        rank++;
    }
}

// Shows all scores with ids. Lets you pick one to change or delete
static async Task CheatModeFlow(IScoreboardApi api, List<ScoreEntry> allEntries)
{
    if (allEntries.Count == 0)
    {
        Console.WriteLine();
        Console.WriteLine("No scores left to edit.");
        return;
    }

    List<ScoreEntry> sortedEntries = allEntries
        .OrderByDescending(entry => entry.Score)
        .ToList();

    Console.WriteLine();
    Console.WriteLine("=== FULL SCOREBOARD (CHEAT MODE) ===");
    Console.WriteLine();

    for (int i = 0; i < sortedEntries.Count; i++)
    {
        ScoreEntry entry = sortedEntries[i];
        string idLabel = string.IsNullOrEmpty(entry.Id) ? "n/a" : entry.Id;
        //Console.WriteLine($"{i + 1}. {entry.Name,-15}{entry.Score,-8}(id: {idLabel})");
        Console.WriteLine($"{i + 1}. {entry.Name,-15}{entry.Score}");
    }

    Console.WriteLine();
    Console.WriteLine("1. Change Score");
    Console.WriteLine("2. Delete Entry");
    Console.WriteLine("3. Back");
    Console.Write("Choose: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await CheatChangeFlow(api, sortedEntries);
            break;

        case "2":
            await CheatDeleteFlow(api, sortedEntries);
            break;
    }
}

// PUT: change the score of the chosen row
static async Task CheatChangeFlow(IScoreboardApi api, List<ScoreEntry> entries)
{
    Console.Write("Row: ");
    string? rowInput = Console.ReadLine();

    if (!int.TryParse(rowInput, out int row) || row < 1 || row > entries.Count)
    {
        Console.WriteLine("Invalid row.");
        return;
    }

    ScoreEntry target = entries[row - 1];

    Console.Write("New score: ");
    string? scoreInput = Console.ReadLine();

    if (!int.TryParse(scoreInput, out int newScore) || newScore < 0)
    {
        Console.WriteLine("Score must be a whole, non-negative number.");
        return;
    }

    // PUT sends the full entry, not just the new score
    ScoreEntry updated = new ScoreEntry
    {
        Id = target.Id,
        Name = target.Name,
        Score = newScore
    };

    try
    {
        HttpResponseMessage response = await api.UpdateScoreAsync(updated);

        Console.WriteLine(response.IsSuccessStatusCode
            ? "Score updated."
            : $"The server rejected the update ({(int)response.StatusCode} {response.StatusCode}).");
    }
    catch (HttpRequestException)
    {
        Console.WriteLine("Could not connect to the server.");
    }
    catch (NotSupportedException ex)
    {
        Console.WriteLine(ex.Message);
    }
}

// DELETE: remove the chosen row
static async Task CheatDeleteFlow(IScoreboardApi api, List<ScoreEntry> entries)
{
    Console.Write("Row: ");
    string? rowInput = Console.ReadLine();

    if (!int.TryParse(rowInput, out int row) || row < 1 || row > entries.Count)
    {
        Console.WriteLine("Invalid row.");
        return;
    }

    ScoreEntry target = entries[row - 1];

    try
    {
        HttpResponseMessage response = await api.DeleteScoreAsync(target.Id);

        Console.WriteLine(response.IsSuccessStatusCode
            ? "Entry deleted."
            : $"The server rejected the delete ({(int)response.StatusCode} {response.StatusCode}).");
    }
    catch (HttpRequestException)
    {
        Console.WriteLine("Could not connect to the server.");
    }
    catch (NotSupportedException ex)
    {
        Console.WriteLine(ex.Message);
    }
}