using System.Management.Automation;

namespace Knom.TimeBox.Powershell
{
    [Cmdlet(VerbsCommunications.Connect, "TimeBox", SupportsShouldProcess = true)]
    public class ConnectTimeBox : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var device = TimeBoxContext.Instance.Device;

            if (!device.IsConnected)
            {
                device.Connect();
                WriteObject(new
                {
                    Status = "Connected"
                });
            }
            else
            {
                WriteWarning("Already connected to TimeBox device!");
                WriteObject(new
                {
                    Status = "Connected"
                });
            }
        }
    }
}