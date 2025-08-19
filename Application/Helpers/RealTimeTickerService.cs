using TheEye.Application.Interfaces;

public class RealTimeTickerService : IHostedService, IDisposable
{
    private readonly IEyeSimulator _simulator;
    private readonly ILogger<RealTimeTickerService> _logger;
    private Timer? _timer;

    // Game time conversion: 90 real-world minutes per simulated day.
    private const double RealSecondsPerSimulatedDay = 90 * 60;
    private const double SimulatedHoursPerRealSecond = 24.0 / RealSecondsPerSimulatedDay;

    public RealTimeTickerService(IEyeSimulator simulator, ILogger<RealTimeTickerService> logger)
    {
        _simulator = simulator;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Real-time Eye ticker service is starting.");

        // Schedule timer to start after a small delay to allow the app to fully initialize.
        _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

        return Task.CompletedTask;
    }

    /// <summary>
    ///  Updates the hours based on the real time elapsed since the last update.    
    /// </summary>
    /// <param name="state"></param>
    private void DoWork(object? state)
    {
        try
        {
            var currentState = _simulator.GetStateSnapshot();

            if (currentState.Paused)
            {
                return; // Don't advance if paused
            }

            var now = DateTime.UtcNow;
            var lastUpdate = currentState.LastUpdated;

            // This check is vital to prevent advancing time if another process just did.
            if (now <= lastUpdate) return;

            var elapsedRealSeconds = (now - lastUpdate).TotalSeconds;

            if (elapsedRealSeconds > 0)
            {
                double hoursToAdvance = elapsedRealSeconds * SimulatedHoursPerRealSecond;
                _simulator.AdvanceHours(hoursToAdvance);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in RealTimeTickerService work loop.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Real-time Eye ticker service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}