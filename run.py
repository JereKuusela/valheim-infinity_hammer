#!/usr/bin/env python3
"""Run managed production-source tests with Unity/UI substitutes; no game DLLs required."""
import argparse, pathlib, subprocess, tempfile
p=argparse.ArgumentParser();p.add_argument('--sdk',required=True,type=pathlib.Path);a=p.parse_args()
sdk=a.sdk.resolve();here=pathlib.Path(__file__).resolve().parent;root=here.parent.parent/'InfinityHammer'
csc=sorted((sdk/'sdk').glob('*/Roslyn/bincore/csc.dll'))[-1]
refs=sorted((sdk/'packs/Microsoft.NETCore.App.Ref').glob('8.*/ref/net8.0'))[-1]
cases={'DeepNorthMenuTests':'buildMenu/DeepNorthBuildMenu.cs','ToolMenuPiecesTests':'buildMenu/ToolMenuPieces.cs','HammerMenuTests':'commands/HammerMenu.cs','ToolStartupTests':'commands/ToolManager.cs'}
with tempfile.TemporaryDirectory() as tmp:
 out=pathlib.Path(tmp)
 for name,src in cases.items():
  dll=out/(name+'.dll');rsp=out/(name+'.rsp')
  rsp.write_text('\n'.join(['-nologo','-langversion:preview','-nullable:enable','-target:exe','-nostdlib+','-out:"'+str(dll)+'"']+['-r:"'+str(f)+'"' for f in refs.glob('*.dll')]+['"'+str(here/(name+'.cs'))+'"','"'+str(root/src)+'"']))
  subprocess.run([str(sdk/'dotnet'),str(csc),'@'+str(rsp)],check=True)
  dll.with_suffix('.runtimeconfig.json').write_text('{"runtimeOptions":{"tfm":"net8.0","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"},"rollForward":"LatestPatch"}}')
  subprocess.run([str(sdk/'dotnet'),str(dll)],check=True)
