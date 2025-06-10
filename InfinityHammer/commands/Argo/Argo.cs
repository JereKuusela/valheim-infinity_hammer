using System;
using System.IO;
using ServerDevcommands;
using Argo.DataAnalysis;
namespace InfinityHammer;

public class ArgoExportPrefabData
{
    public ArgoExportPrefabData() {
        AutoComplete.Register("argo_export_prefab" , (int index) => {
            if (index == 0) return ParameterInfo.Create("File name.");
            return [];
        }, new());
        Helper.Command("argo_export_prefab",
            "[file name] Saves data of all Prefavs json file.",
            (args) => {
                Helper.ArgsCheck(args, 2, "Filename name is missing.");
                try
                {
                    var data = new PrefabData();
                    var name = Path.GetFileNameWithoutExtension(args[1]) +
                        ".json";
                    
                    //var lines = data.ToJson();
                    
                    var path =
                        Path.Combine(
                            Configuration.SaveBlueprintsToProfile
                                ? Configuration.BlueprintLocalFolder
                                : Configuration.BlueprintGlobalFolder, name);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    //    File.Create(path);
                 //   data.GetData(path);
                    //File.WriteAllLines(path, lines);

                } catch (Exception e)
                {
                    System.Console.WriteLine("argo export: " + e);
                }
            });
            
        {
            
        }
    }
}

