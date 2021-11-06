namespace Conflux.gRPC.Grpc
{
	using Conflux.Graphql.Models;
	using GraphQL;
	using System.Threading.Tasks;

	public interface IGrpcServiceMethodExecutor
	{
		Task<object> ExecuteServiceMethodAsync(IResolveFieldContext<object> context, FieldInformation field);
	}
}
