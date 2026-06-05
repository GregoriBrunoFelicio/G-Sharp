using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace GSharp.LanguageServer;

/// <summary>
/// Advertises hover support so editors that request hovers (e.g. an nvim CursorHold
/// autocmd calling vim.lsp.buf.hover) don't error with "hover not supported".
///
/// Phase 1 returns no content: mapping a cursor position to an inferred type needs
/// source spans on the AST, which is the phase-2 (hover) foundation. Returning null
/// is a valid LSP response ("nothing to show here").
/// </summary>
public class HoverHandler : HoverHandlerBase
{
    protected override HoverRegistrationOptions CreateRegistrationOptions(
        HoverCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("gsharp")
        };

    public override Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken) =>
        Task.FromResult<Hover?>(null);
}
