using System.Collections.Generic;

namespace Conflux.Graphql.Models
{
    public class GraphControllerDefinition
    {
        public IEnumerable<GraphRouteDefinition> Routes { get; private set; }
        public GraphControllerDefinition(IEnumerable<GraphRouteDefinition> routes)
        {
            Routes = routes;
        }
    }

}
