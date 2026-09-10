using System;
using System.Collections.Generic;
using System.Reflection;
using InfinityHammer;
using UnityEngine;
class DeepNorthMenuTests {
 static int n;static void C(bool x,string m){if(!x)throw new Exception(m);n++;}
 static object Call(string name,params object[] args)=>typeof(DeepNorthBuildMenu).GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!.Invoke(null,args)!;
 static void Main(){
 var table=new PieceTable{custom=true};table.m_categories.Add((Piece.PieceCategory)3);table.m_categories.Add((Piece.PieceCategory)4);table.m_categoryLabels.AddRange(new[]{"First","Second"});
 for(int i=0;i<5;i++)table.m_availablePiecesByCategory.Add(new());
 var back=new BuildMenuTool{tool=new(){Instant=true,Name="back"}};var pipette=new BuildMenuTool{tool=new(){Name="Pipette"}};var other=new BuildMenuTool{tool=new(){Name="Area pipette"}};
 table.m_availablePiecesByCategory[3].AddRange(new Piece[]{back,pipette});table.m_availablePiecesByCategory[4].AddRange(new Piece[]{back,other});
 C(DeepNorthBuildMenu.TryGetIndex(table,other,out var xy,out var cat)&&xy.x==1&&(int)cat==4,"distinct same-name buttons select by reference and containing tab");
 C(DeepNorthBuildMenu.TryGetIndex(table,back,out xy,out cat)&&(int)cat==3,"shared back button doesn't use Piece category");
 C(!DeepNorthBuildMenu.TryGetIndex(table,new Piece(),out xy,out cat),"foreign piece rejected");
 var vanilla=new FakeList();var list=new HammerPieceList(vanilla);list.UpdateAvailableTags(table);C(list.TagCount==2&&list.GetTagDisplayName(1)=="Second"&&list.GetTagIdByIndex(1)==4,"custom labels and tags");
 var result=new List<Piece>();list.GetAvailablePiecesWithTag(-1,table,result);C(result.Count==3&&result[0]==back&&result[2]==other,"All preserves order and deduplicates shared back");result.Clear();list.GetAvailablePiecesWithTag(4,table,result);C(result.Count==2&&result[1]==other,"category isolation");
 table.m_categories.RemoveAt(1);table.m_categoryLabels.RemoveAt(1);list.UpdateAvailableTags(table);C(list.TagCount==1,"shrinking category set");result.Clear();list.GetAvailablePiecesWithTag(4,table,result);C(result.Count==0,"stale tag returns no stale buttons");
 var normal=new PieceTable();list.UpdateAvailableTags(normal);C(vanilla.updated==1&&list.TagCount==9,"normal hammer returns to native tags");list.GetAvailablePiecesWithTag(-1,normal,result);C(vanilla.queried==1,"normal native list delegated");
 var ui=new BuildUi();ui.m_pieceLists.Add(vanilla);Call("InstallPieceList",ui);var wrapper=ui.m_pieceLists[0];Call("InstallPieceList",ui);C(wrapper==ui.m_pieceLists[0]&&wrapper is HammerPieceList,"UI wrapper installed once per UI");
 Player.m_localPlayer=null;C((bool)Call("SelectTool",ui,pipette),"no local player doesn't execute admin tool");Player.m_localPlayer=new Player{m_buildPieces=table};C((bool)Call("SelectTool",ui,new Piece()),"ordinary selection remains native");
 Console.instance.Action=_=>Selection.Current=new Selection.State();C(!(bool)Call("SelectTool",ui,pipette),"synthetic selection intercepted");C(Console.instance.Last=="tool Pipette"&&Hud.closed==1,"continuous tool selected and menu closed");C(table.m_selectedPiece[3].x==1,"selection retained after tool command resets it");
 var oldClosed=Hud.closed;Console.instance.Action=_=>Hammer.MenuRevision++;Call("SelectTool",ui,back);C(Hud.closed==oldClosed,"navigation keeps rebuilt menu open");
 ZInput.touch=true;ZInput.doubleTap=false;Console.instance.Action=_=>{};Call("SelectTool",ui,pipette);C(Hud.closed==oldClosed,"touch single tap keeps menu");ZInput.doubleTap=true;Call("SelectTool",ui,pipette);C(Hud.closed==oldClosed+1,"touch double tap closes menu");

 var hoe=new PieceTable();hoe.m_availablePiecesByCategory.Add(new());hoe.m_availablePiecesByCategory[0].Add(pipette);
 C(DeepNorthBuildMenu.TryGetIndex(hoe,pipette,out xy,out cat)&&xy.x==0&&(int)cat==0,"uncategorized hoe finds the visible tool");
 Player.m_localPlayer.m_buildPieces=hoe;ZInput.touch=false;var before=Hud.closed;
 Call("SelectTool",ui,pipette);C(Console.instance.Last=="tool Pipette"&&Hud.closed==before+1,"uncategorized hoe tool executes and closes menu");
 var plain=new Piece();hoe.m_availablePiecesByCategory[0].Add(plain);
 Selection.Current=new Selection.State{piece=pipette};Call("ClearCommandSelection",Player.m_localPlayer,plain);
 C(Selection.Get().GetSelectedPiece()==null&&hoe.m_selectedPiece[0].x==1&&Player.m_localPlayer.setups==1,"normal hoe selection clears command ghost even in the same slot");
 Selection.Current=new Selection.State{piece=pipette};var remote=new Player{m_buildPieces=hoe};Call("ClearCommandSelection",remote,plain);
 C(Selection.Get().GetSelectedPiece()==pipette&&remote.setups==0,"remote selection leaves local preview unchanged");
 Call("ClearCommandSelection",Player.m_localPlayer,new Piece());C(Selection.Get().GetSelectedPiece()==pipette,"foreign piece cannot clear active selection");
 System.Console.WriteLine($"PASS {n} production Deep North menu assertions (Unity/UI substitutes)");
 }
}
namespace HarmonyLib{[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method,AllowMultiple=true)]public class HarmonyPatch:Attribute{public HarmonyPatch(){}public HarmonyPatch(Type t,string s,params Type[] p){}}public class HarmonyPrefix:Attribute{}public class HarmonyPostfix:Attribute{}}
namespace UnityEngine{public class Object{public static implicit operator bool(Object? o)=>o!=null;}public struct Vector2Int{public int x,y;public Vector2Int(int a,int b){x=a;y=b;}public static Vector2Int zero=>new();}}
public class Piece:UnityEngine.Object{public enum PieceCategory{Misc}public PieceCategory m_category;public T? GetComponent<T>() where T:class=>this as T;public bool TryGetComponent<T>(out T value)where T:class{value=(this as T)!;return value!=null;}}
public class PieceTable:UnityEngine.Object{public bool custom;public const int m_gridWidth=15;public List<Piece.PieceCategory> m_categories=new();public List<string> m_categoryLabels=new();public List<List<Piece>> m_availablePiecesByCategory=new();public Piece.PieceCategory m_selectedCategory;public Vector2Int[] m_selectedPiece=new Vector2Int[10];public void GetPieceIndex(){}}
public interface IPieceList{string DisplayName{get;}bool ShowTags{get;}bool CanCustomizeTags{get;}int TagCount{get;}int TagSeparatorIndex{get;}string GetTagDisplayName(int i);int GetTagIdByIndex(int i);void UpdateAvailableTags(PieceTable t);void GetAvailablePiecesWithTag(int i,PieceTable t,IList<Piece> pieces);}
public class FakeList:IPieceList{public int updated,queried;public string DisplayName=>"Usage";public bool ShowTags=>true;public bool CanCustomizeTags=>false;public int TagCount=>9;public int TagSeparatorIndex=>-1;public string GetTagDisplayName(int i)=>"native";public int GetTagIdByIndex(int i)=>i;public void UpdateAvailableTags(PieceTable t)=>updated++;public void GetAvailablePiecesWithTag(int i,PieceTable t,IList<Piece> p)=>queried++;}
public class BuildUi{public List<IPieceList> m_pieceLists=new();public void Awake(){}public void OnSelectPiece(){}}
public class Player:UnityEngine.Object{public static Player? m_localPlayer;public PieceTable m_buildPieces=null!;public float m_placePressedTime;public void PlayButtonSound(){}public void SetSelectedPiece(Piece p){}public int setups;public void SetupPlacementGhost(){setups++;}}
public class Console{public static Console instance=new();public Action<string> Action=_=>{};public string Last="";public bool TryRunCommand(string c){Last=c;Action(c);return true;}}
public static class Hud{public static int closed;public static void CloseBuildUi()=>closed++;}
public static class ZInput{public static bool touch,doubleTap;public static bool IsTouchActive()=>touch;public static bool HasDoubleTapped()=>doubleTap;}
namespace InfinityHammer{public class Tool{public bool Instant;public string Name="";}public class BuildMenuTool:Piece{public Tool? tool;}public static class Hammer{public static int MenuRevision;public static bool IsInfinityHammer(PieceTable t)=>t.custom;}public static class Selection{public class State{public Piece? piece;public Piece? GetSelectedPiece()=>piece;}public static State Current=new();public static State Get()=>Current;public static void Clear()=>Current=new();}public static class TakeOverBuildMenu{public static string GetInstantCommand(Tool t,Player p)=>t.Name;}}
