namespace ThreadWar.Model
{
    public sealed class Bullet
    {
        public int X;
        public int Y;
        public int SpeedMs;
        public bool IsAlvie;
        
        public Bullet(int x, int y, int speedMs)
        {
            X = x; 
            Y = y;
            SpeedMs = speedMs;
            IsAlvie = true;
        }
    }
}
