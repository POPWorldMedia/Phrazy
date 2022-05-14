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
        event Action? OnStartGame;
        
        GameState GameState { get; }
        LastResultPayload? LastResultPayload { get; }

        Task ChooseLetter(string letter);
        Task<List<List<PhraseLetterStateBox>>?> Start();
        void ToggleSolveMode();
        void SolveBackspace();
        void OpenDialog();
        event Action? OnTimeUpdated;
    }

    public class GameEngine : IGameEngine
    {
	    private readonly IPuzzleService _puzzleService;
	    private readonly IDeviceIDService _deviceIDService;
	    private readonly IAlertService _alertService;
	    private readonly IGameStatePersistenceService _gameStatePersistenceService;

	    public event Action? OnKeyPress;
        public event Action? OnSolveModeChange;
        public event Action? OnWrongSolve;
        public event Action<bool>? OnDialogOpen;
        public event Action? OnBoardLoad;
        public event Action? OnTimeUpdated;
        public event Action? OnStartGame;

        public GameState GameState { get; private set; }
        public LastResultPayload? LastResultPayload { get; private set; }

        private readonly Timer _timer;
        private string? _deviceID;

        public GameEngine(IPuzzleService puzzleService, IDeviceIDService deviceIDService, IAlertService alertService, IGameStatePersistenceService gameStatePersistenceService)
        {
	        _puzzleService = puzzleService;
	        _deviceIDService = deviceIDService;
	        _alertService = alertService;
	        _gameStatePersistenceService = gameStatePersistenceService;
	        GameState = new GameState();
	        LastResultPayload = null;
	        _timer = new Timer(250);
            _timer.AutoReset = true;
            _timer.Elapsed += UpdateClock;
        }

        public async Task<List<List<PhraseLetterStateBox>>?> Start()
        {
	        _deviceID = await _deviceIDService.GetDeviceID();

	        var currentPuzzle = await _puzzleService.GetCurrentPuzzle();

	        var isStateRestored = false;
	        var previousState = await _gameStatePersistenceService.Load();
	        if (previousState != null && previousState.PuzzleDefinition?.PuzzleID == currentPuzzle.PuzzleID)
	        {
                // restore state
                isStateRestored = true;
                GameState = previousState;
                GameState.Seconds = (GameState.TimeStamp - DateTime.UtcNow).Seconds;

                if (!GameState.IsGameOver)
                {
	                _timer.Start();
                }
            }
	        else
	        {
                // new state
		        GameState.PuzzleDefinition = currentPuzzle;
	        }

	        if (string.IsNullOrEmpty(GameState.PuzzleDefinition?.Phrase))
            {
                LastResultPayload = await _puzzleService.GetLastResult();
                await _alertService.PopAlert("We couldn't find a new puzzle.");
		        GameState.IsGameOver = true;
		        OnBoardLoad?.Invoke();
		        return null;
	        }

            GameState.Phrase = GameState.PuzzleDefinition.Phrase;

            // divy up the letters
            var wordsOfStateBoxes = new List<List<PhraseLetterStateBox>>();
            var words = GameState.Phrase.Split(' ');
	        var index = 0;
            foreach (var word in words)
            {
                var wordOfStateBoxes = new List<PhraseLetterStateBox>();
                var chars = word.ToCharArray();
                var alphabet = "abcdefghijklmnopqrstuvwxyz";
                var array = alphabet.ToCharArray();
                foreach (var character in chars)
                {
                    var initialState = array.Contains(character) ? PhraseLetterState.NotGuessed : PhraseLetterState.Special;
                    var stateBox = isStateRestored ?
	                    previousState!.PhraseLetterStateBoxes[index]
	                    : new PhraseLetterStateBox {Letter = character.ToString(), PhraseLetterState = initialState};
                    if (!isStateRestored)
	                    GameState.PhraseLetterStateBoxes.Add(stateBox);
                    wordOfStateBoxes.Add(stateBox);
                    index++;
                }
                wordsOfStateBoxes.Add(wordOfStateBoxes);
            }

            LastResultPayload = await _puzzleService.GetLastResult();

            if (isStateRestored)
	            OnSolveModeChange?.Invoke();
            OnBoardLoad?.Invoke();
            OnKeyPress?.Invoke();
            OnTimeUpdated?.Invoke();

            return wordsOfStateBoxes;
        }

        private void UpdateClock(object? sender, ElapsedEventArgs e)
        {
	        var elapsed = DateTime.UtcNow - GameState.TimeStamp;
	        GameState.Seconds = (int)elapsed.TotalSeconds;
            _gameStatePersistenceService.Save(GameState);
	        OnTimeUpdated?.Invoke();
        }

        private async Task End(bool isWinner)
        {
	        if (GameState.Seconds < 0)
		        GameState.Seconds = 0;
	        GameState.IsGameOver = true;
	        var lettersUsed = GameState.GuessRecords.Count;
	        var score = lettersUsed;

            GameState.Results = new Results
	        {
		        IsWin = isWinner,
		        LettersUsed = lettersUsed,
                Score = isWinner ? score : 0,
                Seconds = GameState.Seconds
	        };
	        _timer.Stop();
	        OnKeyPress?.Invoke();
	        await Task.Delay(2500);
	        OpenDialog();
	        await _gameStatePersistenceService.Save(GameState);
            _puzzleService.SendResults(_deviceID!, GameState.PuzzleDefinition!.Hash, GameState.PuzzleDefinition!.PuzzleID, GameState.Results);
        }

        public async Task ChooseLetter(string letter)
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

            if (letter == "enter")
            {
                if (!GameState.IsSolveMode)
					ToggleSolveMode();
	            return;
            }

            if (!_timer.Enabled)
            {
                // start of game
                GameState.TimeStamp = DateTime.UtcNow;
                _timer.Start();
                OnStartGame?.Invoke();
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
                            await SolveCheck();
                    }
                }
                else
                    await SolveCheck();

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
	                UpdateClock(null, null!);
	                await End(false);
	                return;
                }

                // all possible hits are made
                var notGuessed = GameState.PhraseLetterStateBoxes.Where(x => x.PhraseLetterState == PhraseLetterState.NotGuessed).ToList();
                if (!notGuessed.Any())
                {
	                await End(false);
	                return;
                }

                OnKeyPress?.Invoke();
            }
        }

        private async Task SolveCheck()
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
                await End(true);
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
