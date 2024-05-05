using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Lxy.HttpUtils.App;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.IncludeFields = true;
        });
        builder.WebHost.UseKestrelHttpsConfiguration();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllers();

        var app = builder.Build();
        app.MapControllers();
        // Test api
        var requestIds = new HashSet<string>();
        var testApi = app.MapGroup("/test");
        testApi.Map("/check", () => { });

        testApi.MapGet("/token", async context =>
        {
            await context.Response.WriteAsync(context.Request.Headers.TryGetValue("Token", out var token) ? token.ToString() : string.Empty);
        });

        testApi.Map("/method", async context => await context.Response.WriteAsync(context.Request.Method));

        testApi.Map("/data", (TestParams request, int P3, [FromServices] IHttpContextAccessor httpContextAccessor) =>
        {
            return $"{httpContextAccessor.HttpContext!.Request.Method}_{request.P1 + request.P2 + P3}";
        });

        testApi.MapPost("/form-data", async ([FromQuery] int P3, [FromServices] IHttpContextAccessor httpContextAccessor) =>
        {
            var form = await httpContextAccessor.HttpContext!.Request.ReadFormAsync();

            return $"{httpContextAccessor.HttpContext!.Request.Method}_{(int.TryParse(form[nameof(TestParams.P1)], out var val) ? val : 0) + (int.TryParse(form[nameof(TestParams.P2)], out val) ? val : 0) + P3}";
        });

        testApi.MapGet("/get-query", (string id, string name, string? reqId) =>
        {
            if (!string.IsNullOrWhiteSpace(reqId) && requestIds.Add(reqId))
            {
                return Results.BadRequest();
            }

            return Results.Ok(new[] { id, name });
        });

        testApi.Map("/async-stream", GetAsyncStream);
        testApi.Map("/async-stream-object", GetObjectAsyncStream);
        testApi.Map("/async-stream-line", async (HttpResponse response, int s, int e) =>
        {
            response.ContentType = "text/event-stream";
            await response.StartAsync();

            for (int i = s; i <= e; i++)
            {
                await response.WriteAsync($"{JsonSerializer.Serialize(new { S = s, E = e, C = i })}{Environment.NewLine}");

                await Task.Delay(100);
            }

            await response.CompleteAsync();
        });

        app.UseDeveloperExceptionPage();
        app.UseExceptionHandler(configure => configure.Run(async context =>
        {
            await Results.Problem().ExecuteAsync(context);
        }));

        if (args.Any(c => c.StartsWith("--start=Start")))
        {
            app.Start();
        }
        else
        {
            app.Run();
        }
    }

    private static async IAsyncEnumerable<int> GetAsyncStream(int s, int e)
    {
        for (int i = s; i <= e; i++)
        {
            await Task.Delay(100);

            yield return i;
        }
    }

    private static async IAsyncEnumerable<object> GetObjectAsyncStream(int s, int e)
    {
        for (int i = s; i <= e; i++)
        {
            await Task.Delay(100);

            yield return new { Value = i };
        }
    }
}

public class TestParams
{
    public int P1 { get; set; }
    public int P2 { get; set; }
}

public class AsyncStreamValue
{
    public int Value { get; set; }
}