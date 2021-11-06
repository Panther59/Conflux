namespace Conflux.Graphql.Wrappers
{
	using Conflux.Graphql.Schema;
	using GraphQL.Types;

	public class EnumerationGraphTypeWrapper<T> : EnumerationGraphType, IIgnore
	{
		public EnumerationGraphTypeWrapper()
		{
			new SchemaBuilder().BuildEnum(this, typeof(T));
		}
	}
}
