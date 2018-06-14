namespace Knom.TimeBox.Powershell
{
    internal class TimeBoxContext
    {
        static TimeBoxContext()
        {
            Instance = new TimeBoxContext();
        }

        public TimeBoxContext()
        {
            this.Device = new TimeBoxDevice();
        }

        public static TimeBoxContext Instance { get; private set; }
        public TimeBoxDevice Device { get; set; }
}
}