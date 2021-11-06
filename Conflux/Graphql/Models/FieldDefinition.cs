namespace Conflux.Graphql.Models
{
	using GraphQL;
	using System;
	using System.Threading.Tasks;

	/// <summary>
	///    Field definition needed for a schema.
	/// </summary>
	public class FieldDefinition
	{
		public FieldDefinition(FieldInformation field, Func<IResolveFieldContext<object>, Task<object>> resolve)
		{
			Field = field;
			Resolve = resolve;
		}

		/// <summary>
		///     Type of response.
		/// </summary>
		public FieldInformation Field { get; }

		/// <summary>
		///     Resolve function to get the object.
		/// </summary>
		public Func<IResolveFieldContext<object>, Task<object>> Resolve { get; }
	}
}
