using GraphQL.Types;

namespace Conflux.Graphql.Schema
{
    public interface ISchemaFactory
    {
        ISchema GetOrCreateSchema();
    }

}
