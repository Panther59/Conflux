using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Execution;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;

namespace GraphQL.SchemaGenerator
{
    /// <summary>
    ///     Caches a saved document for reuse.
    /// </summary>
    public class SavedDocumentBuilder : IDocumentBuilder
    {
        public SavedDocumentBuilder(string query, IDocumentBuilder builder)
        {
            builder = builder ?? new GraphQLDocumentBuilder();

            Document = builder.Build(query);
        }

        /// <summary>
        ///     The saved document.
        /// </summary>
        public Document Document { get; }

        public Document Build(string body)
        {
            return Document;
        }

        public int OperationNodes
        {
            get
            {
                if (Document?.Operations == null)
                {
                    return 0;
                }

                var count = Document.Operations.Sum(t => t.SelectionSet.Selections.Count());

                return count;
            }
        }
    }
}
