using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var baseUrl = args.Length > 0 ? args[0] : "http://localhost:5000/";

using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

Console.WriteLine("Logging in");
var loginBody = JsonSerializer.Serialize(new { username = "user", password = "pass" });
var loginRes = await http.PostAsync("auth/login",
    new StringContent(loginBody, Encoding.UTF8, "application/json"));
if (!loginRes.IsSuccessStatusCode)
{
    Console.WriteLine($"Login failed: {loginRes.StatusCode}");
    return;
}
var token = JsonDocument.Parse(await loginRes.Content.ReadAsStringAsync())
    .RootElement.GetProperty("token").GetString();
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
Console.WriteLine("Login OK.");

var nValues = new[] { 2_000_000, 2_200_000, 2_400_000 };
var jobIds = new List<Guid>();

foreach (var n in nValues)
{
    var startBody = JsonSerializer.Serialize(new { n });
    var startRes = await http.PostAsync("compute",
        new StringContent(startBody, Encoding.UTF8, "application/json"));
    var json = await startRes.Content.ReadAsStringAsync();
    Console.WriteLine($"Enqueue n={n}: {json}");
    var jobId = JsonDocument.Parse(json).RootElement.GetProperty("jobId").GetGuid();
    jobIds.Add(jobId);
}

Console.WriteLine("Polling statuses");
while (jobIds.Any())
{
    foreach (var jobId in jobIds.ToList())
    {
        var statusRes = await http.GetAsync($"compute/{jobId}");
        if (!statusRes.IsSuccessStatusCode)
        {
            Console.WriteLine($"Job {jobId}: {statusRes.StatusCode}");
            continue;
        }

        var txt = await statusRes.Content.ReadAsStringAsync();
        Console.WriteLine(txt);

        if (txt.Contains("\"completed\"") || txt.Contains("\"failed\"") || txt.Contains("\"canceled\""))
        {
            jobIds.Remove(jobId);
        }
    }

    await Task.Delay(1000);
}

Console.WriteLine("Done.");