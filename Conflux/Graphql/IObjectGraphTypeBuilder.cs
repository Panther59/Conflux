using Conflux.Graphql.Wrappers;
using GraphQL.Types;
using System;
using System.Reflection;

namespace Conflux.Graphql
{
	public interface IObjectGraphTypeBuilder
	{
		void Build(InputObjectGraphType graphType, Type type);
		void Build(InterfaceGraphType graphType, Type type);
		void Build(ObjectGraphType graphType, Type type);
		GraphType BuildInstance(Type type);
		bool IsSpecialMethod(MethodInfo method);
	}
}