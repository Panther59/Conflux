namespace Conflux.Graphql
{
	using global::GraphQL.Execution;
	using global::GraphQL.Language.AST;
	using System.Linq;

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

		public Document Build(string body)
		{
			return Document;
		}
	}
}
