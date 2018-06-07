using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Knom.TimeBox;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TimeBoxDevice box = new TimeBoxDevice();

            box.Connect();
            box.SetView(TimeBoxDevice.ViewType.Clock);
            box.SetTimeColor(Color.DarkOrchid);
            box.SetTime(DateTime.Now);

            box.SetTempUnit(fahrenheit: false);
            box.SetTempUnitAndColor(Color.Orange, fahrenheit: false);

            box.SetVolume(0);
            box.SetVolume(16);

            Thread.Sleep(Int32.MaxValue);
        }


    }
}