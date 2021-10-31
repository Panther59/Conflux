using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.SchemaGenerator
{
	public class GraphQLQueryArgument : QueryArgument
	{
		public GraphQLQueryArgument(IGraphType graphType, Type type) : base(graphType)
		{
			this.BaseType = type;
		}

		public Type BaseType { get; }
	}

	public class GraphQLQueryArguments : List<GraphQLQueryArgument>
	{
		public GraphQLQueryArguments(IEnumerable<GraphQLQueryArgument> collection) : base(collection)
		{
		}

		public QueryArguments GetQueryArguments()
		{
			return new QueryArguments(this);
		}
	}
}
