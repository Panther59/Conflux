namespace Conflux.gRPC
{
	using Microsoft.Extensions.DependencyInjection;
	using System;
	using System.Reflection;
	using Microsoft.AspNetCore.Builder;
	using GraphQL.Types;
	using GraphQL.Server;
	using GraphQL.MicrosoftDI;
	using GraphQL.SystemTextJson;
	using GraphQL;
	using Microsoft.Extensions.Options;
	using System.Collections.Generic;
	using GraphQL.Instrumentation;
	using Example;
	using GraphQL.DI;
	using Conflux.gRPC.Grpc;
	using GraphQL.SchemaGenerator;
	using Conflux.Graphql;
	using Conflux.Extensions;

	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddConfluxGRPC(this IServiceCollection services, GrpcService service)
		{
			services.AddSingleton<ISchemaGenerator, GrpcToGrpahQLSchemaGenerator>();
			services.AddSingleton<IGrpcServiceMethodExecutor, GrpcServiceMethodExecutor>();
			services.AddAdditionalGraphTypeConverter<AdditionalGraphTypeConverter>();
			// add execution components
			services.AddGraphQL()
				.AddSystemTextJson()
				.AddSchemaWithType(service)
				.ConfigureSchema((schema, serviceProvider) =>
				{
					// install middleware only when the custom EnableMetrics option is set
					var middlewares = serviceProvider.GetRequiredService<IEnumerable<IFieldMiddleware>>();
					foreach (var middleware in middlewares)
						schema.FieldMiddleware.Use(middleware);
				});

			return services;
		}

		private static IGraphQLBuilder AddSchemaWithType(this IGraphQLBuilder builder, GrpcService service)
		{
			builder.AddSchema<ISchema>((serviceProvider) =>
			{
				var schemaGenerator = serviceProvider.GetService<ISchemaGenerator>();
				var schema = schemaGenerator.CreateSchema(service.Name, service.Type);
				return schema;
			}, GraphQL.DI.ServiceLifetime.Transient);
			return builder;
		}

		public static IApplicationBuilder UseConfluxGRPC(this IApplicationBuilder app)
		{
			app.UseMiddleware<GraphQLMiddleware>();
			app.UseGraphQLPlayground("/playground");

			return app;
		}
	}
}
