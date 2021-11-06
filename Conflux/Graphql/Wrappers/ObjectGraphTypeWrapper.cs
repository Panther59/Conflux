
namespace Conflux.Graphql.Wrappers
{
	using GraphQL.Types;

	public class ObjectGraphTypeWrapper<T> : ObjectGraphType, IIgnore
	{
		public ObjectGraphTypeWrapper()
		{
		}

		public void Build(IObjectGraphTypeBuilder builder)
		{
			builder.Build(this, typeof(T));
		}
	}
}
