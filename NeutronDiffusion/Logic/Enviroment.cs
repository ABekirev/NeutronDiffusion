using System;
using System.Collections.Generic;
using System.Linq;

namespace NeutronDiffusion.Logic
{
    public class Enviroment
	{
		public double SigmaS { get; set; }
		public double SigmaA { get; set; }
        public double CosFi { get; set; }
        public double SigmaTr { get; set; }
        public double SigmaT { get; set; }
        public int NeutronNums { get; set; }

        public double TheoreticalMeanFreePath => 1/SigmaS;
        public double TheoreticalMeanPath => 1/SigmaA;

		private readonly List<Neutron> _neutrons = new List<Neutron>();

        public List<Neutron> Neutrons => _neutrons;

        public Enviroment(double sigmaS, double sigmaA, double cosFi)
		{
			SigmaS = sigmaS;
			SigmaA = sigmaA;
			CosFi = cosFi;
		    SigmaTr = sigmaA + sigmaS*(1 - cosFi);
            SigmaT = sigmaA + sigmaS;
		}

        public Enviroment(Enviroment enviroment) : this(enviroment.SigmaS, enviroment.SigmaA, enviroment.CosFi)
        {
            NeutronNums = enviroment.NeutronNums;
        }

		public static void Main2()
		{
		    Enviroment env = new Enviroment(2, 2, 0.1) {NeutronNums = 20000};
		    Console.WriteLine(@"Launching {0} _neutrons...", env.NeutronNums);
			env.SimulateBatchNeutrons();
		}

		public List<Neutron> SimulateBatchNeutrons()
		{
			for (int i = 0; i < NeutronNums; i++)
				_neutrons.Add(new Neutron(new CustomPoint3D(), SigmaA, SigmaS, SigmaTr, SigmaT));
            var threads = new NeutronThreadsWrapper(_neutrons);
            threads.LaunchCalculations();
		    return _neutrons;
        }

        public Neutron SimulateOneNeutron()
        {
            Neutron neutron = new Neutron(new CustomPoint3D(), SigmaA, SigmaS, SigmaTr, SigmaT);
            neutron.Move();
            return neutron;
        }
	}
}
