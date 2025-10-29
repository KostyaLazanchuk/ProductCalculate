namespace Lab3.Services
{
    public class PrimeCounter
    {
        public long CountPrimes(int n, CancellationToken ct)
        {
            long count = 0;

            Parallel.For(2, n + 1, new ParallelOptions { CancellationToken = ct }, i =>
            {
                if (IsPrime(i)) Interlocked.Increment(ref count);
            });

            return count;
        }

        private static bool IsPrime(int x)
        {
            if (x < 2) return false;
            if (x % 2 == 0) return x == 2;
            int lim = (int)Math.Sqrt(x);
            for (int d = 3; d <= lim; d += 2)
                if (x % d == 0) return false;
            return true;
        }
    }
}
