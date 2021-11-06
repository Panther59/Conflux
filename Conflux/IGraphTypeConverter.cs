namespace Conflux
{
	using Conflux.Graphql.Models;
	using System;

	public interface IGraphTypeConverter
	{
		Type ConvertTypeToGraphType(Type propertyType, RequiredType requiredType = RequiredType.Default, bool isInputType = false);
	}
}
