namespace Conflux.Graphql
{
	using Conflux.Graphql.Models;
	using GraphQL;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public interface ISchemaGenerator
	{
		IEnumerable<FieldDefinition> CreateDefinitions(string name, params Type[] types);

		GraphQL.Types.Schema CreateSchema(string name, params Type[] types);

		Task<object> ResolveField(IResolveFieldContext<object> context, FieldInformation field);
	}
}
