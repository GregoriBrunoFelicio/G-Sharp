using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

// Protocol.Models.Range collides with System.Range under ImplicitUsings — alias it.
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace GSharp.LanguageServer;

/// <summary>
/// Answers hover requests by looking up the cursor position in the latest analysis for the
/// document and reporting the inferred type of whatever is under it — a function's full
/// signature, a binding's type, a literal's type.
///
/// Returning null is a valid LSP response ("nothing to show here"), used when the document
/// hasn't been analysed yet, failed to analyse, or the cursor isn't on a typed construct.
/// </summary>
public class HoverHandler(DocumentStore store) : HoverHandlerBase
{
    protected override HoverRegistrationOptions CreateRegistrationOptions(
        HoverCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("gsharp")
        };

    public override Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        var analysis = store.Get(request.TextDocument.Uri);
        if (analysis is null)
            return Task.FromResult<Hover?>(null);

        // LSP positions are 0-based; the AST/lexer coordinates are 1-based.
        var line      = request.Position.Line + 1;
        var character = request.Position.Character + 1;

        var result = HoverProvider.Find(analysis, line, character);
        if (result is null)
            return Task.FromResult<Hover?>(null);

        var hover = new Hover
        {
            Contents = new MarkedStringsOrMarkupContent(new MarkupContent
            {
                Kind  = MarkupKind.Markdown,
                Value = result.Markdown
            }),
            Range = new Range(
                new Position(result.Line - 1, result.StartColumn - 1),
                new Position(result.Line - 1, result.EndColumn - 1))
        };

        return Task.FromResult<Hover?>(hover);
    }
}
