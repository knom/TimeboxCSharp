using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Knom.TimeBox.Powershell
{
    [Cmdlet(VerbsCommon.Set, "TimeBoxTime", SupportsShouldProcess = true)]
    public class SetTimeBoxTime : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        public DateTime? DateTime { get; set; }

        protected override void ProcessRecord()
        {
            if (!DateTime.HasValue)
                this.DateTime = System.DateTime.Now;

            var device = TimeBoxContext.Instance.Device;
            device.SetTime(DateTime.Value);

        }
    }
}
