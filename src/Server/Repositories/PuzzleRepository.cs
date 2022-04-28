using Dapper;
using Microsoft.Data.SqlClient;
using Phrazy.Server.Models;

namespace Phrazy.Server.Repositories;

public interface IPuzzleRepository
{
	Task<PuzzleRecord?> GetPuzzleByDate(DateTime date);
}

public class PuzzleRepository : IPuzzleRepository
{
	private readonly IConfig _config;
	private readonly ICacheHelper _cacheHelper;

	public PuzzleRepository(IConfig config, ICacheHelper cacheHelper)
	{
		_config = config;
		_cacheHelper = cacheHelper;
	}

	public async Task<PuzzleRecord?> GetPuzzleByDate(DateTime date)
	{
		var key = "p" + date.Ticks;
		var cachedObject = _cacheHelper.GetCacheObject<PuzzleRecord?>(key);
		if (cachedObject != null)
			return cachedObject;
		var connection = new SqlConnection(_config.DatabaseConnectionString);
		var payload = await connection.QuerySingleOrDefaultAsync<PuzzleRecord?>("SELECT PuzzleID, Phrase, PlayDate FROM Puzzles WHERE PlayDate = @PlayDate", new {PlayDate = date});
		if (payload != null)
			_cacheHelper.SetCacheObject(key, payload, DateTime.UtcNow.AddDays(2));
		return payload;
	}
}