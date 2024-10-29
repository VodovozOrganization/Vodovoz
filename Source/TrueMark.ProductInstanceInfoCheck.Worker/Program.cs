using TrueMark.ProductInstanceInfoCheck.Worker;

IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureServices(services =>
	{
		services.AddProductInstanceInfoCheckWorker();
		services.AddHostedService<Worker>();
	})
	.Build();

await host.RunAsync();
