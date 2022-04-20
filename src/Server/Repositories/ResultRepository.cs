using Dapper;
using Microsoft.Data.SqlClient;
using Phrazy.Shared.Models;

namespace Phrazy.Server.Repositories;

public interface IResultRepository
{
	Task SaveResult(string deviceID, string puzzleID, int score, int seconds, int? rank, DateTime timeStamp, bool isWin, string results);
	Task<LastResultPayload> GetLastCalculatedResultByDeviceID(string deviceID);
}

public class ResultRepository : IResultRepository
{
	private readonly IConfig _config;

	public ResultRepository(IConfig config)
	{
		_config = config;
	}

	public async Task SaveResult(string deviceID, string puzzleID, int score, int seconds, int? rank, DateTime timeStamp, bool isWin, string results)
	{
		var connection = new SqlConnection(_config.DatabaseConnectionString);
		await connection.ExecuteAsync("INSERT INTO Results (DeviceID, PuzzleID, Score, Seconds, Rank, TimeStamp, IsWin, Results) VALUES (@DeviceID, @PuzzleID, @Score, @Seconds, @Rank, @TimeStamp, @IsWin, @Results)", new {deviceID, puzzleID, score, seconds, rank, timeStamp, isWin, results});
	}

	public async Task<LastResultPayload> GetLastCalculatedResultByDeviceID(string deviceID)
	{
		var connection = new SqlConnection(_config.DatabaseConnectionString);
		LastResultPayload result = null!;
		result = await connection.QuerySingleOrDefaultAsync<LastResultPayload>(@"SELECT TOP 1 Phrase, [TimeStamp], UserCount, Score, Seconds, [Rank] 
FROM Results R JOIN Puzzles P ON R.PuzzleID = P.PuzzleID 
WHERE DeviceID = @deviceID AND [Rank] IS NOT NULL
ORDER BY [TimeStamp] DESC", new {deviceID});
		return result;
	}
}