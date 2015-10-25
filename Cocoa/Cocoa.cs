using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
