namespace Conflux.Graphql.Attributes
{
	using Conflux.Graphql.Schema;
	using System;

	[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	public class GraphKnownTypeAttribute : Attribute, IDomainSchemaTypeMapping
	{
		public GraphKnownTypeAttribute(Type domainType, Type schemaType)
		{
			DomainType = domainType;
			SchemaType = schemaType;
		}

		public Type DomainType { get; set; }

		public Type SchemaType { get; set; }
	}
}
