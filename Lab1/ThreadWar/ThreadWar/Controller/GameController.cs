using ThreadWar.Core;
using ThreadWar.Model;
using ThreadWar.View;

namespace ThreadWar.Controller
{
    public sealed class GameController
    {
        private readonly GameState _gameState;
        private readonly GameEngine _gameEngine;
        private readonly ConsoleView _consoleView;

        private const int FrameMs = 33;

        public GameController(GameState gameState, GameEngine gameEngine, ConsoleView consoleView)
        {
            _gameState = gameState;
            _gameEngine = gameEngine;
            _consoleView = consoleView;
        }

        public void Run()
        {
            var starter = new Thread(() =>
            {
                if (!_gameState.StartEvent.Wait(TimeSpan.FromSeconds(15))) _gameState.StartEvent.Set();
            })
            {
                IsBackground = true
            };

            starter.Start();

            while (!_gameState.IsGameOver && !_gameState.Cts.IsCancellationRequested)
            {
                HandleInput();
                _consoleView.DrawFrame();
                if (_gameState.Misses >= 30) _gameEngine.TriggerGameOver();
                Thread.Sleep(FrameMs);
            }
        }

        private void HandleInput()
        {
            if (!Console.KeyAvailable) return;
            var key = Console.ReadKey(true).Key;
            if (!_gameState.StartEvent.IsSet) _gameState.StartEvent.Set();


            lock (_gameState.StateLock)
            {
                switch (key)
                {
                    case ConsoleKey.LeftArrow:
                        _gameState.CannonX = Math.Max(0, _gameState.CannonX - 1); break;
                    case ConsoleKey.RightArrow:
                        _gameState.CannonX = Math.Min(_gameState.Width - 1, _gameState.CannonX + 1); break;
                    case ConsoleKey.Spacebar:
                        _gameEngine.TryFire(); break;
                }
            }
        }
    }
}
