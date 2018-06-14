using System.IO;
using System.Management.Automation;

namespace Knom.TimeBox.Powershell
{
    [Cmdlet(VerbsCommon.Show, "TimeBoxView", SupportsShouldProcess = true)]
    public class ShowTimeBoxView: PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public TimeBoxDevice.ViewType ViewType { get; set; }

        protected override void ProcessRecord()
        {
            var device = TimeBoxContext.Instance.Device;
            device.SetView(ViewType);
        }
    }
}