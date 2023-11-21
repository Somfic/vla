using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Somfic.Common;
using Whisper.net;
using Whisper.net.Ggml;

namespace Vla.Voice;

public class RecogniserService
{
    private const GgmlType ModelType = GgmlType.BaseEn;
    private const double AudioSampleLengthS = 1;
    private const double TotalBufferLength = 10 / AudioSampleLengthS;
    
    private readonly ILogger<RecogniserService> _log;
    private readonly WaveInEvent _waveIn;
    private readonly List<float[]> _slidingBuffer = new((int)(TotalBufferLength + 1));

    private WhisperProcessor _processor;
    private SegmentData _segmentData;
    
    public AsyncCallback<SegmentData> Recognised { get; } = new();
    public AsyncCallback<SegmentData> PartlyRecognised { get; } = new();
    
    public RecogniserService(ILogger<RecogniserService> log)
    {
        _log = log;

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
        if (!File.Exists(ModelType.ToString()))
        {
            _log.LogInformation("Downloading model");
            await using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ModelType);
            await using var fileWriter = File.OpenWrite(ModelType.ToString());
            await modelStream.CopyToAsync(fileWriter);
            
            _log.LogDebug("Downloaded model");
        }
        
        _processor = WhisperFactory.FromPath(ModelType.ToString())
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
    
    public async Task StopAsync()
    {
        _waveIn.StopRecording();
        await Recognised.Set(_segmentData);
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
                await Recognised.Set(_segmentData);
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