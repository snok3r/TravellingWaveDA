using System;
using System.ComponentModel;

namespace TW.Model
{
    public abstract class AbstractFHN
    {
        protected const int POINTS_THRESHOLD = 10;

        protected double hx, ht; // steps
        protected double[] t; // time

        // variables for properties
        protected double varL;
        private int varN;
        private double varA;
        private double varT;

        public abstract void Allocate();
        public abstract void Initials();
        public abstract void InitialsFurther();
        public abstract bool Solve();
        public virtual void Reload() { }

        public AbstractFHN()
        {
            a = 0.1;
            Classical = false;
        }

        // properties
        public int N
        {   // quantity of u,v t's (x's)
            get { return varN; }
            set
            {
                if (value > POINTS_THRESHOLD) varN = value;
            }
        }

        public virtual double L
        {
            get { return varL; }
            set
            {
                if (value > 0) varL = value;
            }
        }

        [Description("Interval for t [0, T]")]
        public double T
        {   // bound t's segment
            get { return varT; }
            set
            {
                if (value > 0) varT = value;
            }
        }

        [Description("F's constant in non-classical non-linearity (classical == false)")]
        public double a
        {   // f's constant if non-classical
            get { return varA; }
            set
            {
                if (value > 0 && value < 1) varA = value;
            }
        }

        [Description("Whether this is a classical f(u) or not")]
        public bool Classical { get; set; }

        public double GetT(int j)
        { return t[j]; }

        public double GetHx()
        { return hx; }

        public double GetHt()
        { return hx; }


        public double F(double u)
        {
            if (Classical)
                return u - u * u * u / 3;
            else
                return - u * (u - 1) * (u - a);
        }

        public virtual void Initials(String UX0) { }

        public virtual void Dispose()
        { t = null; }
    }
}
