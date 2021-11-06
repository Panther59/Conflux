namespace Conflux.Graphql.Wrappers
{
	using GraphQL.Types;

	public class InputObjectGraphTypeWrapper<T> : InputObjectGraphType, IIgnore
	{
		public InputObjectGraphTypeWrapper()
		{
			//ObjectGraphTypeBuilder.Build(this, typeof(T));
			Name = "Input_" + Name;
		}

		public void Build(IObjectGraphTypeBuilder builder)
		{
			builder.Build(this, typeof(T));
		}
	}
}
