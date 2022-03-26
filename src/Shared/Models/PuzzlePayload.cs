namespace Phrazy.Shared.Models
{
	public class PuzzlePayload
	{
		public string PuzzleID { get; set; } = null!;
		public string Puzzle { get; set; } = null!;
		public DateTime Date { get; set; }
		public string Hash { get; set; } = null!;
	}
}
