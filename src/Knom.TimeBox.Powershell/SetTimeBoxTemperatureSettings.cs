using System.Drawing;
using System.Management.Automation;

namespace Knom.TimeBox.Powershell
{
    [Cmdlet(VerbsCommon.Set, "TimeBoxTemperatureSettings", SupportsShouldProcess = true)]
    public class SetTimeBoxTemperatureSettings : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public Color Color { get; set; }

        [Parameter(Mandatory = true)]
        public TemperatureMode Mode { get; set; }


        protected override void ProcessRecord()
        {
            var device = TimeBoxContext.Instance.Device;

            device.SetTempUnitAndColor(Color, Mode);
        }
    }
}