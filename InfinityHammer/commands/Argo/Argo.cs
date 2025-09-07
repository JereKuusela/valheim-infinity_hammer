using System;
using System.IO;
using ServerDevcommands;
using Argo.DataAnalysis;

namespace InfinityHammer;

public class ArgoExportPrefabData
{
    public ArgoExportPrefabData() {
        AutoComplete.Register("argo_export_prefab", (int index) => {
            if (index == 0) return ParameterInfo.Create("File name.");
            return [];
        }, new());
        Helper.Command("argo_export_prefab",
            "[file name] Saves data of all Prefavs json file.",
            (args) => {
                Helper.ArgsCheck(args, 2, "Filename name is missing.");
                try {
                    var name = Path.GetFileNameWithoutExtension(args[1]) +
                        ".json";

                    //var lines = data.ToJson();

                    var path =
                        Path.Combine(
                            Configuration.SaveBlueprintsToProfile
                                ? Configuration.BlueprintLocalFolder
                                : Configuration.BlueprintGlobalFolder, name);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    PrefabDataMining prefabData   = PrefabDataMining.ReadData();
                    var              lines        = prefabData.SerializeData();
                    StreamWriter     streamWriter = new StreamWriter(path, false);

                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                        using (var writer = new StreamWriter(fs)) {
                            writer.Write(string.Join(Environment.NewLine, lines));
                        }
                    }
                } catch (Exception e) {
                    System.Console.WriteLine("argo export: " + e);
                }
            });

        { }
    }
}