using System;
using System.IO;
using System.Reflection;
using OSGeo.GDAL;
using OSGeo.OGR;

namespace GDAL
{
    public static class GdalConfigurationMP
    {
        private static bool _gdalConfigured;
        private static bool _ogrConfigured;

        public static void ConfigureGdal()
        {
            if (_gdalConfigured)
                return;

            try
            {
                var executingAssemblyFile = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath;
                var executingDirectory = Path.GetDirectoryName(executingAssemblyFile);
                if (!string.IsNullOrEmpty(executingDirectory))
                {
                    var gdalPath = Path.Combine(executingDirectory, "gdal");

                    var gdalData = Path.Combine(gdalPath, "data");
                    if (Directory.Exists(gdalData))
                        Gdal.SetConfigOption("GDAL_DATA", gdalData);

                    var projLib = Path.Combine(gdalPath, "projlib");
                    if (Directory.Exists(projLib))
                        Gdal.SetConfigOption("PROJ_LIB", projLib);
                }
            }
            catch
            {
            }

            Gdal.UseExceptions();
            Gdal.AllRegister();

            _gdalConfigured = true;
        }

        public static void ConfigureOgr()
        {
            if (_ogrConfigured)
                return;

            Ogr.UseExceptions();
            Ogr.RegisterAll();

            _ogrConfigured = true;
        }
    }
}
