using System;
using System.Drawing;
using System.Management.Automation;

namespace Knom.TimeBox.Powershell
{
    [Cmdlet(VerbsCommon.Set, "TimeBoxTimeSettings", SupportsShouldProcess = true)]
    public class SetTimeBoxTimeSettings : PSCmdlet
    {
        [Parameter(Mandatory = false, HelpMessage = "Display time in 24 or 12 hours format.")]
        public bool Show24Format { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "The color of the time, use valid color codes.")]
        public Color Color { get; set; }

        protected override void ProcessRecord()
        {
            var device = TimeBoxContext.Instance.Device;
            device.SetTimeColor(this.Color, this.Show24Format);
        }
    }
}