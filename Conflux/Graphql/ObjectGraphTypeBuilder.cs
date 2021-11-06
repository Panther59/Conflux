using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Conflux.Graphql.Attributes;
using Conflux.Graphql.Extensions;
using Conflux.Graphql.Helpers;
using Conflux.Graphql.Wrappers;
using GraphQL;
using GraphQL.Types;

namespace Conflux.Graphql
{
	public class ObjectGraphTypeBuilder : IObjectGraphTypeBuilder
	{
		private readonly IGraphTypeConverter graphTypeConverter;

		public ObjectGraphTypeBuilder(IGraphTypeConverter graphTypeConverter)
		{
			this.graphTypeConverter = graphTypeConverter;
		}

		public void Build(ObjectGraphType graphType, Type type)
		{
			if (type.IsInterface || type.IsAbstract)
			{
				throw new InvalidOperationException("type must not be an abstract type or an  interface");
			}

			ProcessObjectType(graphType, type);

			bool hasDataContract = type.ShouldIncludeInGraph();

			// KnownTypeAttribute could be used when SchemaType and DomainType are the same
			ProcessType(graphType, type);
			ProcessProperties(graphType, GetProperties(hasDataContract, type));
			//ProcessFields(graphType, GetFields(hasDataContract, type));
			ProcessMethods(graphType, type, type.GetMethods());
		}

		public void Build(InterfaceGraphType graphType, Type type)
		{
			if (!type.IsInterface && !type.IsAbstract)
			{
				throw new InvalidOperationException("type must be an abstract type or an interface");
			}

			ProcessInterfaceType(graphType, type);

			bool hasDataContract = type.ShouldIncludeInGraph();

			// KnownTypeAttribute could be used when SchemaType and DomainType are the same
			ProcessType(graphType, type);
			ProcessProperties(graphType, GetProperties(hasDataContract, type));
			//ProcessFields(graphType, GetFields(hasDataContract, type));
			ProcessMethods(graphType, type, type.GetMethods());
		}

		public void Build(InputObjectGraphType graphType, Type type)
		{
			ProcessType(graphType, type);
			bool hasDataContract = type.ShouldIncludeInGraph();
			ProcessProperties(graphType, GetProperties(hasDataContract, type), true);
			//ProcessFields(graphType, GetFields(hasDataContract, type), true);
			ProcessMethods(graphType, type, type.GetMethods());
		}

		private IEnumerable<PropertyInfo> GetProperties(bool hasDataContract, Type type)
		{
			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			if (hasDataContract)
			{
				return properties.Where(p => p.ShouldIncludeMemberInGraph());
			}
			else
			{
				return properties;
			}
		}

		private IEnumerable<FieldInfo> GetFields(bool hasDataContract, Type type)
		{
			var fields = type.GetFields();
			if (hasDataContract)
			{
				return fields.Where(f => f.ShouldIncludeMemberInGraph());
			}
			else
			{
				return fields;
			}
		}

		private void ProcessInterfaceType(InterfaceGraphType interfaceGraphType, Type type)
		{
			interfaceGraphType.ResolveType = CreateResolveType(type);
		}

		private void ProcessObjectType(ObjectGraphType objectGraphType, Type type)
		{
			foreach (var @interface in type.GetInterfaces())
			{
				if (!IsGraphType(@interface))
				{
					continue;
				}
				objectGraphType.Interfaces.Add(this.graphTypeConverter.ConvertTypeToGraphType(type));
			}
		}

		private bool IsGraphType(Type @interface)
		{
			return TypeHelper.GetGraphType(@interface) != null ||
				@interface.ShouldIncludeInGraph();
		}

		private void ProcessType(GraphType graphType, Type type)
		{
			graphType.Name = TypeHelper.GetDisplayName(type);

			var descAttr = type.GetCustomAttribute<DescriptionAttribute>();
			if (descAttr != null)
			{
				graphType.Description = descAttr.Description;
			}
			// explicit - include with DataMember, implicit - exclude with GraphIgnore            
		}

		private Func<object, ObjectGraphType> CreateResolveType(Type type)
		{
			var expressions = new List<Expression>();
			var knownTypes = TypeHelper.GetGraphKnownTypes(type);

			var instanceParam = Expression.Parameter(typeof(object), "instance");
			var returnLabel = Expression.Label(typeof(ObjectGraphType));

			foreach (var knownType in knownTypes)
			{
				var graphType = this.graphTypeConverter.ConvertTypeToGraphType(knownType.SchemaType);
				var lookup = Expression.IfThen(
					Expression.TypeIs(instanceParam, knownType.DomainType),
					Expression.Return(returnLabel, Expression.Convert(Expression.New(graphType), typeof(ObjectGraphType)))
				);

				expressions.Add(lookup);
			}

			var result = Expression.Convert(Expression.Constant(null), typeof(ObjectGraphType));
			expressions.Add(Expression.Label(returnLabel, result));
			var body = Expression.Block(expressions);

			return Expression.Lambda<Func<object, ObjectGraphType>>(
				body,
				instanceParam).Compile();
		}

