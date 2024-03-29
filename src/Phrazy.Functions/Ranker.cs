﻿using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Phrazy.Functions;

public class Ranker
{
	public async Task ExecuteRanking(IConfiguration configuration, ILogger log)
	{
		var connectionString = configuration["DatabaseConnectionString"];
		await using var connection = new SqlConnection(connectionString);

		// get the most recent game
		var date = DateTime.UtcNow.AddDays(-1).Date;
		var puzzleID = await connection.QuerySingleOrDefaultAsync<string>("SELECT PuzzleID FROM Puzzles WHERE PlayDate = @PlayDate", new { PlayDate = date });

		if (string.IsNullOrEmpty(puzzleID))
		{
			log.LogInformation($"No puzzle found for {date}");
			return;
		}

		// update number of players
		var playerCountQuery = @"UPDATE Puzzles
SET UserCount = (SELECT COUNT(PuzzleID) FROM Results WHERE PuzzleID = @PuzzleID)
WHERE PuzzleID = @PuzzleID";
		await connection.ExecuteAsync(playerCountQuery, new {PuzzleID = puzzleID});

		// run the rank
		var rankQuery = @"WITH CTE AS
(
SELECT *, RANK() OVER (ORDER BY IsWin DESC, Score, Seconds) AS r
FROM Results
WHERE PuzzleID = @PuzzleID
)
UPDATE CTE
SET [Rank] = r;";
		await connection.ExecuteAsync(rankQuery, new {PuzzleID = puzzleID});

		log.LogInformation($"Calculated results on PuzzleID {puzzleID} for {date}");
	}
}
