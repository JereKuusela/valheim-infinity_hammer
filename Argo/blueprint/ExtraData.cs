using System;
using System.Collections.Generic;
using System.Linq;
using Argo.Blueprint;
using Argo.DataAnalysis;

namespace Argo.blueprint;
using static BpjZVars;
public abstract class AExtraData
{
    public abstract Dictionary<string, ZValue> import(
        Func<int, bool>                    filter);
    protected abstract Dictionary<T, U> GetDic<T, U>(T value);
    
    public Dictionary<string, BpjZVars.ZValue> GetFilteredData(HashSet<int> hashes, bool Exclude ) {
        
        if (Exclude) {
            return import((x) => !hashes.Contains( x ));
        } else {
            return import((x) => hashes.Contains( x ));
        }
    }
}