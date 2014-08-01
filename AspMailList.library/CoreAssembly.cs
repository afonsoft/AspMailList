using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace AspMailList.library
{
    /// <summary>
    /// Class for get Version of Assembly
    /// </summary>
    public static class CoreAssembly
    {
        public static readonly Assembly Reference = typeof(CoreAssembly).Assembly;
        public static readonly Version Version = Reference.GetName().Version;

        private static bool _IsRunningOnMono = (Type.GetType("Mono.Runtime") != null);
        private static bool _IsRunningOnWindows = RuntimeEnvironment.GetRuntimeDirectory().Contains("Microsoft");

        public static bool IsRunningOnMono()
        {
            if (_IsRunningOnMono)
                return true;
            else if (_IsRunningOnWindows)
                return false;
            else
                return true;
        }

        /// <summary>
        /// get File Version
        /// </summary>
        public static string getFileVersion
        {
            get
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Reference.Location);
                return fvi.FileVersion;
            }
        }

        /// <summary>
        /// get Version
        /// </summary>
        public static string getVersion
        {
            get
            {
                return Version.ToString();
            }
        }
    }
}
