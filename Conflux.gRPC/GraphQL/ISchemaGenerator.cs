using GraphQL.SchemaGenerator.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphQL.SchemaGenerator
{
	public interface ISchemaGenerator
	{
		IEnumerable<FieldDefinition> CreateDefinitions(string name, params Type[] types);
		GraphQL.Types.Schema CreateSchema(string name, params Type[] types);
		Task<object> ResolveField(IResolveFieldContext<object> context, FieldInformation field);
	}
}