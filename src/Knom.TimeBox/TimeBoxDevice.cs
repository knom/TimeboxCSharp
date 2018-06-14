using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using InTheHand.Net;
using InTheHand.Net.Sockets;

namespace Knom.TimeBox
{
    public class TimeBoxDevice
    {
        private BluetoothClient _client;

        public bool IsConnected => _client != null && _client.Connected;

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
        
        public void Connect()
        {
            byte[] TIMEBOX_HELLO = new byte[] { 0, 5, 72, 69, 76, 76, 79, 0 };

            _client = new BluetoothClient();
            var info = _client.DiscoverDevices();

            //BluetoothService.RFCommProtocol
            var device = info.FirstOrDefault(p => p.DeviceName == "TimeBox-mini-audio"
                                            && p.Connected);
            if (device == null)
            {
                throw new TimeBoxDeviceNotFoundException("Device not found!");
            }
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

        #region Byte Helpers

        public static byte[] ConvertImage(string path)
        {
            var imageBytes = new List<byte>();
            imageBytes.Add(0);

            int counter = 0;

            bool first = true;
            using (var image = (Bitmap)Image.FromFile(path))
            {
                for (int y = 0; y < image.Size.Width; y++)
                {
                    for (int x = 0; x < image.Size.Height; x++)
                    {
                        var color = image.GetPixel(y, x);
                        //if (first):
                        //  img[-1] = ((r & 0xf0) >> 4) + (g & 0xf0) if a > 32 else 0
                        //  img.append((b & 0xf0) >> 4) if a > 32 else img.append(0)
                        //  first = False
                        //else:
                        //  img[-1] += (r & 0xf0) if a > 32 else 0
                        //  img.append(((g & 0xf0) >> 4) + (b & 0xf0)) if a > 32 else img.append(0)
                        //  img.append(0)
                        //  first = True
                        if (first)
                        {
#warning ALPHA?
                            int idx = imageBytes.Count - 1;
                            imageBytes[idx] = (byte)(((color.R & 0xf0) >> 4) + (color.G & 0xf0));
                            imageBytes.Add((byte)((color.B & 0xf0) >> 4));
                            first = false;
                        }
                        else
                        {
                            int idx = imageBytes.Count - 1;
                            imageBytes[idx] = (byte)(imageBytes[idx] + (byte)(color.R & 0xf0));
                            imageBytes.Add((byte)(((color.G & 0xf0) >> 4) + (color.B & 0xf0)));
                            imageBytes.Add(0);
                            first = true;
                        }
                    }
                }
                return imageBytes.ToArray();
            }
        }
        private static byte[] BuildMessage(byte[] payload)
        {
            // build the checksum
            var checksum = Checksum(payload);

            var buffer = new List<byte>();
            // 0x01 first (begin of message)
            buffer.Add(0x01);

            // concat the message with the checksum
            var completeMessage = payload.Append(new[] { checksum.Item1, checksum.Item2 });

            // escape the message and add
            buffer.AddRange(Escape(completeMessage));

            // 0x02 last (end of message)
            buffer.Add(0x02);

            return buffer.ToArray();
        }
        private static byte[] Escape(byte[] bytes)
        {
            // Escape the payload. 
            // It is not allowed to have occurrences of the codes 0x01, 0x02 and 0x03.
            // They must be escaped by a leading 0x03 followed by 0x04, 0x05 or 0x06 instead
            List<byte> result = new List<byte>();

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
            //  Compute the checksum.
            var p = bytes.Sum(pp => (int)pp);

            byte ck1 = (byte)(p & 0x00ff);
            byte ck2 = (byte)(p >> 8);

            return new Tuple<byte, byte>(ck1, ck2);
        }

        #endregion
        public void ShowImage(string path)
        {
            var header = new byte[] { 0xbd, 0x00, 0x44, 0x00, 0x0a, 0x0a, 0x04 };

            var imageData = ConvertImage(path);

            var payload = header.Append(imageData);

            var message = BuildMessage(payload);

            _client.GetStream().Write(message, 0, message.Length);

            //data = data
            //ck1, ck2 = checksum(sum(head) + sum(data))

            //msg = [0x01] + head + mask(data) + mask([ck1, ck2]) + [0x02]
            //return msg
        }


        public void AnimateImages(IEnumerable<string> images, int delay = 0)
        {
            var result = new Byte[0];

            var header = new Byte[] { 0xbf, 0x00, 0x49, 0x00, 0x0a, 0x0a, 0x04 };

            int fi = 0;
            foreach (var file in images)
            {
                byte[] imageData = ConvertImage(file);
                byte[] payload = header
                    .Append(new byte[] { (byte)fi, (byte)delay })
                    .Append(imageData);

                fi++;
                byte[] message = BuildMessage(payload);

                result = result.Append(message);
            }

            _client.GetStream().Write(result, 0, result.Length);
        }
        public void AnimateImages(string folderPath, int delay = 0)
        {
            this.AnimateImages(Directory.GetFiles(folderPath, "*.png"), delay);
        }
    }
}