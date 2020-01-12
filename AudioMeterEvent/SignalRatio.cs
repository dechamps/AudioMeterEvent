namespace AudioMeterEvent
{
    public struct SignalRatio
    {
        public double Factor { get; set; }

        // https://en.wikipedia.org/wiki/Decibel#Field_quantities_and_root-power_quantities
        public double FieldDecibels
        {
            get
            {
                
                return 20 * System.Math.Log10(Factor);
            }
            set
            {
                Factor = System.Math.Pow(10, value / 20);
            }
        }

        public override string ToString()
        {
            return FieldDecibels + " dB";
        }

        static public bool operator ==(SignalRatio lhs, SignalRatio rhs)
        {
            return lhs.Factor == rhs.Factor;
        }
        static public bool operator !=(SignalRatio lhs, SignalRatio rhs)
        {
            return lhs.Factor != rhs.Factor;
        }
        static public bool operator <(SignalRatio lhs, SignalRatio rhs)
        {
            return lhs.Factor < rhs.Factor;
        }
        static public bool operator >(SignalRatio lhs, SignalRatio rhs)
        {
            return lhs.Factor > rhs.Factor;
        }
        static public bool operator <=(SignalRatio lhs, SignalRatio rhs)
        {
            return lhs.Factor <= rhs.Factor;
        }
        static public bool operator >=(SignalRatio lhs, SignalRatio rhs)
        {
            return lhs.Factor >= rhs.Factor;
        }
        public override bool Equals(object obj)
        {
            return obj is SignalRatio signalRatio && signalRatio == this;
        }
        public override int GetHashCode()
        {
            return Factor.GetHashCode();
        }
    }
}