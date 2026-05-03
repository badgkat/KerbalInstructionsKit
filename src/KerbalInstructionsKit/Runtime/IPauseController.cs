namespace KerbalInstructionsKit.Runtime
{
    public interface IPauseController
    {
        bool IsAvailable { get; }
        bool IsPaused { get; }
        void SetPaused(bool paused);
    }

    public sealed class NullPauseController : IPauseController
    {
        public bool IsAvailable => false;
        public bool IsPaused => false;
        public void SetPaused(bool paused) { }
    }

    public sealed class FlightPauseController : IPauseController
    {
        public bool IsAvailable => true;
        public bool IsPaused => FlightDriver.Pause;
        public void SetPaused(bool paused) => FlightDriver.SetPause(paused);
    }
}
