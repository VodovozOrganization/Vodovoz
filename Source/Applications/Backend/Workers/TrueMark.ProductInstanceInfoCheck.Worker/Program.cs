using NLog.Extensions.Logging;
using TrueMark.ProductInstanceInfoCheck.Worker;

const string _nlogSectionName = nameof(NLog);

IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureLogging((context, logging) =>
	{
		logging.AddNLog(context.Configuration.GetSection(_nlogSectionName));
	})
	.ConfigureServices((context, services) =>
	{
		services.Configure<TrueMarkProductInstanceInfoCheckOptions>(context.Configuration.GetSection(nameof(TrueMarkProductInstanceInfoCheckOptions)));
		services.AddProductInstanceInfoCheckWorker();
	})
	.Build();

await host.RunAsync();
