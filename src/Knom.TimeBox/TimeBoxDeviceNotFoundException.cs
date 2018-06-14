using System;

namespace Knom.TimeBox
{
    public class TimeBoxDeviceNotFoundException : Exception
    {
        public TimeBoxDeviceNotFoundException(string message):base(message)
        {
        }
    }
}