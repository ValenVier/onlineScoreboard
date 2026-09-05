# Online Scoreboard Client

Network Programming
A C# console app that talks to a scoreboard API over HTTP.

## What this app does

- Submit a score (POST)
- View the leaderboard, top 10 (GET)
- View all scores (GET)
- Cheat mode: change a score (PUT) or delete an entry (DELETE)
- Handles network errors without crashing

## What was built

- Required part: POST + GET against the given endpoints (Zapier + Google Sheets)
  - This section has been replaced by my own database, although the configuration has been left in place for review
- Bonus #1: Top 10 leaderboard
- Bonus #3: input validation (empty name, invalid number, negative score)
- Bonus #4: networking code moved to its own class
- Bonus #5: "Loading..." message while waiting for a response
- Bonus #6: own Firebase Realtime Database, with PUT and DELETE added on top

## Files

| File             | Contains                                                 |
|------------------|----------------------------------------------------------|
| Program.cs       | Menu and all user-facing flows                           |
| ScoreEntry.cs    | The name and score model                                 |
| IScoreboardApi.cs | Interface: 4 REST methods any backend must have          |
| ScoreboardApi.cs | Backend using the given Zapier + Google Sheets endpoints |
| FirebaseScoreboardApi.cs| Backend using own Firebase Realtime Database             |

## Program.cs: functions

| Function | Does |
|---|---|
| top-level code | Main menu loop: Submit Score / View Scoreboard / Exit |
| SubmitScoreFlow | Asks name + score, validates, sends POST |
| ViewScoreboardFlow | Gets scores, shows top 10, then submenu: View All / Cheat Mode / Back |
| PrintAllScores | Prints every score, sorted, no ids |
| CheatModeFlow | Shows every score with its id, asks: Change / Delete / Back |
| CheatChangeFlow | Asks row + new score, sends PUT |
| CheatDeleteFlow | Asks row, sends DELETE |

## ScoreEntry.cs

```
public class ScoreEntry
{
    public string Name { get; set; }
    public int Score { get; set; }
    public string Id { get; set; } // Firebase key, never sent to the server
}
```

## IScoreboardApi.cs: the 4 methods

| Method | Verb | Does |
|---|---|---|
| SubmitScoreAsync | POST | Create a new score |
| GetScoreboardAsync | GET | Get all scores |
| UpdateScoreAsync | PUT | Replace one score |
| DeleteScoreAsync | DELETE | Remove one score |

## ScoreboardApi.cs: Teacher’s backend

- Supports: POST, GET
- Does not support: PUT, DELETE

## FirebaseScoreboardApi.cs (my backend)

- Supports: POST, GET, PUT, DELETE
- Firebase GET returns an object keyed by id, not an array, code converts it to a list and keeps each id


## How to switch backend

In Program.cs:

```
IScoreboardApi api = new FirebaseScoreboardApi(); // my Firebase DB
// IScoreboardApi api = new ScoreboardApi();     // given endpoints
```

## Requirement 4: network errors

HttpRequestException is caught everywhere a request is made. On failure, the app prints
"Could not connect to the server." and keeps running instead of crashing.