using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace Knom.TimeBox.Powershell
{
    [Cmdlet(VerbsCommon.Show, "TimeBoxImageAnimation", SupportsShouldProcess = true)]
    public class ShowTimeBoxImageAnimation : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public string[] AnimationImagePaths { get; set; }

        [Parameter(Mandatory = false)]
        public int Delay { get; set; } = 0;

        protected override void ProcessRecord()
        {
            TimeBoxDevice device = TimeBoxContext.Instance.Device;

            List<string> paths = GetResolvedProviderPathFromPSPaths(this.AnimationImagePaths, typeof(FileSystemProvider)).ToList();

            string dir = paths.FirstOrDefault(p => File.GetAttributes(p).HasFlag(FileAttributes.Directory));
            if (dir != null)
            {
                device.AnimateImages(dir, Delay);

                var files = Directory.GetFiles(dir, "*.png");
                WriteObject(files);
            }
            else
            {
                IEnumerable<string> files = paths.Where(p => !File.GetAttributes(p).HasFlag(FileAttributes.Directory));
                device.AnimateImages(files, Delay);

                WriteObject(files);
            }
        }

        private IEnumerable<string> GetResolvedProviderPathFromPSPaths(string[] paths, Type type)
        {
            IEnumerable<string> result = paths.SelectMany(p =>
            {
                IEnumerable<string> path = this.GetResolvedProviderPathFromPSPath(p, out ProviderInfo provider);
                if (provider.ImplementingType == type)
                    return path;
                else
                    return null;
            });

            return result;
        }
    }
}