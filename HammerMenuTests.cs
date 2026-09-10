using System;
using InfinityHammer;
using ServerDevcommands;
class HammerMenuTests{
 static int n;static void C(bool ok,string why){if(!ok)throw new Exception(why);n++;}
 static void Run(string s)=>Helper.execute(new Terminal.ConsoleEventArgs(s));
 static void Main(){
 new HammerMenuCommand();Run("hammer_menu");C(Hud.open&&HammerMenuCommand.CurrentMode==MenuMode.Menu,"root menu opens");
 Run("hammer_menu");C(!Hud.open,"repeated root command closes");Run("hammer_menu");C(Hud.open,"root can reopen after closing");
 Run("hammer_menu locations");C(Hud.open&&HammerMenuCommand.CurrentMode==MenuMode.Locations,"subpage remains open");
 Run("hammer_menu locations");C(!Hud.open,"repeated subpage hotkey closes");Run("hammer_menu locations");
 Run("hammer_menu navigate 2");C(HammerMenuCommand.CurrentPage==2,"pagination navigates");
 Run("hammer_menu back");C(Hud.open&&HammerMenuCommand.CurrentMode==MenuMode.Locations&&HammerMenuCommand.CurrentPage==-1,"back restores previous page");
 Run("hammer_menu back");C(Hud.open&&HammerMenuCommand.CurrentMode==MenuMode.Menu,"back returns to root");
 Run("hammer_menu close");C(!Hud.open,"explicit close works on the root");
 Run("hammer_menu builds Hoe");C(HammerMenuCommand.CurrentFilter=="Hoe"&&Hud.open,"hoe page opens");Run("hammer_menu close");C(!Hud.open,"explicit close also works on borrowed hoe table");
 Hud.open=true;Hammer.custom=false;Run("hammer_menu");C(Hud.open&&Hammer.custom,"root command replaces an ordinary build menu instead of closing it");
 System.Console.WriteLine($"PASS {n} production menu-command assertions (UI substitutes)");
 }
}
namespace HarmonyLib{}
namespace ServerDevcommands{
 public static class AutoComplete{public static void Register(string s,Func<int,int,System.Collections.Generic.List<string>> f){}}
 public static class ParameterInfo{public static System.Collections.Generic.List<string> Create(string s)=>new();public static System.Collections.Generic.List<string> None=>new();}
 public static class Helper{public static Action<Terminal.ConsoleEventArgs> execute=null!;public static void Command(string n,string h,Action<Terminal.ConsoleEventArgs> f)=>execute=f;public static Player GetPlayer()=>new();}
 public static class Parse{public static int? IntNull(string s)=>int.TryParse(s,out int n)?n:null;}
}
public class Terminal{public class ConsoleEventArgs{string[] args;public ConsoleEventArgs(string s){args=s.Split(' ');}public int Length=>args.Length;public string this[int i]=>args[i];public Terminal Context=>new();}}
public class Player{public object GetRightItem()=>new();}
public static class Hud{public static bool open;public static bool IsPieceSelectionVisible()=>open;public static void CloseBuildUi()=>open=false;}
namespace InfinityHammer{
 public static class Hammer{public static bool custom=true;public static void OpenBuildMenu(){Hud.open=true;custom=true;}public static bool IsInfinityHammer(object item)=>custom;}
 public static class HammerHelper{public static void Message(Terminal t,string s){}}
}
