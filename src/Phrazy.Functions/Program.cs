using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

var configuration = new ConfigurationBuilder()
	.SetBasePath(Environment.CurrentDirectory)
	.AddJsonFile("local.settings.json", true)
	.AddEnvironmentVariables()
	.Build();

var host = new HostBuilder()
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureAppConfiguration(c =>
	{
		c.AddConfiguration(configuration);
	})
	.ConfigureServices(s =>
	{
	})
	.Build();

await host.RunAsync();