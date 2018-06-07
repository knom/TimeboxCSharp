using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InTheHand.Net.Bluetooth;

namespace Knom.TimeBox
{
    class Program
    {
        static void Main(string[] args)
        {
            TimeBoxDevice box = new TimeBoxDevice();

            box.Connect();
            box.SetView(TimeBoxDevice.ViewType.Clock);
            box.SetTimeColor(Color.Green);
            box.SetTime(DateTime.Now);

            box.SetTempUnit(fahrenheit: false);
            box.SetTempUnitAndColor(Color.Orange, fahrenheit: true);

            box.SetVolume(0);
            box.SetVolume(16);

            Thread.Sleep(Int32.MaxValue);
        }


    }
}