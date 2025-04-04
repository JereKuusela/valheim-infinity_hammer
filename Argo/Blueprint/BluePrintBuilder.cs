using System.Collections.Generic;

namespace Argo.Blueprint;

public class BluePrintBuilder
{
    // todo this list is for builders still in progress to prevent garbage collection to
    //      destroy them while still runing since they are using coroutines
    List<BluePrintBuilder> PendingBuilders = [];
}