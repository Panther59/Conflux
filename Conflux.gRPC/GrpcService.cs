using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conflux.gRPC
{
	public class GrpcService
	{
		public GrpcService(string name, Type type)
		{
			Name = name;
			Type = type;
		}

		public string Name { get; set; }
		public Type Type { get; set; }
	}
}
