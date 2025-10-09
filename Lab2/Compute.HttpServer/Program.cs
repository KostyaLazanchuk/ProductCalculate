using System.Diagnostics;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.MapPost("/compute", async (ComputeRequest req, ILoggerFactory lf) =>
{
    var logger = lf.CreateLogger("Compute");

    var sw = Stopwatch.StartNew();
    var requestId = Guid.NewGuid();

    int threadId = Environment.CurrentManagedThreadId;

    if (req.DelayMs is > 0)
        await Task.Delay(req.DelayMs.Value);

    double result = Math.Sin(req.X) + req.X * req.X;

    sw.Stop();

    var response = new ComputeResponse(
        RequestId: requestId,
        X: req.X,
        Result: result,
        ElapsedMs: sw.ElapsedMilliseconds,
        ThreadId: threadId,
        At: DateTimeOffset.UtcNow
    );

    logger.LogInformation(
        "Handled {Id}: f({X})={Result}, thread={Thread}, elapsed={Elapsed}ms",
        requestId, req.X, result, threadId, sw.ElapsedMilliseconds
    );

    return Results.Ok(response);
})
.WithName("Compute")
.WithDescription("Calculate f(x) = sin(x) + x²")
.Produces<ComputeResponse>(StatusCodes.Status200OK);
app.Run();

record ComputeRequest(double X, int? DelayMs = null);
record ComputeResponse(Guid RequestId, double X, double Result, long ElapsedMs, int ThreadId, DateTimeOffset At);