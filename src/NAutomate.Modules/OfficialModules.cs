using NAutomate.Abstractions;
using NAutomate.Modules.Core;
using System.Collections.Generic;

namespace NAutomate.Modules;

public static class OfficialModules
{
    public static IEnumerable<IAutomationModule> GetModules()
    {
        yield return new EchoModule();
    }
}
