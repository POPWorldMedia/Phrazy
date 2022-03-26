namespace Phrazy.Server;

public interface IConfig
{
	string DatabaseConnectionString { get; }
}

public class Config : IConfig
{
	private readonly IConfiguration _configuration;

	public Config(IConfiguration configuration)
	{
		_configuration = configuration;
	}
	public string DatabaseConnectionString => _configuration["DatabaseConnectionString"];
}