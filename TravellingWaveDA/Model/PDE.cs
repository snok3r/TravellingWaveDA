using MathParser;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace TW.Model
{
    public class PDE : AbstractFHN
    {
        // variables and arrays
        private double[] x;
        private double[,] u;
        private PDEProperties properties;

        private int varM;
        private double varD;
        private double varDiff;

        // Constructor with default values
        public PDE()
            : base()
        {
            N = 1000;
            M = 2000;
            L = 50.0;
            T = 100.0;
            b = 1.0;
            d = 1.0;
            D = 1.0;
            DeltaCoupling = true;

            properties = new PDEProperties(this);
        }

        // properties
        public int M
        {   // quantity of u,v t's
            get { return varM; }
            set
            {
                if (value > POINTS_THRESHOLD) varM = value;
            }
        }

        [Description("Interval for x [-L, L]")]
        public override double L
        {   // bound x's segment
            get { return varL; }
            set
            {
                if (value >= 20) varL = value;
            }
        }

        [Description("Delay in Delta-Kernel")]
        public double d
        {
            get { return varD; }
            set
            {
                if (value > 0) varD = value;
            }
        }

        [Description("Diffusion Coefficient")]
        public double D
        {
            get { return varDiff; }
            set
            {
                if (value > 0) varDiff = value;
            }
        }

        [Description("constant in front of Kernel")]
        public double b { get; set; }

        [Description("Delta-Kernel or not?")]
        public bool DeltaCoupling { get; set; }

        // methods
        public override void Allocate()
        {   // initialize/declare arrays and steps
            // If we want to change one of the parameters: n, m, l, TB,
            // then it needs to call this (plus Intiials) functions again.
            hx = 2 * L / (N - 1); // step for x
            ht = T / (M - 1);  // step for t

            x = new double[N]; // arrange x's
            t = new double[M]; // arrange t's
            properties.Allocate();

            Parallel.Invoke(
                () =>
                {
                    if (x != null)
                        for (int i = 0; i < N; i++)
                            x[i] = -L + i * hx;
                },
                () =>
                {
                    if (t != null && properties != null)
                        for (int j = 0; j < M; j++)
                        {
                            t[j] = j * ht;
                            properties.Init(j);
                        }
                }
            );

            u = new double[M, N];
        }

        public override void Reload()
        {
            properties.Reload();
        }

        public override void Initials()
        {   // Initialize initials
            // setting an initial waves at t = 0
            Initials("");
        }

        public override void Initials(String UX0)
        {   // Initialize initials
            // setting an initial waves at t = 0
            Parallel.Invoke(
                () =>
                {
                    if (u != null)
                    {
                        if (UX0.Equals(""))
                        {
                            for (int i = 0; i < N; i++)
                                u[0, i] = U_x_0(x[i]);
                        }
                        else
                        {
                            Parser pUX0 = new Parser();
                            for (int i = 0; i < N; i++)
                            {
                                pUX0.Evaluate(UX0.Replace("x", x[i].ToString()));
                                u[0, i] = pUX0.Result;
                            }
                        }
                    }
                }
            );
        }

        public override void InitialsFurther()
        {
            // setting an initial waves
            // at t = T to solve further
            if (u != null)
                for (int i = 0; i < N; i++)
                {
                    u[0, i] = u[M - 1, i];
                }
        }

        public override bool Solve()
        {
            // If we changed ONLY eps, beta, gamma, b, d, Kernel, f or Iext,
            // then just recall this function.

            double step = D * ht / (hx * hx);

            double[] P = new double[N];
            double[] Q = new double[N];

            double[] ai = new double[3] { 0, -step, 1 };
            double[] bi = new double[3] { -1, -1 - 2 * step, 1 };
            double[] ci = new double[3] { -1, -step, 0 };
            double[] di = new double[N];
            di[0] = 0; di[N - 1] = 0; // if Neumann condition changes (smth except du/dn = zero), it needs to be commented

            P[0] = ci[0] / bi[0];
            for (int i = 1; i < N - 1; i++) P[i] = ci[1] / (bi[1] - ai[1] * P[i - 1]);
            P[N - 1] = ci[2] / (bi[2] - ai[2] * P[N - 2]);

            for (int j = 0; j < M - 1; j++)
            {
                if (!CalculateLayerJ(Q, P, ai, bi, di, j))
                    return false;
                properties.Calculate(j);
            }

            return true;
        }

        private bool CalculateLayerJ(double[] Q, double[] P, double[] ai, double[] bi, double[] di, int j)
        {
            int k = Convert.ToInt32(d / hx);
            d = hx * k;

            //di[0] = ht * u_0_t(t[j]); // if Neumann condition is not a zero
            Q[0] = -di[0] / bi[0];
            for (int i = 1; i < N - 1; i++)
            {
                CalculateDCoeff(di, i, j, k);

                Q[i] = (ai[1] * Q[i - 1] - di[i]) / (bi[1] - ai[1] * P[i - 1]);

                // catching Q is NaN
                if (Double.IsNaN(Q[i]))
                    return false;
            }
            //di[N - 1] = ht * u_l_t(t[j]); // if Neumann condition is not a zero
            Q[N - 1] = (ai[2] * Q[N - 2] - di[N - 1]) / (bi[2] - ai[2] * P[N - 2]);

            // starting back substitution
            u[j + 1, N - 1] = Q[N - 1];
            for (int i = N - 2; i > -1; i--) u[j + 1, i] = P[i] * u[j + 1, i + 1] + Q[i];

            return true;
        }

        private void CalculateDCoeff(double[] di, int i, int j, int k)
        {
            if (b == 0)
                di[i] = u[j, i] + ht * F(u[j, i]);
            else
            {
                if (DeltaCoupling)
                    di[i] = u[j, i] + ht * (0.5 * b * UDelta(i, j, k) + F(u[j, i]));
                else
                    di[i] = u[j, i] + ht * (b * (Integral(j, i) - u[j, i]) + F(u[j, i]));
            }
        }

        private double UDelta(int i, int j, int k)
        {
            double uxminusd = 0, uxplusd = 0;
            if (i - k <= 0) // if x - d <= -L
                uxminusd = u[j, 0];
            else // if x - d > -L
                uxminusd = u[j, i - k];

            if (i + k >= N - 1) // x + d >= L
                uxplusd = u[j, N - 1];
            else // if x + d < L
                uxplusd = u[j, i + k];

            return uxminusd + uxplusd - 2 * u[j, i];
        }

        public double GetVelocity(int j0)
        {
            return properties.GetVelocity(j0);
        }

        public double GetX(int i)
        { return x[i]; }

        public double GetU(int j, int i)
        { return u[j, i]; }

        // various functions
        private double Integral(int j, int i)
        {   // Trapezoidal rule, uniform grid
            // integrating at point (t[j], x[i]) from -l to l
            double sum = 0;
            double xi = x[i];

            sum += 0.5 * (Kernel(xi - x[0]) * u[j, 0] + Kernel(xi - x[N - 1]) * u[j, N - 1]);
            for (int k = 1; k < N - 1; k++)
                sum += Kernel(xi - x[k]) * u[j, k];

            return hx * sum;
        }

        private double Kernel(double z)
        {
            return 0.5 * Math.Exp(-Math.Abs(z + 2));
        }

        private double U_x_0(double x)
        {	// initial u wave at t = 0
            return 0.5 * (1 + Math.Tanh(x / (2 * Math.Sqrt(2))));
        }

        private double U_0_t(double t) { return 0.0; } // Neumann boundary condition at x = -l

        private double U_l_t(double t) { return 0.0; } // Neumann boundary condition at x = l

        public override void Dispose()
        {
            base.Dispose();
            x = null;
            u = null;
            properties.Dispose();
            properties = null;
        }
    }
}
