namespace Conflux.Graphql.Attributes
{
	using Conflux.Graphql.Models;
	using System;

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
	public class GraphNotRequiredAttribute : Attribute
	{
		public GraphNotRequiredAttribute(RequiredType type = RequiredType.NotRequired)
		{
			Type = type;
		}

		public RequiredType Type { get; set; }
	}
}
