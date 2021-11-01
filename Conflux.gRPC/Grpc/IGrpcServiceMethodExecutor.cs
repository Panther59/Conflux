using GraphQL;
using GraphQL.SchemaGenerator.Models;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Conflux.gRPC.Grpc
{
	public interface IGrpcServiceMethodExecutor
	{
		Task<object> ExecuteServiceMethodAsync(IResolveFieldContext<object> context, FieldInformation field);
	}
}