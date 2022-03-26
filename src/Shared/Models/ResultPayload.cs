namespace Phrazy.Shared.Models;

public class ResultPayload
{
	public string DeviceID { get; set; } = null!;
	public string PuzzleID { get; set; } = null!;
	public Results Results { get; set; } = null!;
	public string Hash { get; set; } = null!;
}