using System.Net.Http.Json;

var baseUrl = args.FirstOrDefault() ?? "http://localhost:5000";
var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

int N = 20;
var rnd = new Random();

Console.WriteLine($"Sending {N} parallel requests to {baseUrl}/compute ...");

var tasks = Enumerable.Range(0, N).Select(async i =>
{
    var x = Math.Round(rnd.NextDouble() * 10 - 5, 4);
    var delayMs = 200;

    var payload = new { X = x, DelayMs = delayMs };
    using var resp = await http.PostAsJsonAsync("/compute", payload);
    resp.EnsureSuccessStatusCode();
    var json = await resp.Content.ReadAsStringAsync();

    Console.WriteLine($"#{i:00} -> {json}");
}).ToArray();

await Task.WhenAll(tasks);
Console.WriteLine("DONE");