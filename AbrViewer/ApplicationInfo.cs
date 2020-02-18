using System;
using System.Reflection;

namespace AbrViewer
{
    /// <summary>
    /// This class provides information about the running application.
    /// </summary>
    public static class ApplicationInfo
    {
        private static string productName;
        private static bool productNameCached;
        private static string version;
        private static bool versionCached;
        private static string company;
        private static bool companyCached;
        private static string copyright;
        private static bool copyrightCached;


        /// <summary>
        /// Gets the product name of the application.
        /// </summary>
        public static string ProductName {
            get {
                if (!productNameCached) {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly != null) {
                        AssemblyProductAttribute attribute = ((AssemblyProductAttribute) Attribute.GetCustomAttribute(
                            entryAssembly, typeof(AssemblyProductAttribute)
                        ));
                        productName = (attribute != null) ? attribute.Product : "";
                    }
                    else {
                        productName = "";
                    }
                    productNameCached = true;
                }
                return productName;
            }
        }

        /// <summary>
        /// Gets the version number of the application.
        /// </summary>
        public static string Version {
            get {
                if (!versionCached) {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly != null) {
                        version = entryAssembly.GetName().Version.ToString();
                    }
                    else {
                        version = "";
                    }
                    versionCached = true;
                }
                return version;
            }
        }

        /// <summary>
        /// Gets the company of the application.
        /// </summary>
        public static string Company {
            get {
                if (!companyCached) {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly != null) {
                        AssemblyCompanyAttribute attribute = ((AssemblyCompanyAttribute) Attribute.GetCustomAttribute(
                            entryAssembly, typeof(AssemblyCompanyAttribute)
                        ));
                        company = (attribute != null) ? attribute.Company : "";
                    }
                    else {
                        company = "";
                    }
                    companyCached = true;
                }
                return company;
            }
        }

        /// <summary>
        /// Gets the copyright information of the application.
        /// </summary>
        public static string Copyright {
            get {
                if (!copyrightCached) {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly != null) {
                        AssemblyCopyrightAttribute attribute =
                                (AssemblyCopyrightAttribute) Attribute.GetCustomAttribute(
                                    entryAssembly, typeof(AssemblyCopyrightAttribute)
                                );
                        copyright = attribute != null ? attribute.Copyright : "";
                    }
                    else {
                        copyright = "";
                    }
                    copyrightCached = true;
                }
                return copyright;
            }
        }
    }
}
