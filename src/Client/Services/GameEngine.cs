using System.Text;
using Phrazy.Client.Models;

namespace Phrazy.Client.Services
{
    public interface IGameEngine
    {
        event Action OnKeyPress;
        event Action? OnSolveModeChange;
        event Action? OnWrongSolve;
        event Action<bool>? OnDialogOpen;
        event Action? OnBoardLoad;
        
        GameState GameState { get; }
        void ChooseLetter(string letter);
        Task<List<List<PhraseLetterStateBox>>> Start();
        void ToggleSolveMode();
        void SolveBackspace();
        void OpenDialog();
    }

    public class GameEngine : IGameEngine
    {
	    private readonly IPuzzleService _puzzleService;

	    public event Action? OnKeyPress;
        public event Action? OnSolveModeChange;
        public event Action? OnWrongSolve;
        public event Action<bool>? OnDialogOpen;
        public event Action? OnBoardLoad;

        public GameState GameState { get; private set; }

        public GameEngine(IPuzzleService puzzleService)
        {
	        _puzzleService = puzzleService;
	        GameState = new GameState();
        }

        public async Task<List<List<PhraseLetterStateBox>>> Start()
        {
	        var puzzleDefinition = await _puzzleService.GetCurrentPuzzle();

            GameState.Phrase = puzzleDefinition.Puzzle;

            // divy up the letters
            var wordsOfStateBoxes = new List<List<PhraseLetterStateBox>>();
            var words = GameState.Phrase.Split(' ');
            foreach (var word in words)
            {
                var wordOfStateBoxes = new List<PhraseLetterStateBox>();
                var chars = word.ToCharArray();
                var alphabet = "abcdefghijklmnopqrstuvwxyz";
                var array = alphabet.ToCharArray();
                foreach (var character in chars)
                {
                    var initialState = array.Contains(character) ? PhraseLetterState.NotGuessed : PhraseLetterState.Special;
                    var stateBox = new PhraseLetterStateBox
                        {Letter = character.ToString(), PhraseLetterState = initialState};
                    GameState.PhraseLetterStateBoxes.Add(stateBox);
                    wordOfStateBoxes.Add(stateBox);
                }
                wordsOfStateBoxes.Add(wordOfStateBoxes);
            }

            OnBoardLoad?.Invoke();

            return wordsOfStateBoxes;
        }

        public void ChooseLetter(string letter)
        {
            if (GameState.IsGameOver)
                return;

            if (letter == " ")
            {
                if (GameState.IsSolveMode)
                    // ignore space if you're in solve mode
                    return;
                // keyboard input to enter solve mode
                ToggleSolveMode();
                return;
            }
            if (letter == "backspace")
            {
                // keyboard input to backspace in solve mode
                if (!GameState.IsSolveMode)
                    // enter into solve mode
                {
                    ToggleSolveMode();
                    return;
                }
                SolveBackspace();
                return;
            }

            if (GameState.IsSolveMode)
            {
                // solve mode
                var notGuessed = GameState.PhraseLetterStateBoxes.Where(x => x.PhraseLetterState == PhraseLetterState.Solve).ToList();
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
                            SolveCheck();
                    }
                }
                else
                    SolveCheck();

                OnKeyPress?.Invoke();
            }
            else
            {
                // guessing mode
                if (GameState.KeyStates[letter] != KeyState.NotChosen)
                    return;

                var hit = false;
                foreach (var stateBox in GameState.PhraseLetterStateBoxes)
                {
                    if (stateBox.Letter == letter)
                    {
                        stateBox.PhraseLetterState = PhraseLetterState.Guessed;
                        hit = true;
                    }
                }

                GameState.GuessRecords.Add(new GuessRecord {IsCorrect = hit, Letter = letter});

                GameState.KeyStates[letter] = hit ? GameState.KeyStates[letter] = KeyState.Hit : GameState.KeyStates[letter] = KeyState.Miss;

                // all the letters are picked
                var lettersRemaining = GameState.KeyStates.Where(x => x.Value == KeyState.NotChosen);
                if (!lettersRemaining.Any())
                {
	                GameState.Results = new Results
	                {
		                IsWin = false,
		                LettersUsed = 26,
                        Score = 0
                        // TODO: time left
	                };
	                GameState.IsGameOver = true;
	                OpenDialog();
                }
                else
                {
	                // all possible hits are made
	                var notGuessed = GameState.PhraseLetterStateBoxes.Where(x => x.PhraseLetterState == PhraseLetterState.NotGuessed).ToList();
	                if (!notGuessed.Any())
	                {
		                var lettersUsed = GameState.GuessRecords.Count;
		                GameState.Results = new Results
		                {
			                IsWin = true,
			                LettersUsed = lettersUsed,
			                // TODO: score
			                // TODO: time left
		                };
		                GameState.IsGameOver = true;
		                OpenDialog();
	                }
                }

                OnKeyPress?.Invoke();
            }
        }

        private void SolveCheck()
        {
            var solveAttempt = new StringBuilder();
            foreach (var item in GameState.PhraseLetterStateBoxes)
            {
                if (item.PhraseLetterState == PhraseLetterState.Guessed || item.PhraseLetterState == PhraseLetterState.Special)
                    solveAttempt.Append(item.Letter);
                if (item.PhraseLetterState == PhraseLetterState.Solve && !string.IsNullOrEmpty(item.SolveLetter))
                    solveAttempt.Append(item.SolveLetter);
            }

            var scrubbedPhrase = GameState.Phrase.Replace(" ", "");
            if (scrubbedPhrase == solveAttempt.ToString())
            {
                // correct you win
                var lettersUsed = GameState.GuessRecords.Count;
                GameState.Results = new Results
                {
                    IsWin = true,
                    LettersUsed = lettersUsed
                };
                GameState.IsGameOver = true;
                OpenDialog();
            }
            else
            {
                OnWrongSolve?.Invoke();
            }
        }

        public void ToggleSolveMode()
        {
            if (GameState.IsSolveMode)
            {
                // go back to regular guess mode
                var lettersInSolveMode = GameState.PhraseLetterStateBoxes.Where(x => x.PhraseLetterState == PhraseLetterState.Solve).ToList();
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
                var lettersInNotGuessedMode = GameState.PhraseLetterStateBoxes.Where(x => x.PhraseLetterState == PhraseLetterState.NotGuessed).ToList();
                if (lettersInNotGuessedMode.Any())
                    lettersInNotGuessedMode[0].IsFocus = true;
                foreach (var letter in lettersInNotGuessedMode)
                    letter.PhraseLetterState = PhraseLetterState.Solve;
            }
            GameState.IsSolveMode = !GameState.IsSolveMode;
            OnSolveModeChange?.Invoke();
            OnKeyPress?.Invoke();
        }

        public void SolveBackspace()
        {
            // if you can backspace
            var lastLetterInGuessWithSolveLetter = GameState.PhraseLetterStateBoxes.FindLast(x => !string.IsNullOrEmpty(x.SolveLetter));
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
            foreach (var item in GameState.PhraseLetterStateBoxes)
                item.IsFocus = false;
        }

        public void OpenDialog()
        {
	        OnDialogOpen?.Invoke(true);
        }
    }
}
