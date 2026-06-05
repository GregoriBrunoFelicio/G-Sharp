using System.Collections.Concurrent;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace GSharp.LanguageServer;

/// <summary>
/// Holds the latest <see cref="AnalysisResult"/> for every open document.
///
/// Registered as a singleton so the two handlers can share it: the
/// <see cref="TextDocumentHandler"/> writes a fresh result on every open/change, and the
/// <see cref="HoverHandler"/> reads it to answer hover requests without re-parsing.
/// </summary>
public class DocumentStore
{
    private readonly ConcurrentDictionary<DocumentUri, AnalysisResult> _analyses = new();

    public void Set(DocumentUri uri, AnalysisResult result) => _analyses[uri] = result;

    public AnalysisResult? Get(DocumentUri uri) =>
        _analyses.TryGetValue(uri, out var result) ? result : null;

    public void Remove(DocumentUri uri) => _analyses.TryRemove(uri, out _);
}
