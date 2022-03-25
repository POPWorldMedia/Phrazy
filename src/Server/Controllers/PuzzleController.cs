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
		public async Task<PuzzlePayload> GetWithIdentifier(string id)
		{
			var payload =await _puzzleService.GetPayloadForToday(id);
			return payload;
		}
	}
}
