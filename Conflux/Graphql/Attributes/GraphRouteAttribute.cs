namespace Conflux.Graphql.Attributes
{
	using System;

	/// <summary>
	///     Attribute provided on an api endpoint can be converted into graph ql.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class GraphRouteAttribute : Attribute
	{
		/// <summary>
		/// 
		/// </summary>
		public GraphRouteAttribute()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="isMutation"></param>
		public GraphRouteAttribute(string name = null, bool isMutation = false)
		{
			Name = name;
			IsMutation = isMutation;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsMutation { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Name { get; set; }
	}
}
