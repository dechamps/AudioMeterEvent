namespace AudioMeterEvent
{
    public sealed class SignalRatio
    {
        public double Factor { get; set; }

        public double FieldDecibels
        {
            get
            {
                // https://en.wikipedia.org/wiki/Decibel#Field_quantities_and_root-power_quantities
                return 20 * System.Math.Log10(Factor);
            }
        }

        public override string ToString()
        {
            return FieldDecibels + " dB";
        }
    }
}