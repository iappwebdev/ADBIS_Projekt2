using WatDiv.Config.FileConfig;
using WatDiv.Config.PropConfig;
using WatDiv.Sanitizing;
using WatDiv.WatDivProcessing;

const bool isTestmode = false;

var processor = new WatDivProcessor(
    GetFileNameProvider(),
    new PropConfig(),
    GetSanitizer()
);

processor.Start();

IFileNameProvider GetFileNameProvider() => isTestmode ? new FileNameProvider100k() : new FileNameProvider10M();
ISanitizer GetSanitizer() => isTestmode ? new Sanitizer100k() : new Sanitizer10M();