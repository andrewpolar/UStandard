using System;

namespace UStandard
{
    class Program
    {
        static void Main(string[] args)
        {
            Dataholder dh = new Dataholder();

            double mu = 0.05; // regularization
            Console.WriteLine("\nFormula modelling");
            dh.BuildFormulaData();
            KolmogorovModel fm = new KolmogorovModel(dh);
            fm.RunContinuous(100, mu);

            Console.WriteLine("\nModelling of physical object");
            dh.BuildAirfoilData();
            KolmogorovModel af = new KolmogorovModel(dh);
            af.RunContinuous(1000, mu);

            Console.WriteLine("\nModelling of biological system");
            dh.BuildMClassData();
            KolmogorovModel ms = new KolmogorovModel(dh);
            ms.RunQuantized(100, mu);

            Console.WriteLine("\nModelling of sociological data");
            dh.BuildBankChurnData();
            KolmogorovModel bc = new KolmogorovModel(dh);
            bc.RunQuantized(1000, mu);
        }
    }
}
