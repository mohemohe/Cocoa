using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa
{
    static class Program
    {
        static bool IsShowVersion;
        static bool IsShowHelp;
        static bool IsShowPackagesList;
        static bool IsShowNonApprovedPackagesList;
        static bool IsShowLocalPackagesList;
        static List<string> ListPackages = new List<string>();
        static bool IsInstallNonApprovedPackages;
        static List<string> InstallPackages = new List<string>();
        static List<string> UninstallPackages = new List<string>();
        static List<string> UpgradePackages = new List<string>();
        static bool IsNoConfirm;
        static bool IsForce;
        static bool IsInvalidOperation;

        static void Main(string[] args)
        {
            if (args.Length == 0)
                Cocoa.Version();

            ParseArgs(args);
            if (!IsInvalidOperation)
            {
                Task.Run(() => ExecCocoa()).Wait();
            }
        }

        static void ParseArgs(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i].Replace('/', '-');

                switch (arg)
                {
                    case @"--version":
                        IsShowVersion = true;
                        break;

                    case @"--help":
                        IsShowHelp = true;
                        break;

                    case @"-S":
                    case @"install":
                        for (i++; i < args.Length && !args[i].StartsWith("-"); i++)
                        {
                            InstallPackages.Add(args[i]);
                        }
                        i--;
                        break;

                    case @"-Sa":
                        IsInstallNonApprovedPackages = true;
                        for (i++; i < args.Length && !args[i].StartsWith("-"); i++)
                        {
                            InstallPackages.Add(args[i]);
                        }
                        i--;
                        break;

                    case @"-R":
                    case @"uninstall":
                        for (i++; i < args.Length && !args[i].StartsWith("-"); i++)
                        {
                            UninstallPackages.Add(args[i]);
                        }
                        i--;
                        break;

                    case @"-Su":
                    case @"upgrade":
                        for (i++; i < args.Length && !args[i].StartsWith("-"); i++)
                        {
                            UpgradePackages.Add(args[i]);
                        }
                        if(UpgradePackages.Count == 0)
                        {
                            UpgradePackages.Add(@"all");
                        }
                        i--;
                        break;

                    case @"-Ss":
                    case @"-Sq":
                    case @"list":
                        IsShowPackagesList = true;
                        if(arg == @"-Sq")
                        {
                            IsShowLocalPackagesList = true;
                        }
                        for (i++; i < args.Length && !args[i].StartsWith("-"); i++)
                        {
                            ListPackages.Add(args[i]);
                        }
                        i--;
                        break;

                    case @"-Ssa":
                        IsShowPackagesList = true;
                        IsShowNonApprovedPackagesList = true;
                        for (i++; i < args.Length && !args[i].StartsWith("-"); i++)
                        {
                            ListPackages.Add(args[i]);
                        }
                        i--;
                        break;

                    case @"--nonapproved":
                        IsShowNonApprovedPackagesList = true;
                        IsInstallNonApprovedPackages = true;
                        break;

                    case @"--localonly":
                        IsShowLocalPackagesList = true;
                        break;

                    case @"-y":
                    case @"--noconfirm":
                        IsNoConfirm = true;
                        break;

                    case @"--force":
                        IsForce = true;
                        break;

                    default:
                        IsInvalidOperation = true;
                        Console.WriteLine(@"Invalid argument '" + arg + @"'.");
                        break;
                }
            }
        }

        static void ExecCocoa()
        {
            if (UpgradePackages.Count != 0)
            {
                if (IsNoConfirm)
                {
                    UpgradePackages.Add(@"-y");
                }
                if (IsForce)
                {
                    UpgradePackages.Add(@"--force");
                }
                Cocoa.Upgrade(UpgradePackages);
            }

            if (InstallPackages.Count != 0)
            {
                if (IsNoConfirm)
                {
                    InstallPackages.Add(@"-y");
                }
                if (IsForce)
                {
                    InstallPackages.Add(@"--force");
                }
                if (IsInstallNonApprovedPackages)
                {
                    Cocoa.InstallNonApprovedPackages(InstallPackages).Wait();
                }
                else
                {
                    Cocoa.Install(InstallPackages);
                }
            }

            if (UninstallPackages.Count != 0)
            {
                if (IsNoConfirm)
                {
                    UninstallPackages.Add(@"-y");
                }
                if (IsForce)
                {
                    UninstallPackages.Add(@"--force");
                }
                Cocoa.Uninstall(UninstallPackages);
            }

            if (IsShowPackagesList)
            {
                if (IsShowNonApprovedPackagesList)
                {
                    Cocoa.ShowNonApprovedPackagesList(ListPackages);
                }
                else
                {
                    Cocoa.ShowPackagesList(ListPackages, IsShowLocalPackagesList);
                }
            }

            if (IsShowVersion)
            {
                Cocoa.Version();
            }

            if (IsShowHelp)
            {
                Cocoa.Help();
            }
        }
    }
}
