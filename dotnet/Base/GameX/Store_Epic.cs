﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GameX.Store
{
    /// <summary>
    /// Store_Epic
    /// </summary>
    internal static class Store_Epic
    {
        internal static readonly Dictionary<string, string> EpicPaths = [];

        static Store_Epic()
        {
            // get dbPath
            var root = GetPath();
            if (root == null) return;
            var dbPath = Path.Combine(root, "Manifests");
            if (!File.Exists(dbPath)) return;

            // # query games
            foreach (var s in Directory.EnumerateFiles(dbPath).Where(s => s.EndsWith(".item")))
            {
                // add appPath if exists
                var appPath = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(s)).GetProperty("InstallLocation").GetString();
                if (Directory.Exists(appPath)) EpicPaths.Add(Path.GetFileNameWithoutExtension(s), appPath);
            }
        }

        static string GetPath()
        {
            IEnumerable<string> paths;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // windows paths
                var home = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string[] search = [@"Epic\EpicGamesLauncher"];
                paths = search.Select(path => Path.Join(home, path, "Data"));
            }
            else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // linux paths
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string[] search = ["Epic/EpicGamesLauncher"];
                paths = search.Select(path => Path.Join(home, path, "Data"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // mac paths
                var home = "/Users/Shared";
                string[] search = ["Epic/EpicGamesLauncher"];
                paths = search.Select(path => Path.Join(home, path, "Data"));
            }
            else throw new PlatformNotSupportedException();
            return paths.FirstOrDefault(Directory.Exists);
        }
    }
}

