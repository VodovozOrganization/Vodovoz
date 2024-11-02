using NLog.Extensions.Logging;
using TrueMark.ProductInstanceInfoCheck.Worker;

const string _nlogSectionName = nameof(NLog);

IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureLogging((context, logging) =>
	{
		logging.ClearProviders();
		logging.AddNLog();
		logging.AddConfiguration(context.Configuration.GetSection(_nlogSectionName));
	})
	.ConfigureServices((context, services) =>
	{
		services.AddProductInstanceInfoCheckWorker();
		services.AddHostedService<Worker>();
	})
	.Build();

await host.RunAsync();
