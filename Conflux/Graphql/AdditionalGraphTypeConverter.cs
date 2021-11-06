using Conflux.Graphql.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conflux.Graphql
{
	public class AdditionalGraphTypeConverter : IAdditionalGraphTypeConverter
	{
		public Type BaseGraphType(Type propertyType, bool isInputType, Func<Type, RequiredType, bool, Type> orifiginalConverter)
		{
			return null;
		}
	}
}
