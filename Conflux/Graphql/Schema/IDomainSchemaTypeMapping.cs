using System;

namespace Conflux.Graphql.Schema
{
    public interface IDomainSchemaTypeMapping
    {
        Type DomainType { get; }
        Type SchemaType { get; }
    }
}
