using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace Sample.Service
{
	public class MassTransitConsoleHostedService : IHostedService
	{
		private readonly IBusControl _bus;

		public MassTransitConsoleHostedService(IBusControl bus)
		{
			_bus = bus;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _bus.StopAsync(cancellationToken);
		}
	}
}