using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Conflux.gRPC
{
	public class SchemaBuilderService : IHostedService
	{
		private readonly IServiceProvider serviceProvider;

		public SchemaBuilderService(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
