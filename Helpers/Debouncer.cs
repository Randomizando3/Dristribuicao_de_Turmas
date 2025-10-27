using System;
using System.Windows.Threading;

namespace DistribuicaoTurmas.Helpers
{
    // Executa a ação apenas após um intervalo sem novas chamadas
    public class Debouncer
    {
        private readonly DispatcherTimer _timer;
        private Action _action;

        public Debouncer(int milliseconds = 400)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(milliseconds) };
            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                _action?.Invoke();
            };
        }

        public void Run(Action action)
        {
            _action = action;
            _timer.Stop();
            _timer.Start();
        }
    }
}
