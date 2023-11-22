using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Somfic.Common;
using Vla.Server;
using Vla.Server.Messages;
using Whisper.net;
using Whisper.net.Ggml;

namespace Vla.Voice;

public class RecogniserService
{
    private const double AudioSampleLengthS = 1;
    private const double TotalBufferLength = 10 / AudioSampleLengthS;
    
    private readonly ILogger<RecogniserService> _log;
    private readonly HttpClient _http;
    private readonly WaveInEvent _waveIn;
    private readonly List<float[]> _slidingBuffer = new((int)(TotalBufferLength + 1));

    private WhisperProcessor _processor;
    private SegmentData _segmentData;
    private WebsocketService _server;

    public record ProgressData(float Percentage, string Label);
    
    public Callback<SegmentData> Recognised { get; } = new();
    public Callback<SegmentData> PartlyRecognised { get; } = new();
    public Callback<ProgressData> Progress { get; } = new();
    
    public RecogniserService(ILogger<RecogniserService> log, WebsocketService server, IHttpClientFactory http)
    {
        _log = log;
        _server = server;
        _http = http.CreateClient();

        _waveIn = new WaveInEvent
        {
            DeviceNumber = 0,
            WaveFormat = new WaveFormat(rate: 16000, bits: 16, channels: 1),
            BufferMilliseconds = (int)(AudioSampleLengthS * 1000),
        };
        _waveIn.DataAvailable += async (sender, e) => await WaveInOnDataAvailable(sender, e);
    }

    public async Task InitialiseAsync()
    {
        var modelPath = Path.Join("speech-recognition.bin");
        
        if (!File.Exists(modelPath))
        {
            _log.LogInformation("Downloading model");
            const string url = "https://huggingface.co/sandrohanea/whisper.net/resolve/v1/classic/ggml-tiny.en.bin";

            
            await using var fileWriter = File.OpenWrite(modelPath);
            await _http.DownloadAsync(url, fileWriter, new Progress<float>(x => _server.BroadcastAsync(new Progress(x, "Downloading speech model"))));
            
            await fileWriter.FlushAsync();
            fileWriter.Close();
            
            _log.LogDebug("Downloaded model");
        }
        
        _processor = WhisperFactory.FromPath(modelPath)
            .CreateBuilder()
            .WithSegmentEventHandler(s =>
            {
                PartlyRecognised.Set(s);
                _segmentData = s;
            })
            .SplitOnWord()
            .WithSingleSegment()
            .WithProbabilities()
            .Build();
    }
    
    public async Task StartAsync()
    {
        await InitialiseAsync();
        
        _waveIn.StartRecording();
    }
    
    public void Stop()
    {
        _waveIn.StopRecording();
        Recognised.Set(_segmentData);
    }

    private async Task WaveInOnDataAvailable(object? sender, WaveInEventArgs e)
    {
        try
        {
            var values = new short[e.Buffer.Length / 2];
            Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);
            var samples = values.Select(x => x / (short.MaxValue + 1f)).ToArray();
            
            var silenceCount = samples.Count(x => IsSilence(x, -40));
            
            if (silenceCount < values.Length - values.Length / 12)
            {
                _slidingBuffer.Add(samples);

                if (_slidingBuffer.Count > TotalBufferLength)
                {
                    _slidingBuffer.RemoveAt(0);
                }

                _processor.Process(_slidingBuffer.SelectMany(x => x).ToArray());
            }
            else if (_slidingBuffer.Count > 0)
            {
                _slidingBuffer.Clear();
                Recognised.Set(_segmentData);
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Error in recogniser");
        }
    }

    private static bool IsSilence(float amplitude, sbyte threshold)
        => GetDecibelsFromAmplitude(amplitude) < threshold;

    private static double GetDecibelsFromAmplitude(float amplitude)
        => 20 * Math.Log10(Math.Abs(amplitude));
}

public static class HttpClientExtensions
{
    public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination,
        IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        // Get the http headers first to examine the content length
        using var response =
            await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var contentLength = response.Content.Headers.ContentLength;

        await using var download = await response.Content.ReadAsStreamAsync(cancellationToken);
        // Ignore progress reporting when no progress reporter was 
        // passed or when the content length is unknown
        if (progress == null || !contentLength.HasValue)
        {
            await download.CopyToAsync(destination, cancellationToken);
            return;
        }

        // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
        var relativeProgress =
            new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));

        // Use extension method to report progress while downloading
        await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
        progress.Report(1);
    }


    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize,
        IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));
        if (bufferSize < 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize));

        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead =
                   await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }
}