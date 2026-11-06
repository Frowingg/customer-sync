using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CustomerSync.Api.Models;
using CustomerSync.Core.Models;
using Xunit;

namespace CustomerSync.Api.Tests;

public class SyncApiTests : IDisposable
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;

    public SyncApiTests() => _client = _factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static SourceCustomer Valid(int id) =>
        new(id, $"Customer {id}", $"c{id}@example.test", $"DEMO{id:D4}A01H501Z", "Roma");

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostRun_InsertsValidRecordsAndReportsInvalidOnes()
    {
        _factory.Source.Items.AddRange(new[] { Valid(1), Valid(2), Valid(3) with { Email = "bad@@mail" } });

        var response = await _client.PostAsync("/api/sync/runs", null);
        var summary = await response.Content.ReadFromJsonAsync<SyncRunSummary>(Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.NotNull(summary);
        Assert.Equal(2, summary.Inserted);
        Assert.Equal(1, summary.DiscrepancyCount);
    }

    [Fact]
    public async Task SecondRunWithSameData_ChangesNothing()
    {
        _factory.Source.Items.AddRange(new[] { Valid(1), Valid(2) });
        await _client.PostAsync("/api/sync/runs", null);

        var response = await _client.PostAsync("/api/sync/runs", null);
        var summary = await response.Content.ReadFromJsonAsync<SyncRunSummary>(Json);

        Assert.NotNull(summary);
        Assert.Equal(0, summary.Inserted);
        Assert.Equal(0, summary.Updated);
        Assert.Equal(2, summary.Unchanged);
    }

    [Fact]
    public async Task GetRun_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/sync/runs/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListRuns_ReturnsNewestFirst()
    {
        _factory.Source.Items.Add(Valid(1));
        await _client.PostAsync("/api/sync/runs", null);
        await _client.PostAsync("/api/sync/runs", null);

        var runs = await _client.GetFromJsonAsync<List<SyncRunSummary>>("/api/sync/runs", Json);

        Assert.NotNull(runs);
        Assert.Equal(2, runs.Count);
        Assert.True(runs[0].StartedUtc >= runs[1].StartedUtc);
    }

    [Fact]
    public async Task Discrepancies_CanBeFilteredByType()
    {
        _factory.Source.Items.AddRange(new[]
        {
            Valid(1),
            Valid(2) with { Name = "" },   // invalid
            Valid(1)                       // duplicate
        });
        var post = await _client.PostAsync("/api/sync/runs", null);
        var run = await post.Content.ReadFromJsonAsync<SyncRunSummary>(Json);
        Assert.NotNull(run);

        var all = await _client.GetFromJsonAsync<List<DiscrepancyDto>>(
            $"/api/sync/runs/{run.Id}/discrepancies", Json);
        var invalidOnly = await _client.GetFromJsonAsync<List<DiscrepancyDto>>(
            $"/api/sync/runs/{run.Id}/discrepancies?type=InvalidRecord", Json);

        Assert.NotNull(all);
        Assert.NotNull(invalidOnly);
        Assert.Equal(2, all.Count);
        var only = Assert.Single(invalidOnly);
        Assert.Equal(DiscrepancyType.InvalidRecord, only.Type);
        Assert.Equal(2, only.LegacyId);
    }
}