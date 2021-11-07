
namespace Conflux.gRPC.Extensions
{
	using Google.Protobuf.Reflection;
	using System;
	using System.Reflection;

	public static class ReflectionExtensions
	{
		public static ServiceDescriptor GetServiceDescriptor(this Type type) => GetDescriptor<ServiceDescriptor>(type);
		public static MessageDescriptor GetMessageDescriptor(this Type type) => GetDescriptor<MessageDescriptor>(type);
		public static FieldDescriptor GetFieldDescriptor(this PropertyInfo propertyInfo)
		{
			var messageDescriptor = GetMessageDescriptor(propertyInfo.DeclaringType!);

			var fieldNumberProperty = propertyInfo.DeclaringType!.GetField(
				$"{propertyInfo.Name}FieldNumber",
				BindingFlags.Public | BindingFlags.Static);

			if (fieldNumberProperty == null)
			{
				throw new ArgumentException($"Property {propertyInfo.Name} is not a valid Protobuf property", nameof(propertyInfo));
			}

			var fieldNumber = (int)fieldNumberProperty.GetValue(null)!;
			return messageDescriptor.FindFieldByNumber(fieldNumber);
		}

		private static T GetDescriptor<T>(Type type)
		{
			var descriptorProperty = type.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static);
			if (descriptorProperty == null)
			{
				throw new ArgumentException($"Type {type.Name} is not a valid Protobuf type", nameof(type));
			}

			return (T)descriptorProperty.GetValue(null);
		}
	}
}
