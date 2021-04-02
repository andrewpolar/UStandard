using System;
using System.Collections.Generic;
using System.Text;

namespace UStandard
{
    class U
    {
        private List<PLL> _plist = new List<PLL>();
        private int _nt;
        private int[] _positions = null;

        public U(int[] group)
        {
            _positions = new int[group.Length];
            for (int i = 0; i < group.Length; ++i)
            {
                _positions[i] = group[i];
            }
        }

        public void Clear()
        {
            _plist.Clear();
        }

        public double GetDerrivative(int layer, double x)
        {
            return _plist[layer].GetDerrivative(x);
        }

        public void SetLinear(double ymin, double ymax)
        {
            for (int i = 0; i < _nt; ++i)
            {
                _plist[i].SetLinear(ymin, ymax);
            }
        }

        public void SetRandom(double ymin, double ymax)
        {
            for (int i = 0; i < _nt; ++i)
            {
                _plist[i].SetRandom(ymin, ymax);
            }
        }

        public void Initialize(int nt, int[] nx, double[] xmin, double[] xmax)
        {
            _nt = nt;
            for (int i = 0; i < _nt; ++i)
            {
                _plist.Add(new PLL(nx[i], xmin[i], xmax[i]));
            }
        }

        public void Initialize(int nt, int nx, double[] xmin, double[] xmax)
        {
            _nt = nt;
            for (int i = 0; i < _nt; ++i)
            {
                _plist.Add(new PLL(nx, xmin[i], xmax[i]));
            }
        }

        public void Update(double y, double[] inputs, double mu)
        {
            y /= _nt;
            for (int i = 0; i < _nt; ++i)
            {
                _plist[i].Update(inputs[_positions[i]], y, mu);
            }
        }

        public double GetU(double[] inputs)
        {
            double f = 0.0;
            for (int i = 0; i < _nt; ++i)
            {
                f += _plist[i].GetFunction(inputs[_positions[i]]);
            }
            return f;
        }

        public int GetNumberOfLayers()
        {
            return _nt;
        }

        public void RenormalizeToCurrent()
        {
            for (int i = 0; i < _nt; ++i)
            {
                _plist[i].RenormalizeToCurrent();
            }
        }

        public void ShowOperator()
        {
            int nt = _plist.Count;
            Console.WriteLine("-------------- Operator --------------");
            for (int i = 0; i < nt; ++i)
            {
                int nx = _plist[i].GetNumberOfPoints();
                for (int j = 0; j < nx - 1; ++j)
                {
                    Console.Write("{0:0.0000}, ", _plist[i].GetY(j));
                }
                Console.Write("{0:0.0000}", _plist[i].GetY(nx - 1));
                Console.WriteLine();
            }
            Console.WriteLine("----------------------");
        }
    }
}
