namespace Conflux.Graphql.Models
{
	using GraphQL;
	using System;
	using System.Reflection;

	/// <summary>
	///    Field definition needed for a schema.
	/// </summary>
	public class FieldInformation
	{
		/// <summary>
		///     Query arguments.
		/// </summary>
		public GraphQLQueryArguments Arguments { get; set; }

		public Type Grpc { get; set; }

		/// <summary>
		///     Is a mutation.
		/// </summary>
		public bool IsMutation { get; set; }

		/// <summary>
		///     The method information this field is defined from.
		/// </summary>
		//public MethodInfo Method { get; set; }

		/// <summary>
		///     Name of field.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///     The obsolete attribute message if any.
		/// </summary>
		public string ObsoleteReason { get; set; }

		/// <summary>
		///     Type of response.
		/// </summary>
		public Type Response { get; set; }

		public string ServiceName { get; set; }
	}
}
