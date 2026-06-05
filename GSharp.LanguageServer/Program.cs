using GSharp.LanguageServer;
using Microsoft.Extensions.DependencyInjection;
using OmniSharp.Extensions.LanguageServer.Server;

// The OmniSharp server type shares the simple name "LanguageServer" with this project's
// namespace, so alias it to keep the bootstrap unambiguous.
using OmniSharpServer = OmniSharp.Extensions.LanguageServer.Server.LanguageServer;

var server = await OmniSharpServer.From(options =>
    options
        .WithInput(Console.OpenStandardInput())
        .WithOutput(Console.OpenStandardOutput())
        // Shared across handlers: the text handler writes each analysis, hover reads it.
        .WithServices(services => services.AddSingleton<DocumentStore>())
        .WithHandler<TextDocumentHandler>()
        .WithHandler<HoverHandler>());

await server.WaitForExit;
