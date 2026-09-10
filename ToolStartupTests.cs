using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using InfinityTools;
using BepInEx;
class ToolStartupTests{
 static int n;static void C(bool x,string m){if(!x)throw new Exception(m);n++;}
 static void Main(){Paths.ConfigPath=Path.Combine(Path.GetTempPath(),"ih-tools-"+Guid.NewGuid());Directory.CreateDirectory(Paths.ConfigPath);
 try{
 ToolManager.Initialize();C(ToolManager.Tools.ContainsKey("hammer"),"first initialization loads tools without watcher event");C(Service.Yaml.Loads==2,"new file immediately loaded after creation");C(ToolManager.Get("hammer").Count==1,"default loaded once");C(File.ReadAllText(Path.Combine(Paths.ConfigPath,"infinity_tools.yaml"))=="DEFAULT","default file written");
 File.WriteAllText(Path.Combine(Paths.ConfigPath,"infinity_tools.yaml"),"custom");ToolManager.FromFiles();C(ToolManager.Get("hammer")[0].Name=="custom","existing custom tools respected");C(File.ReadAllText(Path.Combine(Paths.ConfigPath,"infinity_tools.yaml"))=="custom","custom file not overwritten");
 Service.Yaml.Invalid=true;ToolManager.FromFiles();C(ToolManager.Tools.Count==0&&ServerDevcommands.Log.Warnings==1,"invalid existing file reported, not replaced with defaults");C(File.ReadAllText(Path.Combine(Paths.ConfigPath,"infinity_tools.yaml"))=="custom","invalid file preserved");
 System.Console.WriteLine($"PASS {n} production tool initialization assertions (filesystem real; parser/watcher substitutes)");
 }finally{Directory.Delete(Paths.ConfigPath,true);}
 }
}
namespace BepInEx{public static class Paths{public static string ConfigPath="";}}
namespace HarmonyLib{public class HarmonyPatch:Attribute{}}
namespace InfinityTools{public class ToolData{public string name="";public bool IsDefaultData;}public class Tool{public string Name;public Tool(ToolData d){Name=d.name;}}public static class InitialData{public static string Get()=>"DEFAULT";}}
public class Player{public static Player? m_localPlayer;public void UpdateAvailablePiecesList(){}}
namespace ServerDevcommands{public static class Settings{public static string Substitution="";}public static class AliasManager{public static void AddAlias(string a,string b){}}public static class Log{public static int Warnings;public static void Info(string s){}public static void Warning(string s)=>Warnings++;}}
namespace Service{public static class Yaml{
 public static int Loads;public static bool Invalid;
 public static void ConsolidateDefaultFile(string d,string f,string n){var p=Path.Combine(d,n);if(!File.Exists(p))File.WriteAllText(p,"");}
 public static bool AnyFileExists(string d,string p,string f)=>File.ReadAllText(Path.Combine(d,"infinity_tools.yaml")).Length>0;
 public static void LoadDictFromDirectory<T>(string d,string p,string f,Action<string,string,T> action){Loads++;var path=Path.Combine(d,"infinity_tools.yaml");var s=File.Exists(path)?File.ReadAllText(path):"";if(s.Length==0||Invalid)return;action("infinity_tools.yaml","hammer",(T)(object)new List<ToolData>{new(){name=s=="DEFAULT"?"Pipette":s}});}
 public static bool IsDefaultFile(string f,string d,string n)=>f==n;public static void SetupWatcher(string d,string p,string f,Action a){}public static T Deserialize<T>(string s,string f)=>throw new NotImplementedException();public static SerializerStub Serializer()=>new();public class SerializerStub{public string Serialize(object x)=>"";}
}}

namespace InfinityHammer{internal static class ToolMenuPieces{internal static void Clear(){}}}
public class Hud{public static Hud? instance;public static implicit operator bool(Hud? h)=>h!=null;public static bool IsPieceSelectionVisible()=>false;public BuildUi m_buildUi=new();}public class BuildUi{public object? m_currentBuildTool;public void OpenBuildMenu(){}}
