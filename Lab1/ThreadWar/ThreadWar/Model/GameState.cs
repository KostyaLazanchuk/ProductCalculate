namespace ThreadWar.Model
{
    public sealed class GameState
    {
        public int Width;
        public int Height;
        public int CannonX;

        public readonly List<Enemy> Enemies = new();
        public readonly List<Bullet> Bullets = new();

        public int Hits;
        public int Misses;

        public double SpawnProbability = 0.25;
        public int EnemySpeedMs = 160;

        public readonly object StateLock = new();
        public readonly object DrawLock = new();
        public readonly SemaphoreSlim BulletsLimiter = new(initialCount: 3, maxCount: 3);
        public readonly ManualResetEventSlim StartEvent = new(false);
        public readonly CancellationTokenSource Cts = new();
        public volatile bool IsGameOver;

        public int GameOverOnce = 0;
    }
}
