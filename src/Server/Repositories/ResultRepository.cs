using Dapper;
using Microsoft.Data.SqlClient;

namespace Phrazy.Server.Repositories;

public interface IResultRepository
{
	Task SaveResult(string deviceID, string puzzleID, int score, int? rank, DateTime timeStamp, bool isWin, string results);
}

public class ResultRepository : IResultRepository
{
	private readonly IConfig _config;
public ResultRepository(IConfig config)
{
	_config = config;
}
public async Task SaveResult(string deviceID, string puzzleID, int score, int? rank, DateTime timeStamp, bool isWin, string results)
{
		var connection = new SqlConnection(_config.DatabaseConnectionString);
		await connection.ExecuteAsync("INSERT INTO Results (DeviceID, PuzzleID, Score, Rank, TimeStamp, IsWin, Results) VALUES (@DeviceID, @PuzzleID, @Score, @Rank, @TimeStamp, @IsWin, @Results)", new { deviceID, puzzleID, score, rank, timeStamp, isWin, results });
	}
}