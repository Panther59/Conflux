namespace Conflux.Graphql.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
	public class GraphTypeAttribute : Attribute
	{
		public GraphTypeAttribute(Type type = null, bool exclude = false)
		{
			Type = type;
			Exclude = exclude;
		}

		public bool Exclude { get; set; }

		public Type Type { get; set; }
	}
}
