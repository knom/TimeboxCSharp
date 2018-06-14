using System;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace Knom.TimeBox.Powershell
{
    [Cmdlet(VerbsCommon.Show, "TimeBoxImage", SupportsShouldProcess = true)]
    public class ShowTimeBoxImage : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public string ImagePath { get; set; }

        protected override void ProcessRecord()
        {
            var files = this.GetResolvedProviderPathFromPSPath(ImagePath, out ProviderInfo provider);

            var device = TimeBoxContext.Instance.Device;

            if (provider.ImplementingType != typeof(FileSystemProvider))
                throw new ArgumentException("ImagePath should be a valid file system path!");
            
            if (files.Count > 0)
                device.ShowImage(files[0]);
            else
                throw new ItemNotFoundException($"Cannot find path '{ImagePath}' because it does not exist.");
        }
    }
}