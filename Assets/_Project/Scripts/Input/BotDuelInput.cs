using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// An AI duelist. On BANG it schedules a fire at bang + a random reaction
    /// drawn from its <see cref="BotConfig"/>. Never fires before BANG in v1.
    /// </summary>
    public class BotDuelInput : IDuelInput
    {
        readonly BotConfig _config;
        bool _armed;
        bool _fired;
        double _fireTime;
        double _scheduledFire = double.MaxValue;

        public BotDuelInput(BotConfig config) { _config = config; }

        public bool HasFired => _fired;
        public double FireTimeRealtime => _fireTime;

        public void Arm()
        {
            _armed = true;
            _fired = false;
            _scheduledFire = double.MaxValue;
        }

        public void ResetInput()
        {
            _armed = false;
            _fired = false;
            _fireTime = 0;
            _scheduledFire = double.MaxValue;
        }

        public void OnBang(double bangTimeRealtime)
        {
            float reaction = _config != null ? _config.RollReaction() : 0.35f;
            _scheduledFire = bangTimeRealtime + reaction;
        }

        public void Tick(double nowRealtime)
        {
            if (!_armed || _fired) return;
            if (nowRealtime >= _scheduledFire)
            {
                _fired = true;
                _fireTime = _scheduledFire; // exact scheduled instant, not frame-quantized
            }
        }
    }
}
