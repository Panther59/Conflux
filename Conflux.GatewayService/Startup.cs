using Conflux.Extensions;
using Conflux.gRPC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Conflux.gRPCServices.greet.Greeter;

namespace Conflux.GatewayService
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			var type = typeof(GraphQL.Types.QueryArgument<GraphQL.SchemaGenerator.Wrappers.InputObjectGraphTypeWrapper<Conflux.gRPCServices.greet.HelloRequest>>);
			var type1 = typeof(GraphQL.SchemaGenerator.Wrappers.InputObjectGraphTypeWrapper<Conflux.gRPCServices.greet.HelloRequest>);
			var type2 = typeof(Conflux.gRPCServices.greet.HelloRequest);
			var obj = Activator.CreateInstance(type);
			var obj2 = Activator.CreateInstance(type1);
			var obj3 = Activator.CreateInstance(type2);
			var q = new GraphQL.Types.QueryArgument(type1);
			services.AddConflux();
			services.AddConfluxGRPC(typeof(GreeterBase));
			services.AddControllers();

			services.AddGrpcClient<GreeterBase>("Greeter", o =>
			{
				o.Address = new Uri("https://localhost:6001");
			});

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Conflux.GatewayService", Version = "v1" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conflux.GatewayService v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseConfluxGRPC();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
