namespace Conflux.Graphql
{
	using Conflux.Graphql.Models;
	using System;

	public interface IAdditionalGraphTypeConverter
	{
		Type BaseGraphType(Type propertyType, bool isInputType, Func<Type, RequiredType, bool, Type> orifiginalConverter);
	}
}
