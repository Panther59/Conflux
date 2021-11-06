namespace Conflux.Graphql.Wrappers
{
	using GraphQL.Language.AST;
	using GraphQL.Types;
	using System;

	/// <summary>
	///     Byte array graph type.
	/// </summary>
	public class ByteArrayGraphType : ScalarGraphType
	{
		/// <summary>
		///     Constructor.
		/// </summary>
		public ByteArrayGraphType()
		{
			Name = "Base64";
			Description = "Byte array";
		}

		/// <summary>
		///     Parse literal.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override object ParseLiteral(IValue value)
		{
			var val = value as StringValue;

			if (val == null)
			{
				return null;
			}

			return ParseValue(val.Value);
		}

		/// <summary>
		///     Parse value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override object ParseValue(object value)
		{
			var bytes = value as byte[];
			if (bytes != null)
			{
				return Convert.ToBase64String(bytes);
			}

			return null;
		}

		/// <summary>
		///     Serialize.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override object Serialize(object value)
		{
			return ParseValue(value);
		}
	}
}
