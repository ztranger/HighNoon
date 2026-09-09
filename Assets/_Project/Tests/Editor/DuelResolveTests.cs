using System.Collections.Generic;
using NUnit.Framework;

namespace HighNoon
{
    public class DuelResolveTests
    {
        [Test]
        public void TryPickLaneWinner_EarlierFireWins()
        {
            var a = Fired("A", 1.10);
            var b = Fired("B", 1.20);
            Assert.IsTrue(DuelResolve.TryPickLaneWinner(new List<Duelist> { a, b }, 1.0, out var winner));
            Assert.AreSame(a, winner);
        }

        [Test]
        public void TryPickLaneWinner_EqualTimesAreDraw()
        {
            var a = Fired("A", 1.15);
            var b = Fired("B", 1.15);
            Assert.IsTrue(DuelResolve.TryPickLaneWinner(new List<Duelist> { a, b }, 1.0, out var winner));
            Assert.IsNull(winner);
        }

        [Test]
        public void TryPickLaneWinner_ListOrderDoesNotBreakATie()
        {
            var bottom = Fired("Bottom", 1.15);
            var top = Fired("Top", 1.15);
            Assert.IsTrue(DuelResolve.TryPickLaneWinner(new List<Duelist> { bottom, top }, 1.0, out var winner));
            Assert.IsNull(winner);
        }

        [Test]
        public void TryPickLaneWinner_PreBangFireIsIgnored()
        {
            var jumper = Fired("Jump", 0.5);
            Assert.IsFalse(DuelResolve.TryPickLaneWinner(new List<Duelist> { jumper }, 1.0, out var winner));
            Assert.IsNull(winner);
        }

        [Test]
        public void TryPickLaneWinner_NobodyFired_ReturnsFalse()
        {
            var idle = new Duelist { Label = "Idle", Input = new StubInput() };
            Assert.IsFalse(DuelResolve.TryPickLaneWinner(new List<Duelist> { idle }, 1.0, out var winner));
            Assert.IsNull(winner);
        }

        [Test]
        public void PickTimingWinner_ClosestToCentreWins()
        {
            var a = Aimed("A", 0.04f);
            var b = Aimed("B", 0.11f);
            Assert.AreSame(a, DuelResolve.PickTimingWinner(new List<Duelist> { a, b }));
        }

        [Test]
        public void PickTimingWinner_ExactTieIsDraw()
        {
            var a = Aimed("A", 0.08f);
            var b = Aimed("B", 0.08f);
            Assert.IsNull(DuelResolve.PickTimingWinner(new List<Duelist> { a, b }));
        }

        static Duelist Fired(string label, double t) =>
            new Duelist { Label = label, Input = new StubInput { HasFired = true, FireTimeRealtime = t } };

        static Duelist Aimed(string label, float err) =>
            new Duelist { Label = label, AimError = err };

        class StubInput : IDuelInput
        {
            public bool HasFired { get; set; }
            public double FireTimeRealtime { get; set; }
            public void Arm() { }
            public void OnBang(double bangTimeRealtime) { }
            public void Tick(double nowRealtime) { }
            public void ResetInput() { HasFired = false; FireTimeRealtime = 0; }
        }
    }
}
