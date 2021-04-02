using System;
using System.Collections.Generic;
using System.Text;

namespace UStandard
{
    class KolmogorovModel
    {
        private Dataholder _dh = null;
        private List<U> _ulist = new List<U>();
        private U _bigU = null;
        private Random _rnd = new Random();
        private double _targetMin;
        private double _targetMax;

        public KolmogorovModel(Dataholder DH)
        {
            _dh = DH;
            _targetMin = _dh._targetMin;
            _targetMax = _dh._targetMax;
        }

        private void Swap(ref int n1, ref int n2)
        {
            int v1 = n1;
            n1 = n2;
            n2 = v1;
        }

        private int[] Shuffle(int size)
        {
            int[] position = new int[size];
            for (int i = 0; i < size; ++i)
            {
                position[i] = i;
            }
            for (int i = 0; i < position.Length; ++i)
            {
                int plusPos = _rnd.Next() % position.Length;
                int next = i + plusPos;
                if (next > position.Length - 1) next -= (position.Length - 1);

                int el1 = position[i];
                int el2 = position[next];
                Swap(ref el1, ref el2);
                position[i] = el1;
                position[next] = el2;
            }
            return position;
        }

        private void GenerateInitialOperators(int M, int[] nx)
        {
            _ulist.Clear();
            int size = _dh._inputs[0].Length;
            int[] positions = new int[size];
            for (int i = 0; i < size; ++i)
            {
                positions[i] = i;
            }

            U u = new U(positions);
            u.Initialize(size, nx, _dh._xmin, _dh._xmax);
            for (int count = 1; count <= 10; ++count)
            {
                double accuracy = 0.0;
                int cnt = 0;
                for (int i = 0; i < _dh._inputs.Count; ++i)
                {
                    if (DataType.TRAIN != _dh._dt[i]) continue;
                    double[] data = _dh._inputs[i];
                    double diff = _dh._target[i] - u.GetU(data);
                    u.Update(diff, data, 0.05);
                    accuracy += diff * diff;
                    ++cnt;
                }
                accuracy /= cnt;
                accuracy = Math.Sqrt(accuracy);
                accuracy /= (_targetMax - _targetMin);
            }
            _ulist.Add(u);

            for (int counter = 1; counter < M; ++counter)
            {
                U uc = new U(positions);
                uc.Initialize(size, nx, _dh._xmin, _dh._xmax);
                uc.SetRandom(_targetMin, _targetMax);
                _ulist.Add(uc);
            }
        }

        private double[] GetVector(double[] data)
        {
            int size = _ulist.Count;
            double[] vector = new double[size];
            for (int i = 0; i < size; ++i)
            {
                vector[i] = _ulist[i].GetU(data);
            }
            return vector;
        }

        private void BuildRepresentation(int M, int[] nx, int steps, double mu)
        {
            double[] min = new double[M];
            double[] max = new double[M];
            for (int i = 0; i < M; ++i)
            {
                min[i] = _targetMin;
                max[i] = _targetMax;
            }
            
            if (null != _bigU)
            {
                _bigU.Clear();
                _bigU = null;
            }

            int[] positions = new int[M];
            for (int k = 0; k < M; ++k)
            {
                positions[k] = k;
            }
            _bigU = new U(positions);              
            _bigU.Initialize(M, nx, min, max);
            int N = _dh._inputs.Count;
            double error = (_targetMax - _targetMin) / 150.0;
            double training_error = 0.0;

            for (int step = 0; step < steps; ++step)
            {
                double RMSE = 0.0;
                int cnt = 0;
                for (int i = 0; i < N; ++i)
                {
                    if (DataType.TRAIN != _dh._dt[i]) continue;
                    double[] data = _dh._inputs[i];
                    double[] v = GetVector(data);
                    double value = _bigU.GetU(v);
                    double diff = _dh._target[i] - value;

                    if (Math.Abs(diff) > error)
                    {
                        for (int k = 0; k < _ulist.Count; ++k)
                        {
                            double derrivative = _bigU.GetDerrivative(k, v[k]);
                            if (Math.Abs(derrivative) < 0.1)
                            {
                                if (derrivative < 0.0) derrivative = -0.1;
                                else derrivative = 0.1;
                            }
                            _ulist[k].Update(diff / derrivative, data, mu);
                        }
                        v = GetVector(data);
                        diff = _dh._target[i] - _bigU.GetU(v);
                        _bigU.Update(diff, v, mu);
                    }
                    RMSE += diff * diff;
                    ++cnt;
                }

                RMSE /= cnt;
                RMSE = Math.Sqrt(RMSE);
                RMSE /= (_targetMax - _targetMin);
                //Console.WriteLine("Error in big operator {0:0.0000}", RMSE);
                training_error = RMSE;
            }
            //Console.WriteLine("Training is completed, relative error {0:0.0000}", training_error);
        }

