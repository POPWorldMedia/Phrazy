namespace Phrazy.Shared.Models
{
	public class PuzzlePayload
	{
		public string PuzzleID { get; set; } = null!;
		public string Phrase { get; set; } = null!;
		public DateTime PlayDate { get; set; }
		public string Hash { get; set; } = null!;
	}
}
