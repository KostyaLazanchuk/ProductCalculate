namespace ThreadWar.Core
{
    public interface ITimer
    {
        void Sleep(int ms);
        long NowMs { get; }
    }
}
