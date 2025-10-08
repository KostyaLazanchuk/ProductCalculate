using ThreadWar.Model;

namespace ThreadWar.Core
{
    public sealed class GameEngine
    {
        private readonly GameState _gameState;
        public GameEngine(GameState gameState)
        {
            _gameState = gameState;
        }

        public void StartBackgroundSystems()
        {
            var spawner = new Thread(SpawnerLoop)
            {
                IsBackground = true
            };
            spawner.Start();

            var difficulty = new Thread(DifficultyLoop)
            {
                IsBackground = true
            };
            difficulty.Start();
        }

        public void TryFire()
        {
            if (!_gameState.BulletsLimiter.Wait(0)) return;
            var bullet = new Bullet(_gameState.CannonX, _gameState.Height - 2, 30);
            lock (_gameState.StateLock) _gameState.Bullets.Add(bullet);
            var tread = new Thread(() => BulletLoop(bullet))
            {
                IsBackground = true
            };
            tread.Start();
        }

        public void TriggerGameOver()
        {
            if (Interlocked.CompareExchange(ref _gameState.GameOverOnce, 1, 0) == 0)
            {
                _gameState.IsGameOver = true;
                _gameState.Cts.Cancel();
            }
        }

        private void SpawnerLoop()
        {
            _gameState.StartEvent.Wait();
            var rand = new Random();
            while (!_gameState.IsGameOver && !_gameState.Cts.IsCancellationRequested)
            {
                double p;
                int speed;

                lock (_gameState.StateLock)
                {
                    p = _gameState.SpawnProbability;
                    speed = _gameState.EnemySpeedMs;
                }

                if (rand.NextDouble() < p)
                {
                    int x;

                    lock (_gameState.StateLock)
                    {
                        x = rand.Next(0, _gameState.Width);
                        var e = new Enemy(x, 1, speed);
                        lock (_gameState.StateLock)
                        {
                            _gameState.Enemies.Add(e);
                        }

                        var t = new Thread(() => EnemyLoop(e))
                        {
                            IsBackground = true,
                        };
                        t.Start();
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        private void DifficultyLoop()
        {
            while (!_gameState.IsGameOver && !_gameState.Cts.IsCancellationRequested)
            {
                Thread.Sleep(1000);
                lock (_gameState.StateLock)
                {
                    _gameState.SpawnProbability = Math.Min(0.90, _gameState.SpawnProbability + 0.05);
                    _gameState.EnemySpeedMs = Math.Max(60, _gameState.EnemySpeedMs - 10);
                }
            }
        }

        private void EnemyLoop(Enemy enemy)
        {
            while (!_gameState.IsGameOver && !_gameState.Cts.IsCancellationRequested && enemy.IsAlive)
            {
                bool removed = false;
                lock (_gameState.StateLock)
                {
                    if (!enemy.IsAlive) break;
                    enemy.Y++;
                    var hitBullet = _gameState.Bullets.Find(b => b.IsAlvie && b.X == enemy.X && b.Y == enemy.Y);
                    if (hitBullet is not null)
                    {
                        enemy.IsAlive = false;
                        hitBullet.IsAlvie = false;
                        _gameState.Hits++;
                        removed = true;
                    }
                    else if (enemy.Y >= _gameState.Height -1)
                    {
                        enemy.IsAlive = false;
                        _gameState.Misses++;
                        removed = true;
                    }
                }
                if (removed)
                {
                    break;
                }
                Thread.Sleep(enemy.SpeedMs);
            }
        }

        private void BulletLoop(Bullet bullet)
        {
            try
            {
                while (!_gameState.IsGameOver && !_gameState.Cts.IsCancellationRequested && bullet.IsAlvie)
                {
                    bool removed = false;
                    lock (_gameState.StateLock)
                    {
                        if (!bullet.IsAlvie) break;
                        bullet.Y--;
                        var hitEnemy = _gameState.Enemies.Find(e => e.IsAlive && e.X == bullet.X && e.Y == bullet.Y);
                        if (hitEnemy is not null)
                        {
                            hitEnemy.IsAlive = false;
                            bullet.IsAlvie = false;
                            _gameState.Hits++;
                            removed = true;
                        }
                        else if (bullet.Y <= 0)
                        {
                            bullet.IsAlvie = false;
                            removed = true;
                        }
                    }
                    if (removed) 
                    { 
                        break;
                    }
                    Thread.Sleep(bullet.SpeedMs);
                }
            }

            finally
            {
                _gameState.BulletsLimiter.Release();
            }
        }
    }
}
