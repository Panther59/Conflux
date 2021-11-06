namespace Conflux.Graphql.Wrappers
{
	using GraphQL.Types;

	public class InterfaceGraphTypeWrapper<T> : InterfaceGraphType, IIgnore
	{
		public InterfaceGraphTypeWrapper()
		{
			//ObjectGraphTypeBuilder.Build(this, typeof(T));
		}

		public void Build(IObjectGraphTypeBuilder builder)
		{
			builder.Build(this, typeof(T));
		}
	}
}
