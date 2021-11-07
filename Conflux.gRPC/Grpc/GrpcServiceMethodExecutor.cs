namespace Conflux.gRPC.Grpc
{
	using Conflux.Graphql.Models;
	using global::Grpc.Core;
	using global::Grpc.Net.Client;
	using GraphQL;
	using System;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Conflux.Graphql.Extensions;

	public class GrpcServiceMethodExecutor : IGrpcServiceMethodExecutor
	{
		private readonly IHttpClientFactory httpClientFactory;

		public GrpcServiceMethodExecutor(IHttpClientFactory httpClientFactory)
		{
			this.httpClientFactory = httpClientFactory;
		}

		public async Task<object> ExecuteServiceMethodAsync(IResolveFieldContext<object> context, FieldInformation field)
		{
			var callOPtions = new CallOptions();
			object input = null;
			if (field.Arguments.Count > 0)
			{
				input = this.GetGrpcRequestObject(context, field);
			}

			if (input != null)
			{
				
			}

			var grpcMethodExecutor = this.MethodExecutor(field);
			var parameters = context.Parameters(field);
			try
			{
				var req = Activator.CreateInstance(field.Arguments[0].BaseType);
				var resultTask = grpcMethodExecutor.Invoke(input, callOPtions);
				await resultTask.ConfigureAwait(false);

				//var result = field.Method.Invoke(classObject, parameters);

				return ((dynamic)resultTask).Result;
			}
			catch (Exception ex)
			{
				var stringParams = parameters?.ToList().Select(t => string.Concat(t.ToString(), ":"));

				throw new Exception($"Cant invoke {field.Name} with parameters {stringParams}", ex);
			}
		}

		private object GetGrpcRequestObject(IResolveFieldContext<object> context, FieldInformation field)
		{
			var input = context.GetArgument(field.Arguments[0].BaseType, field.Arguments[0].Name);

			if (input == null)
				throw new Exception($"Can't resolve class from: {field.Arguments[0].BaseType}");

			return input;
		}

		private Func<object, CallOptions, Task> MethodExecutor(FieldInformation field)
		{
			var grpcClientType = field.Grpc.GetNestedType($"{field.ServiceName}Client");
			var httpClient = httpClientFactory.CreateClient(field.ServiceName);

			var grpcChannel = GrpcChannel.ForAddress(
				httpClient.BaseAddress,
				new GrpcChannelOptions
				{
					HttpClient = httpClient,
					MaxSendMessageSize = 50 * 1024 * 1024,
					MaxReceiveMessageSize = 50 * 1024 * 1024,
				});

			var grpcClient = (ClientBase)Activator.CreateInstance(grpcClientType, grpcChannel);
			var grpcMethod = grpcClient.GetType().GetMethod($"{field.Name}Async", new[] { field.Arguments[0].BaseType, typeof(CallOptions) });
			var grpcClientExp = Expression.Constant(grpcClient);
			var requestParamExp = Expression.Parameter(typeof(object));
			var castRequestParamExp = Expression.Convert(requestParamExp, field.Arguments[0].BaseType);
			var optionsParamExp = Expression.Parameter(typeof(CallOptions));

			var grpcMethodExecutor = Expression.Lambda<Func<object, CallOptions, Task>>(
				Expression.Property(
					Expression.Call(grpcClientExp, grpcMethod, castRequestParamExp, optionsParamExp),
					nameof(AsyncUnaryCall<object>.ResponseAsync)),
				requestParamExp,
				optionsParamExp).Compile();

			return grpcMethodExecutor;
		}
	}
}
