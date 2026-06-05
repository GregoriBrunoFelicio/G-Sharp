using GSharp.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Server;

// The OmniSharp server type shares the simple name "LanguageServer" with this project's
// namespace, so alias it to keep the bootstrap unambiguous.
using OmniSharpServer = OmniSharp.Extensions.LanguageServer.Server.LanguageServer;

var server = await OmniSharpServer.From(options =>
    options
        .WithInput(Console.OpenStandardInput())
        .WithOutput(Console.OpenStandardOutput())
        .WithHandler<TextDocumentHandler>()
        .WithHandler<HoverHandler>());

await server.WaitForExit;
