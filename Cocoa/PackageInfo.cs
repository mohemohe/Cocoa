using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa
{
    class PackageInfo
    {
        private const string ChocolateyApi = @"https://chocolatey.org/api/v2/package/";

        public string Name { get; set; }
        public string Version { get; set; }
        public string PackagePageUri { get; set; }
        public bool IsApproved { get; set; }

        public string DownloadUri
        {
            get
            {
                return new Uri(new Uri(ChocolateyApi), Name + @"/" + Version).ToString();
            }
        }

        public override string ToString()
        {
            return Name + @" " + Version;
        }
    }
}
