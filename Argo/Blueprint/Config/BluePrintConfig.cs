using System.Collections.Generic;
using System.Text.Json.Serialization;
using Argo.DataAnalysis;
using Argo.Zdo;

namespace Argo.Blueprint;

public class BluePrintConfig
{
    [JsonIgnore] public SaveExtraData SaveExtraData = SaveExtraData.All;
    [JsonIgnore] public BuilderRegister   Register       ;
    [JsonIgnore] public HashRegister       Info          ;
    [JsonIgnore] public string        SnapPiece   = ""; // todo add branche for snappiece
    [JsonIgnore] public string        CenterPiece = "";
    public              bool          Profile     = false;
    public BluePrintConfig(BuilderRegister? register = null, HashRegister? info = null) {
        Register = register ?? BuilderRegister.GetInstance();
        Info     = info     ?? HashRegister.GetDefault();
        ;
    }
    public BluePrintConfig(Dictionary<string, object> pars) : this() {
        foreach (var pair in pars) {
            switch (pair.Key.ToLowerInvariant()) {
                case ("savedata"):
                    if (pair.Value is SaveExtraData)
                        SaveExtraData = (SaveExtraData)pair.Value;
                    break;
                case ("register"):
                    if (pair.Value is BuilderRegister)
                        Register = (BuilderRegister)pair.Value;
                    break;
                case ("info"):
                    if (pair.Value is HashRegister) this.Info = (HashRegister)pair.Value;

                    break;
                case ("snappiece"):
                    if (pair.Value is string) SnapPiece = (string)pair.Value;
                    break;
                case ("centerpiece"):
                    if (pair.Value is string) CenterPiece = (string)pair.Value;
                    break;
                case ("profile"):
                    if (pair.Value is bool) Profile = (bool)pair.Value;
                    break;
            }
        }
    }
}