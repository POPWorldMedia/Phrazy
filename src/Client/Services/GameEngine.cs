using System.Diagnostics;
using System.Text;
using System.Timers;
using Phrazy.Client.Models;
using Phrazy.Shared.Models;
using Timer = System.Timers.Timer;

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
        event Action? OnTimeUpdated;
    }

    public class GameEngine : IGameEngine
    {
	    private readonly IPuzzleService _puzzleService;
	    private readonly IDeviceIDService _deviceIDService;

	    public event Action? OnKeyPress;
        public event Action? OnSolveModeChange;
        public event Action? OnWrongSolve;
        public event Action<bool>? OnDialogOpen;
        public event Action? OnBoardLoad;
        public event Action? OnTimeUpdated;

        public GameState GameState { get; private set; }

        private readonly Timer _timer;
        private readonly Stopwatch _stopwatch;
        private string? _deviceID;
        private PuzzleDefinition? _puzzleDefinition;
        private const int TotalGameSeconds = 120;  // total starting seconds available
        private const int GuessTimePenalty = 5;    // seconds off per guess if using that rule
        private const int UnusedLetterBonus = 10;  // points for every unused letter if using that rule

        public GameEngine(IPuzzleService puzzleService, IDeviceIDService deviceIDService)
        {
	        _puzzleService = puzzleService;
	        _deviceIDService = deviceIDService;
	        GameState = new GameState();
	        _timer = new Timer(100);
            _timer.Enabled = true;
            _timer.AutoReset = true;
            _timer.Elapsed += UpdateClock;
	        _stopwatch = new Stopwatch();
        }

        public async Task<List<List<PhraseLetterStateBox>>> Start()
        {
	        _deviceID = await _deviceIDService.GetDeviceID();

	        _puzzleDefinition = await _puzzleService.GetCurrentPuzzle();

            GameState.Phrase = _puzzleDefinition.Puzzle;

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

        private void UpdateClock(object? sender, ElapsedEventArgs e)
        {
	        var secondsElapsed = _stopwatch.Elapsed.TotalSeconds;
	        var remainingTime = secondsElapsed - TotalGameSeconds; // + GameState.SecondPenalty;
	        GameState.SecondsRemaining = (int)secondsElapsed;
	        //GameState.SecondsRemaining = -(int)remainingTime;
	        //if (GameState.SecondsRemaining <= 0)
	        //{
		       // GameState.SecondsRemaining = 0;
         //       End(false);
	        //}
	        OnTimeUpdated?.Invoke();
        }

        private void End(bool isWinner)
        {
	        if (GameState.SecondsRemaining < 0)
		        GameState.SecondsRemaining = 0;
	        GameState.IsGameOver = true;
	        var lettersUsed = GameState.GuessRecords.Count;
	        //var score = GameState.SecondsRemaining + ((26 - lettersUsed) * UnusedLetterBonus);
	        var score = lettersUsed;

            GameState.Results = new Results
	        {
		        IsWin = isWinner,
		        LettersUsed = lettersUsed,
                Score = isWinner?  score : 0,
                SecondsLeft = GameState.SecondsRemaining
	        };
	        GameState.IsGameOver = true;
	        OpenDialog();
	        _timer.Stop();
	        _stopwatch.Stop();
	        OnKeyPress?.Invoke();
	        _puzzleService.SendResults(_deviceID!, _puzzleDefinition!.Hash, _puzzleDefinition.PuzzleID, GameState.Results);
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

            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
                _timer.Start();
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

                GameState.SecondPenalty += GuessTimePenalty;

                GameState.GuessRecords.Add(new GuessRecord {IsCorrect = hit, Letter = letter});

                GameState.KeyStates[letter] = hit ? GameState.KeyStates[letter] = KeyState.Hit : GameState.KeyStates[letter] = KeyState.Miss;

                // all the letters are picked
                var lettersRemaining = GameState.KeyStates.Where(x => x.Value == KeyState.NotChosen);
                if (!lettersRemaining.Any())
                {
	                UpdateClock(null, null!);
	                End(false);
	                return;
                }

                // all possible hits are made
                var notGuessed = GameState.PhraseLetterStateBoxes.Where(x => x.PhraseLetterState == PhraseLetterState.NotGuessed).ToList();
                if (!notGuessed.Any())
                {
	                End(false);
	                return;
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
                End(true);
                return;
            }
            
            OnWrongSolve?.Invoke();
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
