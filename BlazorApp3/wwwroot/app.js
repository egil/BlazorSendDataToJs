window.loadDataFromApi = (chart, layout) => {
    return fetch('/api/data')
        .then(response => response.json())
        .then(data => {
            Plotly.newPlot(chart, [data], layout);
            return true;
        });
}

window.loadDataFromProtobuf = async (chart, grpcDataStream, layout) => {
    var buf = new Uint8Array(await grpcDataStream.arrayBuffer());

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

    // Decode the Uint8Array
    const message = TimeSeriesData.decode(buf);
    const data = {
        x: message.x.map(ts => new Date(ts * 1000)),
        y: message.y,
        type: 'scatter'
    };

    Plotly.newPlot(chart, [data], layout);
}