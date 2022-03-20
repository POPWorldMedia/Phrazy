﻿using Phrazy.Client.Models;

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
            Phrase = "too cool for elementary school";
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
                    var stateBox = new PhraseLetterStateBox
                        {Letter = character.ToString(), PhraseLetterState = PhraseLetterState.NotGuessed};
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
                KeyStates[letter] = KeyState.Chosen;

            foreach (var stateBox in PhraseLetterStateBoxes!)
            {
                if (stateBox.Letter == letter)
                {
                    stateBox.PhraseLetterState = PhraseLetterState.Guessed;
                }
            }

            OnKeyPress?.Invoke();
        }
    }
}
