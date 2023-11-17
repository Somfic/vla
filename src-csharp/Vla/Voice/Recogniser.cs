using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Somfic.Common;
using Vosk;

namespace Vla.Voice;

public class RecogniserService
{
    private static Model model = new(@"C:\Users\Lucas\Downloads\vosk-model-en-us-0.22-lgraph\vosk-model-en-us-0.22-lgraph");
    //private static Model model = new(@"C:\Users\Lucas\Downloads\vosk-model-en-us-0.42-gigaspeech\vosk-model-en-us-0.42-gigaspeech");

    private readonly VoskRecognizer _recogniser;
    private readonly ILogger<RecogniserService> _log;
    private readonly WaveInEvent _waveIn;

    public AsyncCallback<string> Recognised { get; } = new();
    public AsyncCallback<string> PartlyRecognised { get; } = new();
    

    public RecogniserService(ILogger<RecogniserService> log)
    {
        _log = log;

        _recogniser = new VoskRecognizer(model, 16000f);

        _waveIn = new WaveInEvent();
        _waveIn.WaveFormat = new WaveFormat(16000, 1);
        _waveIn.DataAvailable += async (sender, e) => await WaveInOnDataAvailable(sender, e);
    }

    public void Start()
    {
        _waveIn.StartRecording();
    }

    private async Task WaveInOnDataAvailable(object? sender, WaveInEventArgs e)
    {
        try
        {
            if (_recogniser.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                var result = _recogniser.Result();
                await Recognised.Set(result);
            }
            else
            {
                var partialResult = _recogniser.PartialResult();
                await PartlyRecognised.Set(partialResult);
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Error in recogniser");
        }
    }
}