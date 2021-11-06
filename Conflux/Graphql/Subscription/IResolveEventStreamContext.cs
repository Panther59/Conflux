using GraphQL;

namespace Conflux.Graphql.Subscription
{
    public interface IResolveEventStreamContext : IResolveFieldContext
    {
    }

    public interface IResolveEventStreamContext<out TSource> : IResolveFieldContext<TSource>, IResolveEventStreamContext
    {
    }
}
