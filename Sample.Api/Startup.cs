using System;
using MassTransit;
using MassTransit.Definition;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sample.Component.Consumers;
using Sample.Contracts;

namespace Sample.Api {
	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
			services.AddMassTransit(cfg =>
			{
				cfg.AddBus(provider => Bus.Factory.CreateUsingRabbitMq());
				cfg.AddRequestClient<SubmitOrder>(new Uri($"exchange:{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}"));
				cfg.AddRequestClient<CheckOrder>();
			});

			services.AddMassTransitHostedService();
			services.AddOpenApiDocument(cfg => cfg.PostProcess = d => d.Info.Title = "Sample Api Site");
			services.AddControllers();
		}

		public void Configure(IApplicationBuilder app,IWebHostEnvironment env) {
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseOpenApi();
			app.UseSwaggerUi3();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			});
		}
	}
}
