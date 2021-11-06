namespace Conflux.gRPC
{
	using Conflux.Graphql;
	using Conflux.Graphql.Models;
	using Conflux.Graphql.Types;
	using Conflux.Graphql.Wrappers;
	using GraphQL.Types;
	using System;

	public class AdditionalGraphTypeConverter : IAdditionalGraphTypeConverter
	{
		public Type BaseGraphType(Type propertyType, bool isInputType, Func<Type, RequiredType, bool, Type> orifiginalConverter)
		{
			if (propertyType == typeof(Google.Protobuf.WellKnownTypes.Enum) || propertyType == typeof(Google.Protobuf.WellKnownTypes.EnumValue))
			{
				return typeof(EnumerationGraphType);
			}

			if (propertyType == typeof(Google.Protobuf.ByteString) || propertyType == typeof(Google.Protobuf.WellKnownTypes.StringValue))
			{
				return typeof(StringGraphType);
			}

			if (propertyType == typeof(Google.Protobuf.WellKnownTypes.Int32Value))
			{
				return typeof(IntGraphType);
			}

			if (propertyType == typeof(Google.Protobuf.WellKnownTypes.DoubleValue))
			{
				return typeof(DecimalGraphType);
			}

			if (propertyType == typeof(Google.Protobuf.WellKnownTypes.BoolValue))
			{
				return typeof(BooleanGraphType);
			}

			if (propertyType == typeof(Google.Protobuf.WellKnownTypes.Timestamp))
			{
				return typeof(OriginalDateGraphType);
			}

			if (propertyType == typeof(Google.Protobuf.WellKnownTypes.BytesValue))
			{
				return typeof(ByteArrayGraphType);
			}

			if (propertyType == typeof(Google.Protobuf.Collections.RepeatedField<>))
			{
				var itemType = propertyType.GetElementType();
				var itemGraphType = orifiginalConverter(itemType, RequiredType.Default, isInputType);
				if (itemGraphType != null)
				{
					return typeof(ListGraphType<>).MakeGenericType(itemGraphType);
				}
			}

			return null;
		}
	}
}
