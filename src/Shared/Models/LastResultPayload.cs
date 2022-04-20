namespace Phrazy.Shared.Models;

public class LastResultPayload
{
	public string Phrase { get; set; } = null!;
	public DateTime TimeStamp { get; set; }
	public int UserCount { get; set; }
	public int Score { get; set; }
	public int Seconds { get; set; }
	public int Rank { get; set; }
}