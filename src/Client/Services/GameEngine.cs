using Phrazy.Client.Models;

namespace Phrazy.Client.Services
{
    public interface IGameEngine
    {
        string? Phrase { get; }
        Dictionary<string, KeyState>? KeyStates { get; }
        List<GuessRecord>? GuessRecords { get; }
        bool IsSolveMode { get; }
        void ChooseLetter(string letter);
        List<List<PhraseLetterStateBox>> Start();
        event Action OnKeyPress;
        event Action? OnSolveModeChange;
        void ToggleSolveMode();
        void SolveBackspace();
    }

    public class GameEngine : IGameEngine
    {
        public event Action? OnKeyPress;
        public event Action? OnSolveModeChange;

        public string? Phrase { get; private set; }
        public Dictionary<string, KeyState>? KeyStates { get; private set; }
        public List<PhraseLetterStateBox>? PhraseLetterStateBoxes { get; private set; }
        public List<GuessRecord>? GuessRecords { get; private set; }
        public bool IsSolveMode { get; private set; }

        public List<List<PhraseLetterStateBox>> Start()
        {
            Phrase = "too cool for bitchin' elementary school";

            // init the stuff
            KeyStates = new Dictionary<string, KeyState>();
            var alphabet = "abcdefghijklmnopqrstuvwxyz";
            var array = alphabet.ToCharArray();
            foreach (var letter in array)
                KeyStates.Add(letter.ToString(), KeyState.NotChosen);
            GuessRecords = new List<GuessRecord>();
            IsSolveMode = false;

            // divy up the letters
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
            if (letter == " ")
            {
                if (IsSolveMode)
                    // ignore space if you're in solve mode
                    return;
                // keyboard input to enter solve mode
                ToggleSolveMode();
                return;
            }
            if (letter == "backspace")
            {
                // keyboard input to backspace in solve mode
                if (!IsSolveMode)
                    // enter into solve mode
                {
                    ToggleSolveMode();
                    return;
                }
                SolveBackspace();
                return;
            }

            if (IsSolveMode)
            {
                // solve mode
                var notGuessed = PhraseLetterStateBoxes!.Where(x => x.PhraseLetterState == PhraseLetterState.Solve).ToList();
                if (notGuessed.Any())
                {
                    var letterToTypeIn = notGuessed.FirstOrDefault(x => x.PhraseLetterState == PhraseLetterState.Solve && string.IsNullOrEmpty(x.SolveLetter));
                    if (letterToTypeIn != null)
                    {
                        letterToTypeIn.IsFocus = false;
                        letterToTypeIn.SolveLetter = letter;
                        var letterToFocus = notGuessed.FirstOrDefault(x => x.PhraseLetterState == PhraseLetterState.Solve && string.IsNullOrEmpty(x.SolveLetter));
                        if (letterToFocus != null)
                            letterToFocus.IsFocus = true;
                        else
                            {
                                // solve check
                            }
                    }
                }
                else
                {
                    // solve check
                }

                OnKeyPress?.Invoke();
            }
            else
            {
                // guessing mode
                if (KeyStates![letter] != KeyState.NotChosen)
                    return;

                var hit = false;
                foreach (var stateBox in PhraseLetterStateBoxes!)
                {
                    if (stateBox.Letter == letter)
                    {
                        stateBox.PhraseLetterState = PhraseLetterState.Guessed;
                        hit = true;
                    }
                }

                GuessRecords?.Add(new GuessRecord {IsCorrect = hit, Letter = letter});

                KeyStates[letter] = hit ? KeyStates![letter] = KeyState.Hit : KeyStates![letter] = KeyState.Miss;

                OnKeyPress?.Invoke();
            }
        }

        public void ToggleSolveMode()
        {
            if (IsSolveMode)
            {
                // go back to regular guess mode
                var lettersInSolveMode = PhraseLetterStateBoxes!.Where(x => x.PhraseLetterState == PhraseLetterState.Solve).ToList();
                foreach (var letter in lettersInSolveMode)
                {
                    letter.PhraseLetterState = PhraseLetterState.NotGuessed;
                    letter.IsFocus = false;
                    letter.SolveLetter = string.Empty;
                }
            }
            else
            {
                // go into solve mode
                var lettersInNotGuessedMode = PhraseLetterStateBoxes!.Where(x => x.PhraseLetterState == PhraseLetterState.NotGuessed).ToList();
                if (lettersInNotGuessedMode.Any())
                    lettersInNotGuessedMode[0].IsFocus = true;
                foreach (var letter in lettersInNotGuessedMode)
                    letter.PhraseLetterState = PhraseLetterState.Solve;
            }
            IsSolveMode = !IsSolveMode;
            OnSolveModeChange?.Invoke();
            OnKeyPress?.Invoke();
        }

        public void SolveBackspace()
        {
            // if you can backspace
            var lastLetterInGuessWithSolveLetter = PhraseLetterStateBoxes!.FindLast(x => !string.IsNullOrEmpty(x.SolveLetter));
            if (lastLetterInGuessWithSolveLetter != null)
            {
                ClearAllSolveFocus();
                lastLetterInGuessWithSolveLetter.IsFocus = true;
                lastLetterInGuessWithSolveLetter.SolveLetter = string.Empty;
                OnKeyPress?.Invoke();
            }
            else
            // no more backspace
            ToggleSolveMode();
        }

        private void ClearAllSolveFocus()
        {
            foreach (var item in PhraseLetterStateBoxes!)
                item.IsFocus = false;
        }
    }
}
