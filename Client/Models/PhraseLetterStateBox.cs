namespace Phrazy.Client.Models;

public class PhraseLetterStateBox
{
    public string Letter { get; set; }
    public PhraseLetterState PhraseLetterState { get; set; }
}

public enum PhraseLetterState
{
    NotGuessed,
    Guessed
}