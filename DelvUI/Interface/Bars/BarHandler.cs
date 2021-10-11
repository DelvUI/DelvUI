using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class BarHandler
    {
        #region singleton
        private static Lazy<BarHandler> _lazyInstance = new Lazy<BarHandler>(() => new BarHandler());

        public static BarHandler Instance => _lazyInstance.Value;

        ~BarHandler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _lazyInstance = new Lazy<BarHandler>(() => new BarHandler());
        }
        #endregion

        private Dictionary<int, List<BarHud>> _retainedBars;

        private int[] _enumValues;
        
        public BarHandler()
        {
            _retainedBars = new Dictionary<int, List<BarHud>>();
            _enumValues = Enum.GetValues(typeof(Strata)).Cast<int>().ToArray();
        }

        public void AddBar(BarHud bar)
        {
            int index = (int) bar.Strata;
            if (_retainedBars.ContainsKey(index))
            {
                _retainedBars[index].Add(bar);
            }
            else
            {
                _retainedBars.Add(index, new List<BarHud>() { bar });
            }
        }
        
        public void DrawBars()
        {
            foreach(int i in _enumValues)
            {
                if (_retainedBars.TryGetValue(i, out var bars))
                {
                    foreach (var bar in bars)
                    {
                        bar.DrawBar();
                    }
                }
            }
            
            _retainedBars = new Dictionary<int, List<BarHud>>();
        }
    }

    public enum Strata
    {
        Bottom = 0,
        Middle = 1,
        Top = 2
    }
}