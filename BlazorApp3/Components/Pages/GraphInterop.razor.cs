using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorApp3.Components.Pages;

public sealed partial class GraphInterop : ComponentBase
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

            using var cts = new CancellationTokenSource();

            var large = JS.InvokeVoidAsync(
                "loadDataFrom",
                cts.Token,
                chart,
                StaticTestData.RawData,
                layout);

            cts.Cancel();
            
            //await Task.Delay(1000);

            var small = JS.InvokeVoidAsync(
                "loadDataFrom",
                chart,
                StaticTestData.RawDataSmall,
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
}
