window.loadDataFrom = (chart, data, layout) => {
    data.x = data.x.map(dt => new Date(dt));
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
            const message = TimeSeriesData.decode(protobufArray);
            const data = {
                x: message.x.map(ts => new Date(ts * 1000)),
                y: message.y,
                type: 'scatter'
            };

            Plotly.newPlot(chart, [data], layout);
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