		private void ProcessProperties(IComplexGraphType graphType, IEnumerable<PropertyInfo> properties, bool isInputType = false)
		{
			foreach (var property in properties.OrderBy(p => p.Name))
			{
				var required = TypeHelper.IsNotNull(property);

				var propertyGraphType = TypeHelper.GetGraphType(property);
				if (propertyGraphType != null)
				{
					propertyGraphType = this.graphTypeConverter.ConvertTypeToGraphType(propertyGraphType, required, isInputType);
					propertyGraphType = EnsureList(property.PropertyType, propertyGraphType);
				}
				else
				{
					propertyGraphType = this.graphTypeConverter.ConvertTypeToGraphType(property.PropertyType, required, isInputType);
				}

				var name = StringHelper.GraphName(property.Name);
				var field = graphType.AddField(new FieldType
				{
					Type = propertyGraphType,
					Name = name,
					Description = TypeHelper.GetDescription(property),
					ResolvedType = propertyGraphType as IGraphType
				}); 

				field.DefaultValue = TypeHelper.GetDefaultValue(property);
				field.DeprecationReason = TypeHelper.GetDeprecationReason(property);
			}
		}

		private void ProcessFields(IComplexGraphType graphType, IEnumerable<FieldInfo> fields, bool isInputType = false)
		{
			foreach (var field in fields.OrderBy(f => f.Name))
			{
				var required = TypeHelper.IsNotNull(field);

				var fieldGraphType = TypeHelper.GetGraphType(field);
				if (fieldGraphType != null)
				{
					fieldGraphType = this.graphTypeConverter.ConvertTypeToGraphType(fieldGraphType, required, isInputType);
					fieldGraphType = EnsureList(field.FieldType, fieldGraphType);
				}
				else
				{
					fieldGraphType = this.graphTypeConverter.ConvertTypeToGraphType(field.FieldType, required, isInputType);
				}

				var addedField = graphType.AddField(new FieldType
				{
					Type = fieldGraphType,
					Name = StringHelper.GraphName(field.Name)
				});

				addedField.DeprecationReason = TypeHelper.GetDeprecationReason(field);

			}
		}

		private void ProcessMethods(IComplexGraphType graphType, Type type, IEnumerable<MethodInfo> methods)
		{
			if (!typeof(GraphType).IsAssignableFrom(type) &&
				!type.IsDefined(typeof(GraphTypeAttribute)))
			{
				return;
			}
			foreach (var method in methods.OrderBy(m => m.Name))
			{
				if (IsSpecialMethod(method))
				{
					continue;
				}

				var required = TypeHelper.IsNotNull(method);
				var returnGraphType = TypeHelper.GetGraphType(method);
				var methodGraphType = returnGraphType;
				if (methodGraphType != null)
				{
					methodGraphType = this.graphTypeConverter.ConvertTypeToGraphType(methodGraphType, required);
					methodGraphType = EnsureList(method.ReturnType, methodGraphType);
				}
				else
				{
					methodGraphType = this.graphTypeConverter.ConvertTypeToGraphType(method.ReturnType, required);
				}

				var arguments =
					new QueryArguments(
						method.GetParameters()
							.Where(p => p.ParameterType != typeof(ResolveFieldContext))
							.Select(CreateArgument));

				// todo: need to fix method execution - not called currently so lower priority
				graphType.AddField(new FieldType
				{
					Type = methodGraphType,
					Name = StringHelper.GraphName(method.Name),
					Arguments = arguments,
					DeprecationReason = TypeHelper.GetDeprecationReason(method),
					//Resolver = new AsyncFuncFieldResolver(()=>ResolveField(context, field))
				});
			}
		}

		private Type EnsureList(Type type, Type methodGraphType)
		{
			if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				methodGraphType = typeof(ListGraphType<>).MakeGenericType(methodGraphType);
			}

			return methodGraphType;
		}

		public bool IsSpecialMethod(MethodInfo method)
		{
			return method.IsSpecialName || method.DeclaringType == typeof(object);
		}

		private QueryArgument CreateArgument(ParameterInfo parameter)
		{
			var required = TypeHelper.IsNotNull(parameter);
			var parameterGraphType = TypeHelper.GetGraphType(parameter);
			if (parameterGraphType != null)
			{
				parameterGraphType = this.graphTypeConverter.ConvertTypeToGraphType(parameterGraphType, required);
				parameterGraphType = EnsureList(parameter.ParameterType, parameterGraphType);
			}
			else
			{
				parameterGraphType = this.graphTypeConverter.ConvertTypeToGraphType(parameter.ParameterType, required);
			}

			return new QueryArgument(parameterGraphType)
			{
				Name = parameter.Name,
				DefaultValue = TypeHelper.GetDefaultValue(parameter),
				Description = TypeHelper.GetDescription(parameter),
			};
		}

		public GraphType BuildInstance(Type type)
		{
			if (type.GenericTypeArguments.Length > 0)
			{
				var innerType = type.GenericTypeArguments[0];
				var instance = Activator.CreateInstance(type) as ObjectGraphType;
				this.Build(instance, innerType);
				instance.Name = "Input_" + instance.Name;
				return instance;
			}
			else
			{
				var instance = Activator.CreateInstance(type) as ObjectGraphType;
				return instance;
			}
		}
	}
}
