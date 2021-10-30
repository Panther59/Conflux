using System;
using System.Threading.Tasks;
using GraphQL.Types;

namespace GraphQL.SchemaGenerator.Models
{
    /// <summary>
    ///    Field definition needed for a schema.
    /// </summary>
    public class FieldDefinition
    {
        /// <summary>
        ///     Type of response.
        /// </summary>
        public FieldInformation Field { get; }

        /// <summary>
        ///     Resolve function to get the object.
        /// </summary>
        public Func<IResolveFieldContext<object>, Task<object>> Resolve { get; }

        public FieldDefinition(FieldInformation field, Func<IResolveFieldContext<object>, Task<object>> resolve)
        {
            Field = field;
            Resolve = resolve;
        }
    }
}
