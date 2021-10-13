using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelvUI.Helpers
{
    public class SmoothHPHelper
    {
        private float? _startHp;
        private float? _targetHp;
        private float? _lastHp;

        public void Reset()
        {
            _startHp = null;
            _targetHp = null;
            _lastHp = null;
        }

        public uint GetNextHp(int currentHp, int maxHp, float velocity)
        {
            if (!_startHp.HasValue || !_targetHp.HasValue || !_lastHp.HasValue)
            {
                _lastHp = currentHp;
                _startHp = currentHp;
                _targetHp = currentHp;
            }

            if (currentHp != _lastHp)
            {
                _startHp = _lastHp;
                _targetHp = currentHp;
            }

            if (_startHp.HasValue && _targetHp.HasValue)
            {
                float delta = _targetHp.Value - _startHp.Value;
                float offset = delta * velocity / 100f;
                _startHp = Math.Clamp(_startHp.Value + offset, 0, maxHp);
            }

            _lastHp = currentHp;
            return _startHp.HasValue ? (uint)_startHp.Value : (uint)currentHp;
        }
    }
}
