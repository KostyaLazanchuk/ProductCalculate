using Compute.Rpc;
using Grpc.Core;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<ComputeServiceImpl>();

app.MapGet("/", () => "gRPC server is running. Use a gRPC client.");

app.Run();

public class ComputeServiceImpl : ComputeService.ComputeServiceBase
{
    public override async Task<ComputeResponse> Compute(ComputeRequest request, ServerCallContext context)
    {
        var sw = Stopwatch.StartNew();
        var id = Guid.NewGuid().ToString();

        if (request.DelayMs > 0)
            await Task.Delay(request.DelayMs);

        double result = Math.Sin(request.X) + request.X * request.X;

        sw.Stop();
        return new ComputeResponse
        {
            RequestId = id,
            X = request.X,
            Result = result,
            ThreadId = Environment.CurrentManagedThreadId,
            ElapsedMs = sw.ElapsedMilliseconds,
            AtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }
}