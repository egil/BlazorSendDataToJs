using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ProtoBuf;

namespace BlazorApp3.Components.Pages;

public sealed partial class GraphInteropGrpc : ComponentBase
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

            using var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, new TimeSeriesDataProto(StaticTestData.RawData));
            memoryStream.Position = 0;
            using var streamRef = new DotNetStreamReference(stream: memoryStream);

            await JS.InvokeVoidAsync(
                "loadDataFromGrpc",
                chart,
                streamRef,
                layout); ;

            stopwatch.Stop();
            elapsed = stopwatch.Elapsed;
            StateHasChanged();
        }
    }

    [JSInvokable]
    public void OnStepClick(string timestamp, double value)
    {
    }
}
