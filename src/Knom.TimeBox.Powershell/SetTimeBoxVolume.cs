using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace Knom.TimeBox.Powershell
{
    [Cmdlet(VerbsCommon.Set, "TimeBoxVolume", SupportsShouldProcess = true)]
    public class SetTimeBoxVolume : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0),
            ValidateRange(0,16)]
        public int Level { get; set; }

        protected override void ProcessRecord()
        {
            var device = TimeBoxContext.Instance.Device;
            device.SetVolume(Level);
        }
    }
}