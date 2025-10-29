using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Lab3.Services
{
    public record ComputeRequest(Guid JobId, int N, string User);
    public class ComputeQueue
    {
        private readonly Channel<ComputeRequest> _channel = Channel.CreateUnbounded<ComputeRequest>();

        // jobId -> (Status, Result, Elapsed)
        public ConcurrentDictionary<Guid, (string Status, long Result, TimeSpan? Elapsed)> Results { get; } = new();

        public async Task EnqueueAsync(ComputeRequest req, CancellationToken ct = default)
        {
            Results[req.JobId] = ("queued", 0, null);
            await _channel.Writer.WriteAsync(req, ct);
        }

        public IAsyncEnumerable<ComputeRequest> DequeueAsync(CancellationToken ct) =>
            _channel.Reader.ReadAllAsync(ct);
    }
}
