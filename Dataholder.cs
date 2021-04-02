using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UStandard
{
    enum DataType
    {
        TRAIN, SELECT, TEST
    }

    class Dataholder
    {
        public List<double[]> _inputs = new List<double[]>();
        public List<double> _target = new List<double>();
        public List<DataType> _dt = new List<DataType>();
        public double[] _xmin = null;
        public double[] _xmax = null;
        public double _targetMin;
        public double _targetMax;
        public int _BOTTOM_BLOCKS;
        public int[] _LINEAR_BLOCKS = null;
        private Random _rnd = new Random();

        private void FindMinMax()
        {
            int size = _inputs[0].Length;
            _xmin = new double[size];
            _xmax = new double[size];

            for (int i = 0; i < size; ++i)
            {
                _xmin[i] = double.MaxValue;
                _xmax[i] = double.MinValue;
            }

            for (int i = 0; i < _inputs.Count; ++i)
            {
                for (int j = 0; j < _inputs[i].Length; ++j)
                {
                    if (_inputs[i][j] < _xmin[j]) _xmin[j] = _inputs[i][j];
                    if (_inputs[i][j] > _xmax[j]) _xmax[j] = _inputs[i][j];
                }

            }

            _targetMin = double.MaxValue;
            _targetMax = double.MinValue;
            for (int j = 0; j < _target.Count; ++j)
            {
                if (_target[j] < _targetMin) _targetMin = _target[j];
                if (_target[j] > _targetMax) _targetMax = _target[j];
            }
        }

        public void AssignDataTypes()
        {
            int size = _inputs.Count;
            for (int i = 0; i < size; ++i)
            {
                if (_rnd.Next() % 100 <= 60) _dt.Add(DataType.TRAIN);
                else
                {
                    if (_rnd.Next() % 100 <= 50) _dt.Add(DataType.SELECT);
                    else _dt.Add(DataType.TEST);
                }
            }
        }

        private void ShowDataStat(string fileName)
        {
            //Stat of data sample
            int nTrain = 0;
            int nSelect = 0;
            int nTest = 0;
            int nTotal = 0;
            foreach (DataType dt in _dt)
            {
                if (DataType.TRAIN == dt) ++nTrain;
                else if (DataType.SELECT == dt) ++nSelect;
                else if (DataType.TEST == dt) ++nTest;
                ++nTotal;
            }
            if (null == fileName)
                Console.WriteLine("Data is ready");
            else 
                Console.WriteLine("Data is ready for file {0}", fileName);
            Console.WriteLine("Total = {0}, training sample = {1}, selection sample = {2}, test sample = {3}",
                nTotal, nTrain, nSelect, nTest);
        }

        public void BuildAirfoilData()
        {
            _BOTTOM_BLOCKS = 15;
            _LINEAR_BLOCKS = new int[] { 11, 11, 11, 11, 11 };

            _inputs.Clear();
            _target.Clear();
            _dt.Clear();

            string fileName = @"..\..\..\data\airfoil_self_noise.csv";
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File not found {0}", fileName);
                Environment.Exit(0);
            }
            string[] lines = System.IO.File.ReadAllLines(fileName);
            for (int m = 1; m < lines.Length; ++m) //we skip first line
            {
                string[] sdata = lines[m].Split(';');
                if (6 != sdata.Length)
                {
                    Console.WriteLine("Misformatted data");
                    Environment.Exit(0);
                }
                double[] data = new double[5];
                for (int i = 0; i < data.Length; ++i)
                {
                    Double.TryParse(sdata[i], out data[i]);
                }
                double target = 0.0;
                Double.TryParse(sdata[5], out target);
                _inputs.Add(data);
                _target.Add(target);
            }

            AssignDataTypes();
            FindMinMax();
            ShowDataStat(fileName);
        }

        public void BuildFormulaData()
        {
            _BOTTOM_BLOCKS = 11;
            _LINEAR_BLOCKS = new int[] { 10, 10, 10, 10, 10 };

            _inputs.Clear();
            _target.Clear();
            _dt.Clear();

            int N = 4000;
            for (int i = 0; i < N; ++i)
            {
                double[] x = new double[5];
                x[0] = (_rnd.Next() % 100) / 100.0;
                x[1] = (_rnd.Next() % 100) / 100.0 * 3.14 / 2.0;
                x[2] = (_rnd.Next() % 50) / 100.0 + 1;
                x[3] = (_rnd.Next() % 100) / 100.0 + 0.4;
                x[4] = (_rnd.Next() % 100) / 200.0;

                double y = Math.Abs(Math.Pow(Math.Sin(x[1]), x[0]) - 1.0 / Math.Exp(x[2])) / x[3] + x[4] * Math.Cos(x[4]);
                _inputs.Add(x);
                _target.Add(y);
            }
            AssignDataTypes();
            FindMinMax();
            ShowDataStat(null);
        }

        public void BuildBankChurnData()
        {
            //15634602; 619; France; Female; 42; 2; 0; 1; 1; 1; 101348.88; 1
            //customer_id; credit_score; country; gender; age; tenure; balance; products_number; 
            //credit_card; active_member; estimated_salary; churn

            _BOTTOM_BLOCKS = 3;
            _LINEAR_BLOCKS = new int[] { 4, 3, 2, 5, 7, 10, 5, 2, 2, 8 };

            Dictionary<string, double> country = new Dictionary<string, double>();
            country.Add("France", 0.0);
            country.Add("Germany", 1.0);
            country.Add("Spain", 2.0);

            Dictionary<string, double> gender = new Dictionary<string, double>();
            gender.Add("Male", 0.0);
            gender.Add("Female", 1.0);

            _inputs.Clear();
            _target.Clear();
            _dt.Clear();

            string fileName = @"..\..\..\data\bankchurn.csv";
            using (StreamReader file = new StreamReader(fileName))
            {
                string line = file.ReadLine(); //skip first line
                while (true)
                {
                    line = file.ReadLine();
                    if (null == line) break;

                    string[] data = line.Split(';');
                    if (12 != data.Length)
                    {
                        Console.WriteLine("Misformatted line {0}", line);
                        Environment.Exit(0);
                    }

                    double[] x = new double[10];
                    Double.TryParse(data[1], out x[0]);
                    x[1] = country[data[2].Trim()];
                    x[2] = gender[data[3].Trim()];
                    Double.TryParse(data[4], out x[3]);
                    Double.TryParse(data[5], out x[4]);
                    Double.TryParse(data[6], out x[5]);
                    Double.TryParse(data[7], out x[6]);
                    Double.TryParse(data[8], out x[7]);
                    Double.TryParse(data[9], out x[8]);
                    Double.TryParse(data[10], out x[9]);

                    double target;
                    Double.TryParse(data[11], out target);

                    _inputs.Add(x);
                    _target.Add(target);
                }
                file.Close();
            }

            AssignDataTypes();
            FindMinMax();
            ShowDataStat(fileName);
        }

        public void BuildMClassData()
        {
            _BOTTOM_BLOCKS = 3;
            _LINEAR_BLOCKS = new int[] { 6, 4, 10, 2, 9, 2, 2, 2, 12, 2, 5, 4, 4, 9, 9, 2, 4, 3, 5, 9, 6, 7 };

            Dictionary<string, double> mclass = new Dictionary<string, double>();
            mclass.Add("p", 0.0);
            mclass.Add("e", 1.0);

            Dictionary<string, double> capshape = new Dictionary<string, double>();
            capshape.Add("x", 0.0);
            capshape.Add("b", 1.0);
            capshape.Add("s", 2.0);
            capshape.Add("f", 3.0);
            capshape.Add("k", 4.0);
            capshape.Add("c", 5.0);

            Dictionary<string, double> capsurface = new Dictionary<string, double>();
            capsurface.Add("s", 0.0);
            capsurface.Add("y", 1.0);
            capsurface.Add("f", 2.0);
            capsurface.Add("g", 3.0);

            Dictionary<string, double> capcolor = new Dictionary<string, double>();
            capcolor.Add("n", 0.0);
            capcolor.Add("y", 1.0);
            capcolor.Add("w", 2.0);
            capcolor.Add("g", 3.0);
            capcolor.Add("e", 4.0);
            capcolor.Add("p", 5.0);
            capcolor.Add("b", 6.0);
            capcolor.Add("u", 7.0);
            capcolor.Add("c", 8.0);
            capcolor.Add("r", 9.0);

            Dictionary<string, double> bruises = new Dictionary<string, double>();
            bruises.Add("t", 0.0);
            bruises.Add("f", 1.0);

            Dictionary<string, double> odor = new Dictionary<string, double>();
            odor.Add("p", 0.0);
            odor.Add("a", 1.0);
            odor.Add("l", 2.0);
            odor.Add("n", 3.0);
            odor.Add("f", 4.0);
            odor.Add("c", 5.0);
            odor.Add("y", 6.0);
            odor.Add("s", 7.0);
            odor.Add("m", 8.0);

            Dictionary<string, double> gillattachment = new Dictionary<string, double>();
            gillattachment.Add("f", 0.0);
            gillattachment.Add("a", 1.0);

            Dictionary<string, double> gillspacing = new Dictionary<string, double>();
            gillspacing.Add("c", 0.0);
            gillspacing.Add("w", 1.0);

            Dictionary<string, double> gillsize = new Dictionary<string, double>();
            gillsize.Add("n", 0.0);
            gillsize.Add("b", 1.0);

            Dictionary<string, double> gillcolor = new Dictionary<string, double>();
            gillcolor.Add("k", 0.0);
            gillcolor.Add("n", 1.0);
            gillcolor.Add("g", 2.0);
            gillcolor.Add("p", 3.0);
            gillcolor.Add("w", 4.0);
            gillcolor.Add("h", 5.0);
            gillcolor.Add("u", 6.0);
            gillcolor.Add("e", 7.0);
            gillcolor.Add("b", 8.0);
            gillcolor.Add("r", 9.0);
            gillcolor.Add("y", 10.0);
            gillcolor.Add("o", 11.0);

            Dictionary<string, double> stalkshape = new Dictionary<string, double>();
            stalkshape.Add("e", 0.0);
            stalkshape.Add("t", 1.0);

            Dictionary<string, double> stalkroot = new Dictionary<string, double>();
            stalkroot.Add("e", 0.0);
            stalkroot.Add("c", 1.0);
            stalkroot.Add("b", 2.0);
            stalkroot.Add("r", 3.0);
            stalkroot.Add("?", 4.0);

            Dictionary<string, double> stalksurfaceabovering = new Dictionary<string, double>();
            stalksurfaceabovering.Add("s", 0.0);
            stalksurfaceabovering.Add("f", 1.0);
            stalksurfaceabovering.Add("k", 2.0);
            stalksurfaceabovering.Add("y", 3.0);

            Dictionary<string, double> stalksurfacebelowring = new Dictionary<string, double>();
            stalksurfacebelowring.Add("s", 0.0);
            stalksurfacebelowring.Add("f", 1.0);
            stalksurfacebelowring.Add("y", 2.0);
            stalksurfacebelowring.Add("k", 3.0);

            Dictionary<string, double> stalkcolorabovering = new Dictionary<string, double>();
            stalkcolorabovering.Add("w", 0.0);
            stalkcolorabovering.Add("g", 1.0);
            stalkcolorabovering.Add("p", 2.0);
            stalkcolorabovering.Add("n", 3.0);
            stalkcolorabovering.Add("b", 4.0);
            stalkcolorabovering.Add("e", 5.0);
            stalkcolorabovering.Add("o", 6.0);
            stalkcolorabovering.Add("c", 7.0);
            stalkcolorabovering.Add("y", 8.0);

            Dictionary<string, double> stalkcolorbelowring = new Dictionary<string, double>();
            stalkcolorbelowring.Add("w", 0.0);
            stalkcolorbelowring.Add("p", 1.0);
            stalkcolorbelowring.Add("g", 2.0);
            stalkcolorbelowring.Add("b", 3.0);
            stalkcolorbelowring.Add("n", 4.0);
            stalkcolorbelowring.Add("e", 5.0);
            stalkcolorbelowring.Add("y", 6.0);
            stalkcolorbelowring.Add("o", 7.0);
            stalkcolorbelowring.Add("c", 8.0);

            Dictionary<string, double> veiltype = new Dictionary<string, double>();
            veiltype.Add("p", 0.0);

            Dictionary<string, double> veilcolor = new Dictionary<string, double>();
            veilcolor.Add("w", 0.0);
            veilcolor.Add("n", 1.0);
            veilcolor.Add("o", 2.0);
            veilcolor.Add("y", 3.0);

            Dictionary<string, double> ringnumber = new Dictionary<string, double>();
            ringnumber.Add("o", 0.0);
            ringnumber.Add("t", 1.0);
            ringnumber.Add("n", 2.0);

            Dictionary<string, double> ringtype = new Dictionary<string, double>();
            ringtype.Add("p", 0.0);
            ringtype.Add("e", 1.0);
            ringtype.Add("l", 2.0);
            ringtype.Add("f", 3.0);
            ringtype.Add("n", 4.0);

            Dictionary<string, double> sporeprintcolor = new Dictionary<string, double>();
            sporeprintcolor.Add("k", 0.0);
            sporeprintcolor.Add("n", 1.0);
            sporeprintcolor.Add("u", 2.0);
            sporeprintcolor.Add("h", 3.0);
            sporeprintcolor.Add("w", 4.0);
            sporeprintcolor.Add("r", 5.0);
            sporeprintcolor.Add("o", 6.0);
            sporeprintcolor.Add("y", 7.0);
            sporeprintcolor.Add("b", 8.0);

            Dictionary<string, double> population = new Dictionary<string, double>();
            population.Add("s", 0.0);
            population.Add("n", 1.0);
            population.Add("a", 2.0);
            population.Add("v", 3.0);
            population.Add("y", 4.0);
            population.Add("c", 5.0);

            Dictionary<string, double> habitat = new Dictionary<string, double>();
            habitat.Add("u", 0.0);
            habitat.Add("g", 1.0);
            habitat.Add("m", 2.0);
            habitat.Add("d", 3.0);
            habitat.Add("p", 4.0);
            habitat.Add("w", 5.0);
            habitat.Add("l", 6.0);

            _inputs.Clear();
            _target.Clear();
            _dt.Clear();

            string fileName = @"..\..\..\data\mushrooms.csv";
            using (StreamReader file = new StreamReader(fileName))
            {
                string line = file.ReadLine(); //skip first line
                while (true)
                {
                    line = file.ReadLine();
                    if (null == line) break;

                    string[] data = line.Split(',');
                    if (23 != data.Length)
                    {
                        Console.WriteLine("Misformatted line {0}", line);
                        Environment.Exit(0);
                    }

                    _target.Add(mclass[data[0].Trim()]);

                    //inputs
                    double[] x = new double[22];
                    x[0] = capshape[data[1].Trim()];
                    x[1] = capsurface[data[2].Trim()];
                    x[2] = capcolor[data[3].Trim()];
                    x[3] = bruises[data[4].Trim()];
                    x[4] = odor[data[5].Trim()];
                    x[5] = gillattachment[data[6].Trim()];
                    x[6] = gillspacing[data[7].Trim()];
                    x[7] = gillsize[data[8].Trim()];
                    x[8] = gillcolor[data[9].Trim()];
                    x[9] = stalkshape[data[10].Trim()];
                    x[10] = stalkroot[data[11].Trim()];
                    x[11] = stalksurfaceabovering[data[12].Trim()];
                    x[12] = stalksurfacebelowring[data[13].Trim()];
                    x[13] = stalkcolorabovering[data[14].Trim()];
                    x[14] = stalkcolorbelowring[data[15].Trim()];
                    x[15] = veiltype[data[16].Trim()];
                    x[16] = veilcolor[data[17].Trim()];
                    x[17] = ringnumber[data[18].Trim()];
                    x[18] = ringtype[data[19].Trim()];
                    x[19] = sporeprintcolor[data[20].Trim()];
                    x[20] = population[data[21].Trim()];
                    x[21] = habitat[data[22].Trim()];

                    _inputs.Add(x);
                }
            }

            AssignDataTypes();
            FindMinMax();
            _xmax[15] = 1.0; //it takes only one value
            ShowDataStat(fileName);
        }
    }
}
