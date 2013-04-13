using System;
using System.Reflection;

namespace SQLServerBackupTool.Lib.Versioning
{
    /// <summary>
    /// Utilitaire d'aide à l'affichage du numero de version
    /// Compatible avec tous les SCM, il suffit juste de créer la bonne task de build
    /// </summary>
    public static class VersionInfo
    {
        private static Version _cachedVersion;
        private static string _cachedVersionHash;
        private static Assembly _cachedAssembly;

        /// <summary>
        /// Assembly en cours d'éxécution
        /// </summary>
        public static Assembly Assembly
        {
            get
            {
                return _cachedAssembly ?? (_cachedAssembly = Assembly.GetExecutingAssembly());
            }
        }

        /// <summary>
        /// Version de l'assembly pure
        /// </summary>
        public static Version Version
        {
            get
            {
                return _cachedVersion ?? (_cachedVersion = Assembly.GetName().Version);
            }
        }

        /// <summary>
        /// Hash de version Git/Hg ou numéro de révision SVN
        /// </summary>
        public static string InformalVersion
        {
            get
            {
                if (_cachedVersionHash == null)
                {
                    _cachedVersionHash = String.Empty;

                    var attrs = Assembly
                        .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);

                    if (attrs.Length > 0)
                    {
                        _cachedVersionHash =
                            ((AssemblyInformationalVersionAttribute)attrs[0]).InformationalVersion;
                    }
                }

                return _cachedVersionHash;
            }
        }

        /// <summary>
        /// Permet de récuperer un numéro de version simple sous forme d'int.
        /// <remarks>V-1 n'est pas forcement > VCurrent !</remarks>
        /// </summary>
        public static int SimpleVersionToken
        {
            get
            {
                return Math.Abs(Version.GetHashCode());
            }
        }
    }
}
