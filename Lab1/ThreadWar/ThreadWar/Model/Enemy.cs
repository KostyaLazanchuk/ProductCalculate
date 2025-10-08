namespace ThreadWar.Model
{
    public sealed class Enemy
    {
        public int X;
        public int Y;
        public int SpeedMs;
        public bool IsAlive;
        public Enemy(int x, int y, int speedMs)
        {
            X = x;
            Y = y;
            SpeedMs = speedMs;
            IsAlive = true;
        }
    }
}
