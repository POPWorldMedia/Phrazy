using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Phrazy.Functions
{
    public class ManualRank
    {
	    private readonly IConfiguration _configuration;
	    private readonly ILogger _logger;

        public ManualRank(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
	        _configuration = configuration;
	        _logger = loggerFactory.CreateLogger<ManualRank>();
        }

        [Function("ManualRank")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
	        var stopwatch = new Stopwatch();
	        stopwatch.Start();

	        var ranker = new Ranker();
	        await ranker.ExecuteRanking(_configuration, _logger);

	        stopwatch.Stop();
	        _logger.LogInformation($"ManualRank executed in {stopwatch.ElapsedMilliseconds}ms at {DateTime.UtcNow} UTC");

	        var response = req.CreateResponse(HttpStatusCode.OK);
	        return response;
        }
    }
}