        private double DoSelection()
        {
            double RMSE = 0.0;
            int cnt = 0;
            int N = _dh._inputs.Count;
            for (int i = 0; i < N; ++i)
            {
                if (DataType.SELECT != _dh._dt[i]) continue;
                double[] data = _dh._inputs[i];
                double[] v = GetVector(data);
                double diff = _dh._target[i] - _bigU.GetU(v);
                RMSE += diff * diff;
                ++cnt;
            }
            RMSE /= cnt;
            RMSE = Math.Sqrt(RMSE);
            RMSE /= (_targetMax - _targetMin);
            return RMSE;
        }

        private double DoTest()
        {
            double RMSE = 0.0;
            int cnt = 0;
            int N = _dh._inputs.Count;
            for (int i = 0; i < N; ++i)
            {
                if (DataType.TEST != _dh._dt[i]) continue;
                double[] data = _dh._inputs[i];
                double[] v = GetVector(data);
                double diff = _dh._target[i] - _bigU.GetU(v);
                RMSE += diff * diff;
                ++cnt;
            }
            RMSE /= cnt;
            RMSE = Math.Sqrt(RMSE);
            RMSE /= (_targetMax - _targetMin);
            return RMSE;
        }

        private double ComputeCorrelationCoeff()
        {
            int N = _dh._inputs.Count;
            double[] targetEstimate = new double[N];
            int count = 0;
            for (int i = 0; i < N; ++i)
            {
                double[] data = _dh._inputs[i];
                double[] v = GetVector(data);
                targetEstimate[i] = _bigU.GetU(v);
                if (DataType.TEST == _dh._dt[i]) ++count;
            }
            double[] x = new double[count];
            double[] y = new double[count];
            count = 0;
            for (int i = 0; i < N; ++i)
            {
                if (DataType.TEST == _dh._dt[i])
                {
                    x[count] = targetEstimate[i];
                    y[count] = _dh._target[i];
                    ++count;
                }
            }
            return Static.PearsonCorrelation(x, y);
        }

        private double DoSelectionQuantized()
        {
            int right = 0;
            int total = 0;
            int N = _dh._inputs.Count;
            for (int i = 0; i < N; ++i)
            {
                if (DataType.SELECT != _dh._dt[i]) continue;
                double[] data = _dh._inputs[i];
                double[] v = GetVector(data);
                double z = _dh._target[i]; 
                double value = _bigU.GetU(v);
                if (z <= 0.5 && value <= 0.5) ++right;
                if (z > 0.5 && value > 0.5) ++right;
                ++total;
            }
            double ratio = (double)(right);
            ratio /= (double)(total);
            return ratio;
        }

        private double DoTestQuantized()
        {
            int right = 0;
            int total = 0;
            int N = _dh._inputs.Count;
            for (int i = 0; i < N; ++i)
            {
                if (DataType.TEST != _dh._dt[i]) continue;
                double[] data = _dh._inputs[i];
                double[] v = GetVector(data);
                double z = _dh._target[i];
                double value = _bigU.GetU(v);
                if (z <= 0.5 && value <= 0.5) ++right;
                if (z > 0.5 && value > 0.5) ++right;
                ++total;
            }
            double ratio = (double)(right);
            ratio /= (double)(total);
            return ratio;
        }

        public void RunContinuous(int steps, double mu)
        {
            int M = _dh._BOTTOM_BLOCKS;
            int[] bottom = _dh._LINEAR_BLOCKS;
            int[] top = new int[M];
            for (int i = 0; i < M; ++i)
            {
                top[i] = 8;
            }

            for (int count = 0; count < 10; ++count)
            {
                GenerateInitialOperators(M, bottom);
                BuildRepresentation(M, top, steps, mu);
                double selectionError = DoSelection();
                double pearson = ComputeCorrelationCoeff();
                Console.WriteLine("Run = {0}, selection sample error = {1:0.0000}, test sample pearson correlation coeff. = {2:0.0000}",
                    count, selectionError, pearson);
            }
        }

        public void RunQuantized(int steps, double mu)
        {
            int M = _dh._BOTTOM_BLOCKS;
            int[] bottom = _dh._LINEAR_BLOCKS;
            int[] top = new int[M];
            for (int i = 0; i < M; ++i)
            {
                top[i] = 8;
            }

            for (int count = 0; count < 10; ++count)
            {
                GenerateInitialOperators(M, bottom);
                BuildRepresentation(M, top, steps, mu);
                double selectionError = DoSelectionQuantized();
                double testError = DoTestQuantized();
                Console.WriteLine("Run = {0}, selection sample error = {1:0.0000}, test sample error = {2:0.0000}", count, selectionError, testError);
            }
        }
    }
}
