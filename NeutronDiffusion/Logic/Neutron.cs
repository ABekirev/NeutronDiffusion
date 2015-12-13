using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace NeutronDiffusion.Logic
{
    public class Neutron
    {
        private static readonly Random _r = new Random();
        private static readonly object randonerLocker = new object();

        private static Random R
        {
            get
            {
                lock (randonerLocker)
                {
                    return _r;
                }
            }
        }
        public List<double> FreePathLength { get; set; }
        private List<Vector3D> GuidedCos { get; }
        public List<CustomPoint3D> CollisionPoint { get; set; }
        public double AverageFreePathLength { get; set; }
        public double PathLength { get; set; }
        private bool IsAbsorbed { get; set; }

        private const double TwoPi = Math.PI*2;
        private readonly double _sigmaA;
        private readonly double _sigmaS;
        private readonly double _sigmaTr;
        private readonly double _sigmaT;

        public Neutron(CustomPoint3D startPoint, double sigmaA, double sigmaS, double sigmaTr, double sigmaT)
        {
            _sigmaA = sigmaA;
            _sigmaS = sigmaS;
            _sigmaTr = sigmaTr;
            _sigmaT = sigmaT;
            AverageFreePathLength = 0;

            IsAbsorbed = false;
            FreePathLength = new List<double>() { 0 };
            GuidedCos = new List<Vector3D>() { new Vector3D() };
            CollisionPoint = new List<CustomPoint3D>() { startPoint };
        }

        public void Move()
        {
            if (IsAbsorbed)
                return;
            var step = CollisionPoint.Count;
            while (!IsAbsorbed)
            {
                SubStep(step, CollisionPoint[step - 1]);
                step += 1;
            }
            AverageFreePathLength = PathLength / (FreePathLength.Count - 1);
        }

        private void SubStep(int step, CustomPoint3D startPoint)
        {
            var gamma1 = Rnd(double.Epsilon, 1);
            FreePathLength.Add(-Math.Log(gamma1) / _sigmaS);
            var gamma2x = Rnd(0, 1);
            var gamma2y = Rnd(0, 1);
            //var gamma2z = Rnd(0, 1);
            //var wz = 1 - 2 * gamma2z;
            //var tmp2 = Math.Sqrt(1 - Math.Pow(cosZ, 2));
            //var wx = tmp2 * Math.Cos(TwoPi * gamma2x);
            //var vy = tmp2 * Math.Sin(TwoPi * gamma2y);
            var wx = Math.Cos(TwoPi * gamma2x);
            var wy = Math.Sin(TwoPi * gamma2y);
            var wz = 0;
            GuidedCos.Add(new Vector3D(wx, wy, wz));
            CollisionPoint.Add(new CustomPoint3D(
                startPoint.X + GuidedCos[step].X * FreePathLength[step],
                startPoint.Y + GuidedCos[step].Y * FreePathLength[step],
                startPoint.Z + GuidedCos[step].Z * FreePathLength[step]));
            PathLength += FreePathLength[step];
            var gamma3 = Rnd(0, 1);
            if (gamma3 <= _sigmaA/_sigmaT)
                IsAbsorbed = true;

            //Console.WriteLine(@"FreePathLength: {0};	wx: {1};	wy: {2};wz: {3};	gamma3: {4};	SigmaA: {5};	SigmaTr: {6};	{7}", FreePathLength[step], wx, wy, wz, gamma3, _sigmaA, _sigmaTr, _sigmaA/_sigmaTr);
        }
        
        private double Rnd(double a, double b)
        {
            return a + R.NextDouble() * (b - a);
        }


    }
}
