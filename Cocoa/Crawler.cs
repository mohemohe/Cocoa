using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cocoa
{
    class Crawler
    {
        const string ChocolateyRoot = @"https://chocolatey.org/";
        const string ChocolateyPackage = @"https://chocolatey.org/packages/";
        const string ChocolateyApi = @"https://chocolatey.org/api/v2/package/";

        public static IEnumerable<PackageInfo> GetPackageInfos(string packageName)
        {
            var packagePage = GetPackagePagesHTML(packageName);
            if (packagePage == null)
            {
                return null;
            }

            var result = new List<PackageInfo>();

            var minifiedHtml = packagePage.Replace("\r", @"").Replace("\n", @"").Replace(@"  ", @"");

            var rootRegex = new Regex(Regex.Escape(@"<table") + @".*" + Regex.Escape(@">") + @".*" + Regex.Escape(@"</table>"));
            var rootMatch = rootRegex.Match(minifiedHtml);
            if (!rootMatch.Success)
            {
                return null;
            }

            var formattedHtml = rootMatch.Value.Replace(@"&nbsp;", @" ");
            var xDoc = XDocument.Load(new StringReader(formattedHtml), LoadOptions.PreserveWhitespace);
            var packagesTableElements = xDoc.Element(@"table").Element(@"tbody").Elements(@"tr");

            foreach (var element in packagesTableElements)
            {
                var nodes = element.Elements().ToList();
                var nameBase = ((XElement)nodes[0].FirstNode).FirstNode.ToString();
                var version = nameBase.Split(' ').Last();
                var name = nameBase.Remove(nameBase.Length - version.Length - 1);
                string packagePageUri;
                try
                {
                    packagePageUri = new Uri(
                        new Uri(ChocolateyRoot),
                        ((XElement)nodes[0].FirstNode).FirstAttribute.Value.Replace(@"href=", @"").Replace("\"", @"")).ToString();
                }
                catch
                {
                    packagePageUri = ChocolateyPackage + name;
                }
                var isApproved = nodes[3].Value.ToLower() == @"approved";

                result.Add(new PackageInfo
                {
                    Name = name,
                    Version = version,
                    PackagePageUri = packagePageUri,
                    IsApproved = isApproved,
                });
            }

            return result;
        }

        public static string GetPackageDownloadUrl(PackageInfo packageInfo)
        {
            return new Uri(new Uri(ChocolateyApi), packageInfo.Name + @"/" + packageInfo.Version).ToString();
        }

        private static string GetPackagePagesHTML(string packageName, string version = null)
        {
            var uri = version == null ?
                ChocolateyPackage + packageName :
                ChocolateyPackage + packageName  + @"/" + version;
            using (var wc = new WebClient { Timeout = 3000 })
            {
                try
                {
                    return wc.DownloadString(uri);
                }
                catch (WebException ex)
                {
                    return null;
                }
            }
        }
    }
}
