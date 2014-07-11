using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
