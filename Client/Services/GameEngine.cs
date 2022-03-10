namespace Phrazy.Client.Services
{
    public interface IGameEngine
    {
        string Phrase { get; set; }
    }

    public class GameEngine : IGameEngine
    {
        public string Phrase { get; set; }
    }
}
