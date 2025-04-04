using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Argo.Blueprint;
using Argo.Zdo;

namespace Argo.Blueprint.Util;

public abstract class Register
{
    
}

public abstract class ObjectPool<T> where T : ObjectPool<T>
{
    protected static readonly T    _defaultInstance;
    // todo add Modified Vanilla Pieces list to all instances except vanilla instance
    protected ImmutableDictionary<ModIdentifier, T> _ModInstances;
    protected ImmutableDictionary<ModIdentifier, bool>    _ModActive;
    static readonly Func<T> _EmptyCreator;
    
    protected struct ActiveMod()
    {
        public T    mod;
        public bool active;
    }
    
    protected  readonly       bool _isVanillaInstance;

    protected static readonly Func<T> VanillCreator;
    protected static Dictionary<ModIdentifier, T> _modInstances = new Dictionary<ModIdentifier, T>();
    protected static Dictionary<ModIdentifier, bool> _activeMods
        = new Dictionary<ModIdentifier, bool>();
    protected bool _isDefaultInstance;
    static ObjectPool() {
        var        type     = typeof(T);
        MethodInfo _default = type.GetMethod("CreateDefaultInstance");
        MethodInfo _empty = type.GetMethod("CreateEmpty");
        _EmptyCreator    = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), _empty);
        _defaultInstance = (T)_default.Invoke(null, null);
       // MethodInfo _common = type.GetMethod("CreateDefaultInstance");
        //defaultInstance = (T)_common.Invoke(null, null);
    }
    public abstract T Clone();

    public static T CreateEmpty() {
        return _EmptyCreator();
    }
    
    public void AddMod(ModIdentifier mod, bool active = true) {
        this._ModInstances.Add(mod, CreateEmpty());
        this._ModActive.Add(mod, active);
    }

    public abstract void AddToMod(ModIdentifier mod, params object[] pairs);
    public static   T    GetDefault()        => _defaultInstance;
    public static   T    GetCommonInstance() => _defaultInstance;
    private static readonly Dictionary<string, T> instances =
        new Dictionary<string, T>();

    protected ObjectPool(bool isDefault) {
        if ((isDefault == true) && (_defaultInstance != null)) {
            throw new ArgumentException("Default instance already set");
        }
        
    }

   
}