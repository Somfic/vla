using Catalyst;
using Microsoft.Extensions.Logging;
using Mosaik.Core;
using Nito.Disposables.Internals;


namespace Vla.Voice;

public class SpeechProcessorService
{
    private Pipeline _processor;
    public ILogger<SpeechProcessorService> Log { get; }

    public SpeechProcessorService(ILogger<SpeechProcessorService> log)
    {
        Log = log;
    }

    public async Task InitialiseAsync()
    {
        Log.LogInformation("Initialising speech");
        
        // TODO: Add support for more languages
        Catalyst.Models.English.Register();
        Storage.Current = new DiskStorage("catalyst-models");
        _processor = await Pipeline.ForAsync(Language.English);
    }

    private readonly string[] _blacklisted = { "can", "may", "get", "thank", "do" };
    
    public IReadOnlyCollection<IToken> Process(string text, Language language = Language.English)
    {
        var doc = new Document(text.Trim().ToLower(), language);
        doc.TrimTokens();
        doc.RemoveOverlapingTokens();

        return _processor.ProcessSingle(doc)
            .ToTokenList()
            .WhereNotNull()
            .Where(x => x.POS is PartOfSpeech.NOUN or PartOfSpeech.VERB or PartOfSpeech.PART or PartOfSpeech.ADV)
            .Where(x => !_blacklisted.Contains(x.Value))
            .ToList();
    }
}