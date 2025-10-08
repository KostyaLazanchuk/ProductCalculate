using ThreadWar.Model;

namespace ThreadWar.View
{
    public sealed class ConsoleView
    {
        private readonly GameState _gameState;
        private long _lastTitle;

        public ConsoleView(GameState gameState)
        {
            _gameState = gameState;
        }

        public void ShowSplash()
        {
            Console.Title = "ThreadWar";
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Controll left and right");
            Console.WriteLine("Enemy will start after first move");
        }

        public void DrawFrame()
        {
            lock (_gameState.DrawLock)
            {
                for (int y = 0; y < _gameState.Height; y++)
                {
                    Console.SetCursorPosition(0, y);
                    Console.Write(new string(' ', Math.Max(0, _gameState.Width - 1)));
                }

                lock (_gameState.StateLock)
                {
                    foreach (var e in _gameState.Enemies)
                    {
                        if (!e.IsAlive) continue;
                        if (e.X >= 0 && e.X < _gameState.Width && e.Y >= 0 && e.Y < _gameState.Height)
                        {
                            Console.SetCursorPosition(e.X, e.Y);
                            Console.Write('V');
                        }
                    }

                    foreach (var b in _gameState.Bullets)
                    {
                        if (!b.IsAlvie) continue;
                        if (b.X >= 0 && b.X < _gameState.Width && b.Y >= 0 && b.Y < _gameState.Height)
                        {
                            Console.SetCursorPosition(b.X, b.Y);
                            Console.Write('|');
                        }
                    }

                    Console.SetCursorPosition(Clamp(_gameState.CannonX, 0, _gameState.Width - 1), _gameState.Height - 1);
                    Console.WriteLine('A');
                }

                var now = Environment.TickCount64;
                if (now - _lastTitle > 100)
                {
                    Console.Title = $"ThreadWar MVC — Hits: {_gameState.Hits} | Misses: {_gameState.Misses} (max 30) | Bullets: {AliveBullets()}/3";
                }
            }
        }

        public void ShowGameOver()
        {
            lock (_gameState.DrawLock)
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("====================================");
                Console.WriteLine("============= GAME OVER ============");
                Console.WriteLine("====================================");
                Console.WriteLine($"Score: {_gameState.Hits}");
                Console.WriteLine($"Misses: {_gameState.Misses} (max 30)");
                Console.WriteLine();
                Console.WriteLine("Press any button to leave...");
            }

            Console.ReadKey(true);
        }

        private int AliveBullets()
        {
            lock (_gameState.StateLock)
            {
                int c = 0;
                foreach (var b in _gameState.Bullets) if (b.IsAlvie) c++; return c;
            }
        }

        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);
    }
}
