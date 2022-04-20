using Microsoft.AspNetCore.Mvc;
using Phrazy.Server.Services;
using Phrazy.Shared;
using Phrazy.Shared.Models;

namespace Phrazy.Server.Controllers
{
	[ApiController]
	public class PuzzleController : ControllerBase
	{
		private readonly IPuzzleService _puzzleService;

		public PuzzleController(IPuzzleService puzzleService)
		{
			_puzzleService = puzzleService;
		}

		[HttpGet(ApiPaths.Puzzle.GetWithIdentifier)]
		public async Task<PuzzlePayload> GetWithIdentifier(string id, [FromQuery]long ticks)
		{
			var date = new DateTime(ticks);
			var payload = await _puzzleService.GetPayloadForToday(id, date);
			return payload;
		}

		[HttpPut(ApiPaths.Puzzle.PutResult)]
		public async Task<IActionResult> PutResult(ResultPayload resultPayload)
		{
			var isHashMatch = await _puzzleService.SaveResult(resultPayload);
			if (isHashMatch)
				return Ok();

			return Unauthorized();
		}

		[HttpGet(ApiPaths.Puzzle.GetLastResult)]
		public async Task<LastResultPayload> GetLastResult(string id)
		{
			var result = await _puzzleService.GetLastResultByDeviceID(id);
			return result;
		}
	}
}
