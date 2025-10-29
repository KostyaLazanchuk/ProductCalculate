using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Lab3.Services
{
    public class ComputeWorker(ComputeQueue queue, PrimeCounter counter) : BackgroundService
    {
        private readonly SemaphoreSlim _concurrency = new(4);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var job in queue.DequeueAsync(stoppingToken))
            {
                _ = ProcessJobAsync(job, stoppingToken);
            }
        }

        private async Task ProcessJobAsync(ComputeRequest job, CancellationToken ct)
        {
            await _concurrency.WaitAsync(ct);
            var sw = Stopwatch.StartNew();

            try
            {
                queue.Results[job.JobId] = ("running", 0, null);
                var result = counter.CountPrimes(job.N, ct);
                sw.Stop();
                queue.Results[job.JobId] = ("completed", result, sw.Elapsed);
            }
            catch (OperationCanceledException)
            {
                queue.Results[job.JobId] = ("canceled", 0, null);
            }
            catch (Exception)
            {
                queue.Results[job.JobId] = ("failed", 0, null);
            }
            finally
            {
                _concurrency.Release();
            }
        }
    }
}
