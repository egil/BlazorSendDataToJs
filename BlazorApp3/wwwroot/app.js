window.loadDataFrom = (chart, data, layout) => {
    data.x = data.x.map(dt => new Date(dt));
    console.log("Received " + data.x.length);
    Plotly.newPlot(chart, [data], layout);
}


window.loadDataFromApi = (chart, layout) => {
    return fetch('/api/data')
        .then(response => response.json())
        .then(data => {
            data.x = data.x.map(dt => new Date(dt));
            Plotly.newPlot(chart, [data], layout);
            return true;
        });
}

// Define your .proto schema as a string
const protoStr = `
    syntax = "proto3";
    message TimeSeriesData {
        repeated int64 x = 1;
        repeated double y = 2;
    }`;

// Parse the .proto string to get the root object
const root = protobuf.parse(protoStr).root;

// Obtain a message type
const TimeSeriesData = root.lookupType("TimeSeriesData");

window.loadDataFromProtobuf = (chart, protobufStream, layout) => {
    return protobufStream
        .arrayBuffer()
        .then(rawArrayBuffer => {
            const protobufArray = new Uint8Array(rawArrayBuffer);
            loadDataFromProtobufArray(chart, protobufArray, layout);
        });
}

window.loadDataFromProtobufArray = (chart, protobufArray, layout) => {
    const message = TimeSeriesData.decode(protobufArray);
    const data = {
        x: message.x.map(ts => new Date(ts * 1000)),
        y: message.y,
        type: 'scatter'
    };

    Plotly.newPlot(chart, [data], layout);
}

window.decompress = async function (compressedBytes) {
    const stream = new Blob([compressedBytes]).stream();
    const decompressedStream = stream.pipeThrough(
        new DecompressionStream("gzip")
    );
    const text = await new Response(decompressedStream).text();
    return text;
}
window.loadCompressedData = function (chart, data, layout) {
    decompress(data).then(rawdata => {
        var data = JSON.parse(rawdata);
        data.x = data.x.map(dt => new Date(dt));
        Plotly.newPlot(chart, [data], layout);
    });
}

window.invokeMe = function (name, randomPayload, additional) {
    console.log(`${name} | ${randomPayload.x.length} | ${new Date().toISOString()} | ${additional}`);
}

window.invokeMeX = function (randomPayload, additional) {
    console.log(`${randomPayload[0].x.length} | ${new Date().toISOString()} | ${additional}`);
}

const aborters = {

};

window.invokeMeAsync = async function (name, randomPayloadStream) {
    abortInvokeMeAsync();

    try {
        const controller = new AbortController();
        aborters[name] = controller;
        const stream = await randomPayloadStream.stream();
        const randomPayload = await processJsonStream(controller.signal, stream);
        console.log(`${name} | ${randomPayload.x.length} | ${new Date().toISOString()}`);
    }
    catch (err) {
        console.warn(err);
    }
}

window.abortInvokeMeAsync = function (name) {
    if (aborters[name]) {
        aborters[name].abort('Cancelled from server'); // Abort previous stream if exists
        delete aborters[name];
    }
}

async function processJsonStream(abortSignal, stream) {
    if (abortSignal.aborted) {
        throw new Error(abortSignal.aborted.reason);
    }

    const decompressedStream = stream.pipeThrough(new DecompressionStream('gzip'), { signal: abortSignal });
    return new Response(decompressedStream).json();
}

class BlazorStreamingJsInterop {
    static abortControllers = new Map();

    static async invokeAsyncWithCancellation(cancellationTokenId, dotNetStreamRef, identifier, args) {
        const abortController = new AbortController();
        this.abortControllers[cancellationTokenId] = abortController;

        let stream = await dotNetStreamRef.stream();
        stream = stream.pipeThrough(new TransformStream({
            transform(chunk, controller) {
                if (abortController.signal.aborted) {
                    controller.terminate();
                    return;
                }
                controller.enqueue(chunk);
            }
        }));

        try {
            const json = await new Response(stream).json();

            if (!abortController.signal.aborted) {
                delete this.abortControllers[cancellationTokenId];
                const jsFunction = DotNet.findJSFunction(identifier, 0);
                let argsArray = [];
                if (Array.isArray(args)) {
                    argsArray = args;
                } else if (args !== undefined) {
                    argsArray = [args];
                }
                argsArray = [json, ...argsArray];
                jsFunction.apply(null, argsArray);
            }
        }
        catch (err) {
            if (!abortController.signal.aborted) {
                throw err;
            }
        }
    }

    static cancelInvokeAsync(cancellationTokenId) {
        if (this.abortControllers[cancellationTokenId]) {
            console.log(`Cancelling invokeAsyncWithCancellation with id ${cancellationTokenId}`);
            this.abortControllers[cancellationTokenId].abort('Blazor JS streaming call cancelled by server.');
            delete this.abortControllers[cancellationTokenId];
        }
    }
}

window.BlazorStreamingJsInterop = BlazorStreamingJsInterop;
