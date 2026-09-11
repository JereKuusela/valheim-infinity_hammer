using System;
using System.Collections.Generic;
using InfinityHammer;
using InfinityTools;
using UnityEngine;
class ToolMenuPiecesTests
{
 static int n;
 static void C(bool ok,string why){if(!ok)throw new Exception(why);n++;}
 static void Main(){
 var tool=new Tool{Name="Level",Description="Level terrain"};
 var first=ToolMenuPieces.Get(tool);
 C(!first.gameObject.activeSelf,"menu template is inactive and cannot run Piece behaviour at the origin");
 var table=new List<BuildMenuTool>{first};var visible=table[0];
 for(int i=0;i<100;i++){table.Clear();table.Add(ToolMenuPieces.Get(tool));}
 C(table.Contains(visible),"same-count availability rebuilds preserve the visible button identity");
 C(GameObject.created==1,"rebuilding availability does not leak tool templates");
 tool.Description="New binding";C(ToolMenuPieces.Get(tool).m_description=="New binding","cached descriptions refresh");
 var other=ToolMenuPieces.Get(new Tool{Name="Level"});C(other!=first,"same-name tools on different equipment stay independent");
 ToolMenuPieces.Clear();C(first.gameObject.destroyed&&other.gameObject.destroyed,"world/reload cleanup destroys all owned templates");
 var next=ToolMenuPieces.Get(tool);C(next!=first,"next world receives a new live template");
 UnityEngine.Object.Destroy(next.gameObject);C(ToolMenuPieces.Get(tool)!=next,"destroyed Unity entries are recreated");
 ToolMenuPieces.Clear();ToolMenuPieces.Clear();C(GameObject.live==0,"cleanup is idempotent with no remaining templates");
 System.Console.WriteLine($"PASS {n} production menu-template assertions (Unity substitutes)");
 }
}
namespace UnityEngine{
 public class Object{public bool destroyed;public static implicit operator bool(Object? o)=>o!=null&&!o.destroyed;public static void Destroy(Object? o){if(o is GameObject g&&!g.destroyed){g.destroyed=true;foreach(var c in g.components)c.destroyed=true;GameObject.live--;}}}
 public class GameObject:Object{public static int created,live;public bool activeSelf=true;public List<Object> components=new();public GameObject(string name){created++;live++;}public void SetActive(bool a)=>activeSelf=a;public T AddComponent<T>()where T:Component,new(){var t=new T{gameObject=this};components.Add(t);return t;}}
 public class Component:Object{public GameObject gameObject=null!;}
 public class Sprite:Object{}
}
namespace InfinityTools{public class Tool{public string Name="",Description="";public Sprite? Icon;}}
namespace InfinityHammer{public class BuildMenuTool:Component{public Tool? tool;public string m_name="",m_description="";public Sprite? m_icon;}}
