using System.Text.Json.Serialization;
using Phrazy.Shared.Models;

namespace Phrazy.Client.Models;

public class GameState
{
	public GameState()
	{
		EncodedString = string.Empty;
		PhraseLetterStateBoxes = new List<PhraseLetterStateBox>();
		GuessRecords = new List<GuessRecord>();
		IsSolveMode = false;
		IsGameOver = false;
		KeyStates = new Dictionary<string, KeyState>();
		var alphabet = "abcdefghijklmnopqrstuvwxyz";
		var array = alphabet.ToCharArray();
		foreach (var letter in array)
			KeyStates.Add(letter.ToString(), KeyState.NotChosen);
	}

	[JsonIgnore]
	public string Phrase
	{
		get
		{
			if (string.IsNullOrEmpty(EncodedString))
				return string.Empty;
			var base64EncodedBytes = Convert.FromBase64String(EncodedString);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
}
		set
		{

			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(value);
			EncodedString = Convert.ToBase64String(plainTextBytes);
		}
	}
	public string EncodedString { get; set; }
	public Dictionary<string, KeyState> KeyStates { get; set; }
	public List<PhraseLetterStateBox> PhraseLetterStateBoxes { get; set; }
	public List<GuessRecord> GuessRecords { get; set; }
	public bool IsSolveMode { get; set; }
	public Results? Results { get; set; }
	public bool IsGameOver { get; set; }
	public int Seconds { get; set; }
	public DateTime TimeStamp { get; set; }
	public PuzzleDefinition? PuzzleDefinition { get; set; }
}