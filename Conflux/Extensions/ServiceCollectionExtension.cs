namespace Conflux.Extensions
{
	using Microsoft.Extensions.DependencyInjection;

	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddConflux(this IServiceCollection services)
		{
			return services;
		}
	}
}
