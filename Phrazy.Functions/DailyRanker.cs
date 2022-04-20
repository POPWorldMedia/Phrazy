using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Phrazy.Functions
{
    public class DailyRanker
    {
	    private readonly IConfiguration _configuration;
	    private readonly ILogger _logger;

	    public DailyRanker(IConfiguration configuration, ILogger logger)
	    {
		    _configuration = configuration;
		    _logger = logger;
	    }

	    [Function("DailyRanker")]
        public async Task Run([TimerTrigger("0 0 8 * * *")]TimerInfo timer)
        {
	        var stopwatch = new Stopwatch();
            stopwatch.Start();

            var ranker = new Ranker();
            await ranker.ExecuteRanking(_configuration, _logger);

            stopwatch.Stop();
            _logger.LogInformation($"DailyRanker executed in {stopwatch.ElapsedMilliseconds}ms at {DateTime.UtcNow} UTC");
        }
    }
}
