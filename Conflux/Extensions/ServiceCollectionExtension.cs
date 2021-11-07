namespace Conflux.Extensions
{
	using Conflux.Graphql;
	using GraphQL;
	using GraphQL.Instrumentation;
	using GraphQL.MicrosoftDI;
	using GraphQL.SystemTextJson;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.Extensions.DependencyInjection;
	using System.Collections.Generic;
	using System.Linq;

	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddAdditionalGraphTypeConverter<T>(this IServiceCollection services)
			where T : class, IAdditionalGraphTypeConverter
		{
			var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IAdditionalGraphTypeConverter));
			if (serviceDescriptor != null)
			{
				services.Remove(serviceDescriptor);
			}

			services.AddSingleton<IAdditionalGraphTypeConverter, T>();
			return services;
		}

		public static IServiceCollection AddConflux(this IServiceCollection services)
		{
			services.AddSingleton<IObjectGraphTypeBuilder, ObjectGraphTypeBuilder>();
			services.AddSingleton<IGraphTypeResolver, GraphTypeResolver>();
			services.AddSingleton<IObjectGraphTypeBuilder, ObjectGraphTypeBuilder>();
			services.AddSingleton<IGraphTypeConverter, GraphTypeConverter>();
			services.AddGraphQL()
				.AddSystemTextJson()
				.ConfigureSchema((schema, serviceProvider) =>
				{
					// install middleware only when the custom EnableMetrics option is set
					var middlewares = serviceProvider.GetRequiredService<IEnumerable<IFieldMiddleware>>();
					foreach (var middleware in middlewares)
						schema.FieldMiddleware.Use(middleware);
				});

			var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IAdditionalGraphTypeConverter));
			if (serviceDescriptor == null)
			{
				services.AddSingleton<IAdditionalGraphTypeConverter, AdditionalGraphTypeConverter>();
			}

			return services;
		}
	}
}
