using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;

namespace TrueMark.Api;

public class Program
{
	private const string _nlogSectionName = nameof(NLog);

	public static void Main(string[] args)
	{
		Console.OutputEncoding = System.Text.Encoding.UTF8;

		var builder = WebApplication.CreateBuilder(args);

		builder.Host.ConfigureLogging((context, logging) =>
		{
			logging.ClearProviders();
			logging.AddNLogWeb();
			logging.AddConfiguration(context.Configuration.GetSection(_nlogSectionName));
		});

		builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
			.ConfigureServices((context, services) => services.AddTrueMarkApi(context.Configuration));

		var app = builder.Build();

		if(app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			app.UseSwagger();
			app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrueMarkApi v1"));
		}

		app.UseRouting();

		app.UseAuthorization();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});

		app.Run();
	}
}
