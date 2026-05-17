using System;
using System.Timers;
using System.Threading.Tasks;

namespace DesktopGremlin.Services
{
    public class FactPoller
    {
        public event EventHandler<string>? FactReceived;
        private readonly FactService _factService;
        private readonly System.Timers.Timer _timer;
        private readonly Random _random = new Random();

        public FactPoller(FactService factService)
        {
            _factService = factService;
            _timer = new System.Timers.Timer();
            _timer.AutoReset = false;
            _timer.Elapsed += TimerElapsed;
        }

        public void Start()
        {
            ScheduleNext();
        }

        private void ScheduleNext()
        {
            var seconds = _random.Next(40, 70);
            _timer.Interval = seconds * 1000;
            _timer.Start();
        }

        private async void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                var fact = await _factService.GetFact();
                var text = fact?.Fact ?? string.Empty;
                try { FactReceived?.Invoke(this, text); } catch { }
            }
            catch { }
            finally
            {
                ScheduleNext();
            }
        }
    }
}
