namespace Conflux.Graphql.Types
{
	using GraphQL.Language.AST;
	using GraphQL.Types;
	using System;

	/// <summary>
	///     Time span graph type.
	/// </summary>
	public class TimeSpanGraphType : ScalarGraphType
	{
		/// <summary>
		///     Constructor
		/// </summary>
		public TimeSpanGraphType()
		{
			Name = "TimeSpan";
			Description = "The `TimeSpan` scalar type represents a timespan in the format HH::mm::ss";
		}

		/// <summary>
		///     Parse literal.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override object ParseLiteral(IValue value)
		{
			var dateTimeVal = value as StringValue;

			if (dateTimeVal == null)
			{
				return null;
			}

			return ParseValue(dateTimeVal.Value);
		}

		/// <summary>
		///     Parse value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override object ParseValue(object value)
		{
			string inputValue;
			TimeSpan timeSpan;

			if (value is DateTime)
			{
				inputValue = ((DateTime)value).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF'Z'");
			}
			else
			{
				inputValue = value?.ToString().Trim('"') ?? string.Empty;
			}

			if (TimeSpan.TryParse(inputValue, out timeSpan))
			{
				return timeSpan;
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
			return ParseValue(value).ToString();
		}
	}
}
