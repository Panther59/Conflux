namespace GraphQL.SchemaGenerator
{
	using Conflux.gRPC.Grpc;
	using GraphQL.SchemaGenerator.Extensions;
	using GraphQL.SchemaGenerator.Helpers;
	using GraphQL.SchemaGenerator.Models;
	using GraphQL.Types;
	using Grpc.Core;
	using Grpc.Net.Client;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Net.Http;
	using System.Reflection;
	using System.Threading.Tasks;

	/// <summary>
	///     Dynamically provides graph ql schema information.
	/// </summary>
	public class SchemaGenerator : ISchemaGenerator
	{
		private readonly IGrpcServiceMethodExecutor grpcServiceMethodExecutor;

		public SchemaGenerator(IServiceProvider serviceProvider, IGrpcServiceMethodExecutor grpcServiceMethodExecutor)
		{
			ServiceProvider = serviceProvider;
			this.grpcServiceMethodExecutor = grpcServiceMethodExecutor;
			TypeResolver = new GraphTypeResolver();
		}

		private IServiceProvider ServiceProvider { get; }

		private IGraphTypeResolver TypeResolver { get; }

		/// <summary>
		///     Dynamically create query arguments.
		/// </summary>
		public static GraphQLQueryArguments CreateArguments(IEnumerable<ParameterInfo> parameters)
		{
			var arguments = new List<GraphQLQueryArgument>();

			foreach (var parameter in parameters)
			{
				var argument = CreateArgument(parameter);
				arguments.Add(argument);
			}

			return new GraphQLQueryArguments(arguments);
		}

		/// <summary>
		///     Ensure graph type. Can return null if the type can't be used.
		/// </summary>
		/// <param name="parameterType"></param>
		/// <returns></returns>
		public static Type EnsureGraphType(Type parameterType)
		{
			if (parameterType == null || parameterType == typeof(void))
				return typeof(StringGraphType);

			if (typeof(GraphType).IsAssignableFrom(parameterType))
				return parameterType;

			var type = GraphTypeConverter.ConvertTypeToGraphType(parameterType);

			if (type == null)
				type = typeof(ScalarGraphType);

			return type;
		}

		/// <summary>
		///     Create field definitions based off a type.
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		public IEnumerable<FieldDefinition> CreateDefinitions(string name, params Type[] types)
		{
			var definitions = new List<FieldDefinition>();
			var commonMethods = typeof(object).GetMethods();
			foreach (var grpcType in types)
			{
				var type = this.ExtractBaseType(grpcType, name);
				foreach (var method in type.GetMethods())
				{
					if (commonMethods.Any(x => x.Name == method.Name))
					{
						continue;
					}
					//var graphRoute = method.GetCustomAttributes(typeof(GraphRouteAttribute), true)
					//    .OfType<GraphRouteAttribute>()
					//    .FirstOrDefault();

					//if (graphRoute == null)
					//    continue;

					var parameters = method.GetParameters();
					var arguments = CreateArguments(parameters.Take(1));
					var response = method.ReturnType;

					if (response.IsGenericType && response.GetGenericTypeDefinition() == typeof(Task<>))
						response = response.GenericTypeArguments.First();

					var field = new FieldInformation
					{
						ServiceName = name,
						IsMutation = false,
						Arguments = arguments,
						Name =
							!string.IsNullOrWhiteSpace(method.Name)
								? method.Name
								: StringHelper.ConvertToCamelCase(method.Name),
						Response = response,
						Method = method,
						Grpc = grpcType,
						ObsoleteReason = TypeHelper.GetDeprecationReason(method)
					};

					var definition = new FieldDefinition(field, context => ResolveField(context, field));

					definitions.Add(definition);
				}
			}

			return definitions;
		}

		/// <summary>
		///     Create schema from the field definitions.
		/// </summary>
		public GraphQL.Types.Schema CreateSchema(
			IEnumerable<FieldDefinition> definitions)
		{
			var mutation = new ObjectGraphType
			{
				Name = "RootMutations"
			};
			var query = new ObjectGraphType
			{
				Name = "RootQueries"
			};

			foreach (var definition in definitions)
			{
				if (definition.Field == null)
					continue;

				var type = EnsureGraphType(definition.Field.Response);

				if (definition.Field.IsMutation)
					mutation.FieldAsync(
						type,
						definition.Field.Name,
						TypeHelper.GetDescription(definition.Field.Method),
						definition.Field.Arguments.GetQueryArguments(),
						definition.Resolve,
						definition.Field.ObsoleteReason);
				else
					query.FieldAsync(
						type,
						definition.Field.Name,
						TypeHelper.GetDescription(definition.Field.Method),
						definition.Field.Arguments.GetQueryArguments(),
						definition.Resolve,
						definition.Field.ObsoleteReason);
			}

			var schema = new GraphQL.Types.Schema(TypeResolver)
			{
				Mutation = mutation.Fields.Any() ? mutation : null,
				Query = query.Fields.Any() ? query : null
			};

			return schema;
		}

		/// <summary>
		///     Helper method to create schema from types.
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		public GraphQL.Types.Schema CreateSchema(string name, params Type[] types)
		{
			return CreateSchema(CreateDefinitions(name, types));
		}

		/// <summary>
		///     Resolve the value from a field.
		/// </summary>
		public async Task<object> ResolveField(IResolveFieldContext<object> context, FieldInformation field)
		{
			return await this.grpcServiceMethodExecutor.ExecuteServiceMethodAsync(context, field);
		}

		private static GraphQLQueryArgument CreateArgument(ParameterInfo parameter)
		{
			var requestArgumentType = GetGraphQLArgumentType(parameter.ParameterType);
			var inner = Activator.CreateInstance(requestArgumentType) as IGraphType;
			var argument = new GraphQLQueryArgument(inner, parameter.ParameterType);
			argument.Name = parameter.Name;

			return argument;
		}

		private static Type GetGraphQLArgumentType(Type parameterType)
		{
			var requestType = GraphTypeConverter.ConvertTypeToGraphType(parameterType, RequiredType.NotRequired, true);

			return requestType;
		}

		private static Type GetRequestArgumentType(Type parameterType)
		{
			var requestType = GraphTypeConverter.ConvertTypeToGraphType(parameterType, RequiredType.NotRequired, true);
			var requestArgumentType = typeof(QueryArgument<>).MakeGenericType(requestType);

			return requestArgumentType;
		}

		private Type ExtractBaseType(Type grpcType, string name)
		{
			var type = grpcType.GetNestedType($"{name}Base");
			return type;
		}
	}
}
