using Compute.Rpc;
using Grpc.Net.Client;

var address = args.FirstOrDefault() ?? "https://localhost:7180";

using var channel = GrpcChannel.ForAddress(address);
var client = new ComputeService.ComputeServiceClient(channel);

int N = 20;
var rnd = new Random();

Console.WriteLine($"Calling {N} parallel RPCs to {address} ...");

var tasks = Enumerable.Range(0, N).Select(async i =>
{
    var x = Math.Round(rnd.NextDouble() * 10 - 5, 4);
    var delayMs = 200;

    var reply = await client.ComputeAsync(new ComputeRequest { X = x, DelayMs = delayMs });
    Console.WriteLine($"#{i:00} -> id={reply.RequestId}, x={reply.X}, result={reply.Result}, thread={reply.ThreadId}, elapsed={reply.ElapsedMs}ms");
}).ToArray();

await Task.WhenAll(tasks);
Console.WriteLine("DONE");