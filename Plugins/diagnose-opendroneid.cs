using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MissionPlanner.Plugin;
using MissionPlanner.Utilities;
using log4net;

namespace MissionPlanner
{
    public class DiagnoseOpenDroneID : Plugin
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Name { get { } = "Diagnose OpenDroneID";
        public override string Version { get { } = "0.01";
        public override string Author { get { } = "Diag";

        public override bool Init()
        {
            try
            {
                var pluginPath = Settings.GetRunningDirectory() + "plugins" + Path.DirectorySeparatorChar;
                log.Info($"[DiagnoseOpenDroneID] Plugin path: {pluginPath}");

                var allDlls = Directory.GetFiles(pluginPath, "*.dll");
                log.Info($"[DiagnoseOpenDroneID] All DLLs in plugins: {string.Join(", ", allDlls.Select(Path.GetFileName))}");

                var targetDll = allDlls.FirstOrDefault(f => Path.GetFileName(f).ToLower().Contains("opendroneid"));
                if (targetDll == null)
                {
                    log.Error("[DiagnoseOpenDroneID] opendroneid.dll NOT FOUND in plugins directory");
                    return false;
                }

                log.Info($"[DiagnoseOpenDroneID] Found DLL: {Path.GetFileName(targetDll)}");

                if (PluginLoader.DisabledPluginNames.Contains(Path.GetFileName(targetDll).ToLower()))
                {
                    log.Error($"[DiagnoseOpenDroneID] {Path.GetFileName(targetDll)} is in DisabledPluginNames - will NOT load");
                }
                else
                {
                    log.Info($"[DiagnoseOpenDroneID] {Path.GetFileName(targetDll)} is NOT in DisabledPluginNames - should load");
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[DiagnoseOpenDroneID] Exception during Init: {ex}");
                return false;
            }
        }

        public override bool Loaded()
        {
            return true;
        }

        public override bool Exit()
        {
            return true;
        }
    }
}
