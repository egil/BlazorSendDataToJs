using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorApp3.Components.Pages;

public sealed partial class GraphInteropCompressed : ComponentBase
{
    private ElementReference chart;
    private TimeSpan elapsed;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            var layout = new
            {
                title = "Sales Growth",
                xaxis = new
                {
                    title = "Year",
                    showgrid = false,
                    zeroline = false,
                },
                yaxis = new
                {
                    title = "Percent",
                    showline = false,
                },
            };

            var json = JsonSerializer.Serialize(StaticTestData.RawData, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var stream = CompressStringToByteArray(json);

            using var streamRef = new DotNetStreamReference(stream: stream);

            await JS.InvokeVoidAsync(
                    "loadCompressedData",
                    chart,
                    streamRef,
                    layout);

            stopwatch.Stop();
            elapsed = stopwatch.Elapsed;
            StateHasChanged();
        }
    }

    [JSInvokable]
    public void OnStepClick(string timestamp, double value)
    {
    }

    private Stream CompressStringToByteArray(string input)
    {
        // Convert the input string to bytes
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);

        // Create an empty MemoryStream
        var memStream = new MemoryStream();

        // Initialize a GZipStream in CompressionMode.Compress
        using (GZipStream zipStream = new GZipStream(memStream, CompressionMode.Compress, leaveOpen: true))
        {
            // Write the input bytes into the GZipStream
            zipStream.Write(inputBytes, 0, inputBytes.Length);
        }

        return memStream;
    }

}
