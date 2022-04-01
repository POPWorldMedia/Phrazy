using Dapper;
using Microsoft.Data.SqlClient;
using Phrazy.Shared.Models;

namespace Phrazy.Server.Repositories;

public interface IPuzzleRepository
{
	Task<PuzzlePayload?> GetPuzzleByDate(DateTime date);
}

public class PuzzleRepository : IPuzzleRepository
{
	private readonly IConfig _config;

	public PuzzleRepository(IConfig config)
	{
		_config = config;
	}

	public async Task<PuzzlePayload?> GetPuzzleByDate(DateTime date)
	{
		var connection = new SqlConnection(_config.DatabaseConnectionString);
		var payload = await connection.QuerySingleOrDefaultAsync<PuzzlePayload?>("SELECT PuzzleID, Phrase, PlayDate FROM Puzzles WHERE PlayDate = @PlayDate", new {PlayDate = date});
		return payload;
	}
}