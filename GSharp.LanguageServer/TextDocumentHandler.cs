using System.Collections.Concurrent;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

// Protocol.Models.Range collides with System.Range under ImplicitUsings — alias it.
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace GSharp.LanguageServer;

/// <summary>
/// Keeps each open G# document in sync and re-runs <see cref="DocumentAnalyzer"/> on every
/// open/change, publishing the resulting diagnostics back to the editor.
/// Uses full-text sync (TextDocumentSyncKind.Full), so each change carries the whole buffer.
/// </summary>
public class TextDocumentHandler(ILanguageServerFacade server) : TextDocumentSyncHandlerBase
{
    private const string LanguageId = "gsharp";

    private readonly ConcurrentDictionary<DocumentUri, string> _documents = new();

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage(LanguageId),
            Change = TextDocumentSyncKind.Full,
            Save = new SaveOptions { IncludeText = false }
        };

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) =>
        new(uri, LanguageId);

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        AnalyzeAndPublish(request.TextDocument.Uri, request.TextDocument.Text);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        // Full sync — the whole document arrives as the last content change.
        var text = request.ContentChanges.LastOrDefault()?.Text ?? string.Empty;
        AnalyzeAndPublish(request.TextDocument.Uri, text);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        _documents.TryRemove(request.TextDocument.Uri, out _);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken) =>
        Unit.Task;

    private void AnalyzeAndPublish(DocumentUri uri, string text)
    {
        _documents[uri] = text;

        var diagnostics = DocumentAnalyzer.Analyze(text)
            .Select(diagnostic => ToLspDiagnostic(diagnostic, text))
            .ToArray();

        server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
        {
            Uri         = uri,
            Diagnostics = new Container<Diagnostic>(diagnostics)
        });
    }

    private static Diagnostic ToLspDiagnostic(AnalyzerDiagnostic diagnostic, string text)
    {
        // Span the offending line so the squiggle is visible even without column precision.
        var endColumn = Math.Max(LineLength(text, diagnostic.Line), diagnostic.Column + 1);

        return new Diagnostic
        {
            Severity = ToLspSeverity(diagnostic.Severity),
            Source   = LanguageId,
            Message  = diagnostic.Message,
            Range    = new Range(
                new Position(diagnostic.Line, diagnostic.Column),
                new Position(diagnostic.Line, endColumn))
        };
    }

    private static DiagnosticSeverity ToLspSeverity(AnalyzerSeverity severity) => severity switch
    {
        AnalyzerSeverity.Warning     => DiagnosticSeverity.Warning,
        AnalyzerSeverity.Information => DiagnosticSeverity.Information,
        AnalyzerSeverity.Hint        => DiagnosticSeverity.Hint,
        _                            => DiagnosticSeverity.Error
    };

    private static int LineLength(string text, int zeroBasedLine)
    {
        var lines = text.Split('\n');
        if (zeroBasedLine < 0 || zeroBasedLine >= lines.Length)
            return 1;
        return lines[zeroBasedLine].TrimEnd('\r').Length;
    }
}
