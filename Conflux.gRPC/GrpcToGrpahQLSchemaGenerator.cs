namespace GraphQL.SchemaGenerator
{
	using Conflux;
	using Conflux.Graphql;
	using Conflux.Graphql.Helpers;
	using Conflux.Graphql.Models;
	using Conflux.Graphql.Wrappers;
	using Conflux.gRPC.Extensions;
	using Conflux.gRPC.Grpc;
	using Google.Protobuf.Reflection;
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
	public class GrpcToGrpahQLSchemaGenerator : ISchemaGenerator
	{
		private readonly IGraphTypeConverter graphTypeConverter;
		private readonly IObjectGraphTypeBuilder objectGraphTypeBuilder;
		private readonly IGrpcServiceMethodExecutor grpcServiceMethodExecutor;

		public GrpcToGrpahQLSchemaGenerator(
			IServiceProvider serviceProvider,
			IGraphTypeResolver graphTypeResolver,
			IGraphTypeConverter graphTypeConverter,
			IObjectGraphTypeBuilder objectGraphTypeBuilder,
			IGrpcServiceMethodExecutor grpcServiceMethodExecutor)
		{
			ServiceProvider = serviceProvider;
			this.grpcServiceMethodExecutor = grpcServiceMethodExecutor;
			TypeResolver = graphTypeResolver;
			this.graphTypeConverter = graphTypeConverter;
			this.objectGraphTypeBuilder = objectGraphTypeBuilder;
		}

		private IServiceProvider ServiceProvider { get; }

		private IGraphTypeResolver TypeResolver { get; }

		/// <summary>
		///     Dynamically create query arguments.
		/// </summary>
		public GraphQLQueryArguments CreateArguments(MessageDescriptor parameters)
		{
			var arguments = new List<GraphQLQueryArgument>();

			//foreach (var parameter in parameters)
			//{
			var argument = CreateArgument(parameters);
			arguments.Add(argument);
			//}

			return new GraphQLQueryArguments(arguments);
		}

		/// <summary>
		///     Ensure graph type. Can return null if the type can't be used.
		/// </summary>
		/// <param name="parameterType"></param>
		/// <returns></returns>
		public Type EnsureGraphType(Type parameterType)
		{
			if (parameterType == null || parameterType == typeof(void))
				return typeof(StringGraphType);

			if (typeof(GraphType).IsAssignableFrom(parameterType))
				return parameterType;

			var type = this.graphTypeConverter.ConvertTypeToGraphType(parameterType);

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
				var serviceDescriptor = grpcType.GetServiceDescriptor();
				var methodDescriptors = serviceDescriptor.Methods
					.Where(x => !x.GetIsInternalOnly())
					.ToArray();

				foreach (var method in methodDescriptors)
				{
					var parameters = method.InputType;
					var arguments = CreateArguments(method.InputType);
					var response = method.OutputType.ClrType;

					if (response.IsGenericType && response.GetGenericTypeDefinition() == typeof(Task<>))
						response = response.GenericTypeArguments.First();

					var field = new FieldInformation
					{
						ServiceName = name,
						IsMutation = method.GetMethodType() == RpcMethodOptions.Types.MethodType.Mutation,
						Arguments = arguments,
						Name =
							!string.IsNullOrWhiteSpace(method.Name)
								? method.Name
								: StringHelper.ConvertToCamelCase(method.Name),
						Response = response,
						//Method = method,
						Grpc = grpcType,
						//ObsoleteReason = TypeHelper.GetDeprecationReason(method)
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
			string name,
			IEnumerable<FieldDefinition> definitions)
		{
			var mutation = new ObjectGraphType
			{
				Name = $"{name}Mutations"
			};
			var query = new ObjectGraphType
			{
				Name = $"{name}Queries"
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
						definition.Field.Name,
						definition.Field.Arguments.GetQueryArguments(),
						definition.Resolve,
						definition.Field.ObsoleteReason);
				else
					query.FieldAsync(
						type,
						definition.Field.Name,
						definition.Field.Name,
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
			return CreateSchema(name, CreateDefinitions(name, types));
		}

		/// <summary>
		///     Resolve the value from a field.
		/// </summary>
		public async Task<object> ResolveField(IResolveFieldContext<object> context, FieldInformation field)
		{
			return await this.grpcServiceMethodExecutor.ExecuteServiceMethodAsync(context, field);
		}

		private GraphQLQueryArgument CreateArgument(MessageDescriptor parameter)
		{
			var requestArgumentType = GetGraphQLArgumentType(parameter.ClrType);
			var inner = Activator.CreateInstance(requestArgumentType) as IGraphType;
			if (requestArgumentType.GetGenericTypeDefinition() == typeof(InputObjectGraphTypeWrapper<>))
			{
				((dynamic)inner).Build(this.objectGraphTypeBuilder);
			}
			var argument = new GraphQLQueryArgument(inner, parameter.ClrType);
			argument.Name = parameter.Name;

			return argument;
		}

		private Type GetGraphQLArgumentType(Type parameterType)
		{
			var requestType = this.graphTypeConverter.ConvertTypeToGraphType(parameterType, RequiredType.NotRequired, true);

			return requestType;
		}

		private Type GetRequestArgumentType(Type parameterType)
		{
			var requestType = this.graphTypeConverter.ConvertTypeToGraphType(parameterType, RequiredType.NotRequired, true);
			var requestArgumentType = typeof(QueryArgument<>).MakeGenericType(requestType);

			return requestArgumentType;
		}
	}
}
