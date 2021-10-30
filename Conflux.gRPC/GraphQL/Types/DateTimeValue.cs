using GraphQL.Language.AST;
using System;

namespace GraphQL.Types
{
    public class DateTimeValue : ValueNode<DateTime>
    {
        /// <summary>
        /// Initializes a new instance with the specified value.
        /// </summary>
        public DateTimeValue(DateTime value) : base(value)
        {
        }
    }
}