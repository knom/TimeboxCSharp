using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using InTheHand.Net;
using InTheHand.Net.Sockets;

namespace Knom.TimeBox
{
    public class TimeBoxDevice
    {
        private BluetoothClient _client;

        public enum ViewType
        {
            Clock = 0x00,
            Temp = 0x01,
            Off = 0x02,
            Anim = 0x03,
            Graph = 0x04,
            Image = 0x05,
            Stopwatch = 0x06,
            Scoreboard = 0x07
        }

        public TimeBoxDevice()
        {
        }

        public void Connect()
        {
            byte[] TIMEBOX_HELLO = new byte[] { 0, 5, 72, 69, 76, 76, 79, 0 };

            _client = new BluetoothClient();
            var info = _client.DiscoverDevices();

            //BluetoothService.RFCommProtocol
            var device = info.Single(p => p.DeviceName == "TimeBox-mini-audio");
            _client.Connect(new BluetoothEndPoint(device.DeviceAddress, Guid.Empty,
                4));
            var stream = _client.GetStream();

            byte[] buffer = new byte[256];
            stream.Read(buffer, 0, 256);

            if (!buffer.Take(8).SequenceEqual(TIMEBOX_HELLO))
            {
                throw new InvalidOperationException("Invalid HELLO sequence from TimeBox!");
            }
        }

        public void Disconnect()
        {
            _client.Close();
        }

        public void SetView(ViewType type)
        {
            var payload = new Byte[] { 0x04, 0x00, 0x45, (byte)type };

            var message = BuildMessage(payload);
            _client.GetStream().Write(message, 0, message.Length);
        }

        public void SetTime(DateTime dt)
        {
            var payload = new Byte[]
            {0x0A, 0x00, 0x18,
                (byte)(dt.Year % 100),
                (byte)(dt.Year / 100),
                (byte)dt.Month, (byte)dt.Day, (byte)dt.Hour, (byte)dt.Minute, (byte)dt.Second};


            var message = BuildMessage(payload);
            _client.GetStream().Write(message, 0, message.Length);
        }

        public void SetTimeColor(Color color, bool h24 = true)
        {
#warning Verify if it works?
            byte h24b = h24 ? (byte)0x01 : (byte)0x00;
            var payload = new byte[] { 0x09, 0x00, 0x45, 0x00, h24b, color.R, color.G, color.B, 0x00 };

            var message = BuildMessage(payload);
            _client.GetStream().Write(message, 0, message.Length);
        }

        public void SetVolume([Range(0, 16)]int level)
        {
#warning Verify if MASK is right?
            var payload = new Byte[] { 0x04, 0x00, 0x08, (byte)level };

            var message = BuildMessage(payload);
            _client.GetStream().Write(message, 0, message.Length);
        }

        public void SetTempUnit(bool fahrenheit = false)
        {
#warning Verify if it works?
            byte f = fahrenheit ? (byte)0x01 : (byte)0x00;
            var payload = new byte[] { 0x09, 0x00, 0x45, 0x01, f };

            var message = BuildMessage(payload);
            _client.GetStream().Write(message, 0, message.Length);
        }

        public void SetTempUnitAndColor(Color color, bool fahrenheit = false)
        {
#warning Verify if it works?
            byte f = fahrenheit ? (byte)0x01 : (byte)0x00;
            var payload = new byte[] { 0x09, 0x00, 0x45, 0x01, f, color.R, color.G, color.B, 0x00 };

            var message = BuildMessage(payload);
            _client.GetStream().Write(message, 0, message.Length);
        }


        private static byte[] BuildMessage(byte[] payload)
        {
            var checksum = Checksum(payload);

            var buffer = new List<byte>();
            buffer.Add(0x01);
            buffer.AddRange(Mask(payload));
            buffer.AddRange(Mask(new[] { checksum.Item1, checksum.Item2 }));
            buffer.Add(0x02);

            return buffer.ToArray();
        }

        private static byte[] Mask(byte[] bytes)
        {
            var result = new List<byte>();

            foreach (byte b in bytes)
            {
                if (b == 0x01)
                    result.AddRange(new Byte[] { 0x03, 0x04 });
                else if (b == 0x02)
                    result.AddRange(new Byte[] { 0x03, 0x05 });
                else if (b == 0x03)
                    result.AddRange(new Byte[] { 0x03, 0x06 });
                else
                    result.Add(b);
            }

            return result.ToArray();
        }

        private static Tuple<byte, byte> Checksum(byte[] bytes)
        {
            var p = bytes.Sum(pp => (int)pp);

            byte ck1 = (byte)(p & 0x00ff);
            byte ck2 = (byte)(p >> 8);

            return new Tuple<byte, byte>(ck1, ck2);
        }
    }
}