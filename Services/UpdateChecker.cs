using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NomisKitchenHDT.Services
{
    public class UpdateChecker
    {
        const string LatestApi = "https://api.github.com/repos/RainWritesCode/NomisKitchenHDT/releases/latest";

        public bool UpdateAvailable { get; private set; }
        public string LatestVersion { get; private set; }
        public string DownloadUrl { get; private set; }
        public string ReleaseUrl { get; private set; }

        readonly Version _current;

        public UpdateChecker(Version current)
        {
            _current = current;
        }

        WebClient MakeClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var wc = new WebClient();
            wc.Headers.Add("User-Agent", "NomisKitchenHDT-UpdateCheck");
            return wc;
        }

        public async Task<bool> CheckAsync()
        {
            try
            {
                string json;
                using (var wc = MakeClient())
                    json = await wc.DownloadStringTaskAsync(LatestApi);

                var tag = Match(json, "\"tag_name\"\\s*:\\s*\"([^\"]+)\"");
                ReleaseUrl = Match(json, "\"html_url\"\\s*:\\s*\"([^\"]+)\"");
                DownloadUrl = Match(json, "\"browser_download_url\"\\s*:\\s*\"([^\"]+\\.exe)\"");

                if (string.IsNullOrEmpty(tag)) return false;
                LatestVersion = tag.TrimStart('v', 'V');
                if (Version.TryParse(LatestVersion, out var latest))
                    UpdateAvailable = latest > _current;

                return UpdateAvailable;
            }
            catch { return false; }
        }

        public async Task<bool> DownloadAndRunAsync()
        {
            if (string.IsNullOrEmpty(DownloadUrl)) return false;
            try
            {
                var tmp = Path.Combine(Path.GetTempPath(), "NomisKitchenSetup-update.exe");
                using (var wc = MakeClient())
                    await wc.DownloadFileTaskAsync(DownloadUrl, tmp);
                Process.Start(tmp);
                return true;
            }
            catch { return false; }
        }

        static string Match(string s, string pattern)
        {
            var m = Regex.Match(s, pattern);
            return m.Success ? m.Groups[1].Value : null;
        }
    }
}
