using Phrazy.Client.Models;

namespace Phrazy.Client.Services
{
    public interface IGameEngine
    {
        string? Phrase { get; set; }
        Dictionary<string, KeyState>? KeyStates { get; set; }
        void ChooseLetter(string letter);
        List<List<PhraseLetterStateBox>> Start();
        event Action OnKeyPress;
    }

    public class GameEngine : IGameEngine
    {
        public event Action? OnKeyPress;

        public string? Phrase { get; set; }

        public Dictionary<string, KeyState>? KeyStates { get; set; }

        public List<PhraseLetterStateBox>? PhraseLetterStateBoxes { get; set; }

        public List<List<PhraseLetterStateBox>> Start()
        {
            Phrase = "too cool for bitchin' elementary school";
            KeyStates = new Dictionary<string, KeyState>();
            var alphabet = "abcdefghijklmnopqrstuvwxyz";
            var array = alphabet.ToCharArray();
            foreach (var letter in array)
                KeyStates.Add(letter.ToString(), KeyState.NotChosen);

            PhraseLetterStateBoxes = new List<PhraseLetterStateBox>();
            var wordsOfStateBoxes = new List<List<PhraseLetterStateBox>>();
            var words = Phrase.Split(' ');
            foreach (var word in words)
            {
                var wordOfStateBoxes = new List<PhraseLetterStateBox>();
                var chars = word.ToCharArray();
                foreach (var character in chars)
                {
                    var initialState = array.Contains(character) ? PhraseLetterState.NotGuessed : PhraseLetterState.Special;
                    var stateBox = new PhraseLetterStateBox
                        {Letter = character.ToString(), PhraseLetterState = initialState};
                    PhraseLetterStateBoxes.Add(stateBox);
                    wordOfStateBoxes.Add(stateBox);
                }
                wordsOfStateBoxes.Add(wordOfStateBoxes);
            }

            return wordsOfStateBoxes;
        }

        public void ChooseLetter(string letter)
        {
            if (KeyStates![letter] == KeyState.NotChosen)
                KeyStates[letter] = KeyState.Hit;

            var hit = false;
            foreach (var stateBox in PhraseLetterStateBoxes!)
            {
                if (stateBox.Letter == letter)
                {
                    stateBox.PhraseLetterState = PhraseLetterState.Guessed;
                    hit = true;
                }
            }

            KeyStates[letter] = hit ? KeyStates![letter] = KeyState.Hit : KeyStates![letter] = KeyState.Miss;

            OnKeyPress?.Invoke();
        }
    }
}
