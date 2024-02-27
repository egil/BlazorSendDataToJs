using Microsoft.JSInterop;

namespace BlazorApp3;

public static class JsInteropCancellableExtensions
{
    public static async ValueTask InvokeStreamVoidAsync(this IJSRuntime js, string identifier, CancellationToken cancellationToken, Stream stream, params object?[]? args)
    {
        using var wrappedStream = new BlazorCancellableStreamWrapper(stream, cancellationToken);
        using var streamRef = new DotNetStreamReference(wrappedStream);
        var streamIdentifier = cancellationToken.GetHashCode() + identifier;

        cancellationToken.Register(static (state) =>
        {
            var (js, identifier) = ((IJSRuntime, string))state!;
            js.InvokeVoidAsync("BlazorStreamingJsInterop.cancelInvokeAsync", identifier);
        }, state: (js, streamIdentifier));

        args = [streamIdentifier, streamRef, identifier, .. args];

        await js.InvokeVoidAsync(
            "BlazorStreamingJsInterop.invokeAsyncWithCancellation",
            cancellationToken: cancellationToken,
            args: args);
    }

    private sealed class BlazorCancellableStreamWrapper : Stream
    {
        private readonly Stream innerStream;
        private readonly CancellationToken streamCancellationToken;

        public BlazorCancellableStreamWrapper(Stream innerStream, CancellationToken streamCancellationToken)
        {
            this.innerStream = innerStream;
            this.streamCancellationToken = streamCancellationToken;
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // Because SendDotNetStreamToJS (https://source.dot.net/#Microsoft.AspNetCore.Components.Server/ComponentHub.cs,287)
            // will throw if the stream gets disposed before it is finished sending the full content,
            // this effectively stops it from continuing after the cancellation token has been set. We cheat
            // it to think the stream is done and DotNetStreamReference will then dispose it.
            if (streamCancellationToken.IsCancellationRequested)
            {
                return ValueTask.FromResult(0);
            }

            return innerStream.ReadAsync(buffer, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                innerStream.Dispose();
            }

            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            innerStream.DisposeAsync();
            return base.DisposeAsync();
        }

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
