using System;

namespace TW.Model
{
    class PDEProperties
    {
        private PDE pde;
        private Velocity[] velocities;

        public PDEProperties(PDE pde)
        { this.pde = pde; }

        public double GetVelocity(int j0)
        {
            if (!velocities[j0].calculated)
                Calculate(j0);

            return velocities[j0].velocity;
        }

        public void Allocate()
        {
            velocities = new Velocity[pde.M];
        }

        public void Init(int j)
        {
            velocities[j] = new Velocity();
        }

        public void Reload()
        {
            for (int j = 0; j < pde.M; j++)
            {
                velocities[j].calculated = false;
            }
        }

        public void Dispose()
        {
            velocities = null;
        }

        public void Calculate(int j)
        {
            velocities[j].velocity = CalculateVelocity(j);
            velocities[j].calculated = true;
        }

        private double CalculateVelocity(int j0)
        {
            if (j0 > pde.M - 1)
                return 0;

            int k = Convert.ToInt32(pde.d / pde.GetHx());

            double nominator = 0;
            double denominator = 0;

            nominator += 0.5 * NominatorFunction(pde.N - 1, j0, k);
            denominator += 0.5 * DenominatorFunction(pde.N - 1, j0);
            for (int i = 1; i < pde.N - 1; i++)
            {
                nominator += NominatorFunction(i, j0, k);
                denominator += DenominatorFunction(i, j0);
            }
            nominator += 0.5 * NominatorFunction(0, j0, k);
            denominator += 0.5 * DenominatorFunction(0, j0);

            return -nominator / denominator;
        }

        private double DenominatorFunction(int i, int j)
        {
            return Math.Pow(DiffUx(i, j), 2);
        }

        private double NominatorFunction(int i, int j, int k)
        {
            double uij = pde.GetU(j, i);

            if (pde.b == 0)
                return DiffUx(i, j) * pde.F(uij);
            else
                return DiffUx(i, j) * (pde.F(uij) + pde.b * UDelta(i, j, k));
        }

        private double DiffUx(int i, int j)
        {
            if (i <= 0 || i > pde.N - 1 || j < 0 || j > pde.M - 1)
                return 0;

            return (pde.GetU(j, i) - pde.GetU(j, i - 1)) / pde.GetHx();
        }

        private double UDelta(int i, int j, int k)
        {
            double uxminusd = 0, uxplusd = 0;
            if (i - k <= 0) // if x - d <= -L
                uxminusd = pde.GetU(j, 0);
            else // if x - d > -L
                uxminusd = pde.GetU(j, i - k);

            if (i + k >= pde.N - 1) // x + d >= L
                uxplusd = pde.GetU(j, pde.N - 1);
            else // if x + d < L
                uxplusd = pde.GetU(j, i + k);

            return uxminusd + uxplusd - 2 * pde.GetU(j, i);
        }

        private double CalculateVelocityNumerical(int j0)
        {
            int deltaj = (int)(1 / pde.GetHt());

            int j1 = (j0 + deltaj);
            if (j1 > pde.M - 1) // we're out of frame and no need to find X0 max
                return 0;

            int i0 = 0; // X0 max = u[j0, i0]
            int i1 = 0; // X1 max = u[j1, i0]

            for (int i = 1; i < pde.N; i++)
            {
                if (pde.GetU(j0, i) > pde.GetU(j0, i0))
                    i0 = i;

                if (pde.GetU(j1, i) > pde.GetU(j1, i1))
                    i1 = i;
            }

            double x0 = i0 * pde.GetHx(); double x1 = i1 * pde.GetHx();
            double t0 = j0 * pde.GetHt(); double t1 = j1 * pde.GetHt();

            return (x1 - x0) / (t1 - t0);
        }

        internal class Velocity
        {
            internal double velocity;
            internal bool calculated;

            public override string ToString()
            {
                if (calculated)
                    return String.Format("{0}", velocity);
                else
                    return "-";
            }
        }
    }
}
