namespace Conflux.gRPC.Extensions
{
	using Google.Protobuf.Reflection;
	using static RpcMethodOptions.Types;

	public static class DescriptorExtensions
	{
		public static bool GetIsInternalOnly(this MethodDescriptor methodDescriptor) =>
			GetRpcMethodOptions(methodDescriptor).IsInternalOnly;

		public static MethodType GetMethodType(this MethodDescriptor methodDescriptor) =>
			GetRpcMethodOptions(methodDescriptor).MethodType;

		public static RpcFieldOptions GetRpcFieldOptions(this FieldDescriptor fieldDescriptor) =>
			fieldDescriptor
			.GetOptions()?
			.GetExtension(CustomOptionsExtensions.RpcFieldOptions) ?? new RpcFieldOptions();

		public static RpcMethodOptions GetRpcMethodOptions(this MethodDescriptor methodDescriptor) =>
			methodDescriptor
			.GetOptions()?
			.GetExtension(CustomOptionsExtensions.RpcMethodOptions) ?? new RpcMethodOptions();
	}
}
