using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa
{
    static class Cocoa
    {
        public static void Version()
        {
            Console.WriteLine(
                @"Cocoa v" +
                Assembly.GetExecutingAssembly().GetName().Version.ToString());

            using (var choco = new ChocolateyHost())
            {
                choco.Exec();
            }
        }

        public static void Help()
        {
            using (var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Cocoa.Help.txt")))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }

        public static void Install(IEnumerable<string> packages)
        {
            var sb = new StringBuilder();
            sb.Append(@"install");
            packages.ToList().ForEach(x => sb.Append(@" " + x));

            using (var choco = new ChocolateyHost())
            {
                choco.ExecInteractive(sb.ToString());
            }
        }

        public static void Uninstall(IEnumerable<string> packages)
        {
            var sb = new StringBuilder();
            sb.Append(@"uninstall");
            packages.ToList().ForEach(x => sb.Append(@" " + x));

            using (var choco = new ChocolateyHost())
            {
                choco.ExecInteractive(sb.ToString());
            }
        }

        public static void Upgrade(IEnumerable<string> packages)
        {
            var sb = new StringBuilder();
            sb.Append(@"upgrade");
            packages.ToList().ForEach(x => sb.Append(@" " + x));

            using (var choco = new ChocolateyHost())
            {
                choco.ExecInteractive(sb.ToString());
            }
        }

        public static void ShowPackagesList(IEnumerable<string> packages, bool isLocalPackages = false)
        {
            var sb = new StringBuilder();
            sb.Append(@"list");
            packages.ToList().ForEach(x => sb.Append(@" " + x));
            if (isLocalPackages)
            {
                sb.Append(@" --localonly");
            }

            using (var choco = new ChocolateyHost())
            {
                choco.ExecInteractive(sb.ToString());
            }
        }

        public static void ShowNonApprovedPackagesList(IEnumerable<string> packages)
        {
            var count = 0;
            packages.ToList().ForEach(x => 
            {
                var packageInfo = Crawler.GetPackageInfos(x);
                if (packageInfo != null && packageInfo.Count() != 0)
                {
                    Console.WriteLine(packageInfo.ToList()[0].ToString());
                    count++;
                }
            });

            Console.WriteLine(count + @" packages found.");
        }

        public static async Task InstallNonApprovedPackages(IEnumerable<string> packages)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), @"cocoa");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            var availablePackages = new List<string>();

            foreach(var package in packages)
            {
                if (package.StartsWith(@"-"))
                {
                    continue;
                }

                var packageInfos = Crawler.GetPackageInfos(package);
                if (packageInfos == null || !packageInfos.Any())
                {
                    Console.WriteLine(@"'" + package + @"' was not found.");
                    continue;
                }

                availablePackages.Add(package);

                var packageInfo = packageInfos.ToList()[0];
                string fileName;
                string filePath;
                using (var hc = new HttpClient())
                {
                    var res = hc.GetAsync(packageInfo.DownloadUri, HttpCompletionOption.ResponseHeadersRead).Result;
                    fileName = Path.GetFileName(res.RequestMessage.RequestUri.ToString());
                    filePath = Path.Combine(tempDir, fileName);

                    using (var fs = File.Create(filePath))
                    {
                        using (var s = res.Content.ReadAsStreamAsync().Result)
                        {
                            s.CopyTo(fs);
                            fs.Flush();
                        }
                    }
                }
            }

            if (availablePackages.Count != 0)
            {
                var sb = new StringBuilder();
                sb.Append(@"install");
                availablePackages.ForEach(x => sb.Append(@" " + x));
                sb.Append(@" -source " + tempDir);

                using (var choco = new ChocolateyHost())
                {
                    choco.ExecInteractive(sb.ToString());
                }
            }
        }
    }
}
