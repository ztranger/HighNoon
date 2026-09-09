namespace HighNoon
{
    /// <summary>
    /// Abstraction over "a duelist pulled the trigger". Both human taps and bot
    /// timers implement this, so <see cref="DuelManager"/> resolves them identically.
    /// </summary>
    public interface IDuelInput
    {
        /// <summary>Begin listening (called when the hidden tension countdown starts).</summary>
        void Arm();

        /// <summary>Notified of the exact BANG moment (bots schedule their reaction from here).</summary>
        void OnBang(double bangTimeRealtime);

        /// <summary>
        /// Polled every frame. Humans stamp the Input System event time; bots stamp
        /// their scheduled instant. <paramref name="nowRealtime"/> is the poll clock
        /// (and the human fallback if the event time is missing).
        /// </summary>
        void Tick(double nowRealtime);

        bool HasFired { get; }

        /// <summary>Realtime seconds when the trigger was pulled.</summary>
        double FireTimeRealtime { get; }

        void ResetInput();
    }
}
