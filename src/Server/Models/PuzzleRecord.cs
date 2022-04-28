namespace Phrazy.Server.Models;

public class PuzzleRecord
{
	public string PuzzleID { get; set; } = null!;
	public string Phrase { get; set; } = null!;
	public DateTime PlayDate { get; set; }
}