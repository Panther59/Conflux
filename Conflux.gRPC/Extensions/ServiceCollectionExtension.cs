namespace Conflux.gRPC
{
	using GraphQL.SchemaGenerator;
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

	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddConfluxGRPC(this IServiceCollection services, System.Type type)
		{
			//var typeAdapter = new GraphTypeAdapter();
			//var constructor = new SchemaConstructor<ISchema, IGraphType>(typeAdapter);
			SchemaGenerator schemaGenerator = new SchemaGenerator(services.BuildServiceProvider());
			var schema = schemaGenerator.CreateSchema(type);

			// add execution components
			services.AddGraphQL()
				.AddSystemTextJson()
				.AddSchemaWithType(schema)           
				.ConfigureSchema((schema, serviceProvider) =>
				{
					// install middleware only when the custom EnableMetrics option is set
					var middlewares = serviceProvider.GetRequiredService<IEnumerable<IFieldMiddleware>>();
					foreach (var middleware in middlewares)
						schema.FieldMiddleware.Use(middleware);
				});

			//// writes all the property names
			//foreach (MethodInfo methodInfo in methodInfos)
			//{
			//	Console.WriteLine($"{methodInfo.Name}");
			//	//Response.Write(methodInfo.Name + "<br/>");
			//}
			//var schema = constructor.Build(typeof(SchemaDefinition<GraphQl.Query, GraphQl.Mutation, GraphQl.Subscription>));
			//var graphQLEngine = new GraphQLEngine()
			//	.WithFieldResolutionStrategy(FieldResolutionStrategy.Normal)
			//	.WithQuery<GraphQl.Query>()
			//	.WithMutation<GraphQl.Mutation>()
			//	.WithSubscription<GraphQl.Subscription>()
			//	.BuildSchema();
			return services;
		}

		private static IGraphQLBuilder AddSchemaWithType(this IGraphQLBuilder builder, ISchema schema)
		{
			if (schema != null)
			{
				builder.AddSchema(schema);
			}

			return builder;
		}

		public static IApplicationBuilder UseConfluxGRPC(this IApplicationBuilder app)
		{
			app.UseMiddleware<GraphQLMiddleware>();
			app.UseGraphQLPlayground("/ui/playground");

			return app;
		}
	}
}
