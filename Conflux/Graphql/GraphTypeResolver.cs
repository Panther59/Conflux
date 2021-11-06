using System;
using Conflux.Graphql.Wrappers;
using GraphQL.Types;

namespace Conflux.Graphql
{
	public class GraphTypeResolver : IGraphTypeResolver
	{
		private readonly IGraphTypeConverter graphTypeConverter;
		private readonly IObjectGraphTypeBuilder objectGraphTypeBuilder;

		public GraphTypeResolver(
			IGraphTypeConverter graphTypeConverter,
			IObjectGraphTypeBuilder objectGraphTypeBuilder)
		{
			this.graphTypeConverter = graphTypeConverter;
			this.objectGraphTypeBuilder = objectGraphTypeBuilder;
		}

		/// <summary>
		///     Resolve a type into a graph type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public GraphType ResolveType(Type type)
		{
			if (type.IsInterface || type.IsAbstract)
			{
				return null;
			}

			if (type.IsGenericType)
			{
				if (type.GetGenericTypeDefinition() == typeof(InterfaceGraphTypeWrapper<>))
				{
					return CreateCustomTypeInsance(type);
				}

				if (type.GetGenericTypeDefinition() == typeof(InputObjectGraphTypeWrapper<>))
				{
					return CreateCustomTypeInsance(type);
				}

				if (type.GetGenericTypeDefinition() == typeof(KeyValuePairGraphType<,>))
				{
					return Activator.CreateInstance(type) as GraphType;
				}

				if (type.GetGenericTypeDefinition() == typeof(EnumerationGraphTypeWrapper<>))
				{
					return Activator.CreateInstance(type) as GraphType;
				}

				if (type.GetGenericTypeDefinition() == typeof(ObjectGraphTypeWrapper<>))
				{
					return CreateCustomTypeInsance(type);
				}
			}

			if (type.IsAssignableFrom(typeof(GraphType)) || type.IsSubclassOf(typeof(GraphType)))
			{
				return Activator.CreateInstance(type) as GraphType;
			}

			var graphType = this.graphTypeConverter.ConvertTypeToGraphType(type);

			if (graphType == null)
			{
				return null;
			}

			var generic = typeof(ObjectGraphTypeWrapper<>).MakeGenericType(graphType);

			return Activator.CreateInstance(generic) as GraphType;
		}

		public GraphType CreateCustomTypeInsance(Type type)
		{
			var obj = Activator.CreateInstance(type) as GraphType;
			((dynamic)obj).Build(this.objectGraphTypeBuilder);
			return obj;
		}

			public T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		public object Resolve(Type type)
		{
			return ResolveType(type);
		}

		public object GetService(Type serviceType)
		{
			return this.Resolve(serviceType);
		}
	}
}

