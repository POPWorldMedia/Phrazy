namespace Phrazy.Client.Models;

public class PhraseLetterStateBox
{
    public string Letter { get; set; } = null!;
    public string SolveLetter { get; set; } = null!;
    public PhraseLetterState PhraseLetterState { get; set; }
    public bool IsFocus { get; set; }
}

public enum PhraseLetterState
{
    NotGuessed,
    Guessed,
    Special,
    Solve
}