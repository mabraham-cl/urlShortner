using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using urlShortner.Server.Services;
using urlShortner.Shared;

namespace urlShortner.Server.Controllers
{
    /// <summary>
    /// API that facilitates shortening of long url, retrieving of all urls and redirection for short url.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class UrlShortnerController : ControllerBase
    {
        /// <summary>
        /// Constructor
        /// TO DO - Dependency injection for Memory cache when implementing that.
        /// </summary>
        /// <param name="logger">DI for Logger</param>
        /// <param name="urlService">DI for UrlService</param>
        public UrlShortnerController(ILogger<UrlShortnerController> logger, IUrlService urlService)
        {
            _logger = logger;
            _urlService = urlService;
        }

        /// <summary>
        /// Retrieves all urls in the system
        /// </summary>
        /// <returns>All urls</returns>
        /// <response code="200">Success</response>   
        [HttpGet]
        [ProducesResponseType(typeof(List<LongUrlShortUrl>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get()
        {
            try
            {
                _logger.LogInformation("Request received for retrieving all urls.");

                return Ok(await _urlService.GetAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occured on retrieving all urls.", ex);

                return new ObjectResult(new UrlShortnerErrorResponse() { HttpStatus = 500, Message = $"Internal Server Error! {ex.Message}" });
            }
        }

        /// <summary>
        /// Provides redirection for shorturl
        /// </summary>
        /// <param name="shortUrl">Shorturl</param>
        /// <returns></returns>
        /// <response code="404">shorturl is not found</response>
        [HttpGet, Route("/{shortUrl}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromRoute] string shortUrl)
        {
            try
            {
                _logger.LogInformation($"Request received for shortened url {shortUrl}.");

                var result = await _urlService.GetAsync(shortUrl);

                // Redirect original url
                return Redirect(result.LongUrl);
            }
            catch(UrlShortnerException exception)
            {
                _logger.LogError($"Exception occured on retrieving url map for {shortUrl}.", exception);

                if (exception.HttpStatusCode == 404)
                    return new NotFoundObjectResult(new UrlShortnerErrorResponse() { HttpStatus = exception.HttpStatusCode , Message = exception.Message });
                else
                    return new ObjectResult(new UrlShortnerErrorResponse() { HttpStatus = exception.HttpStatusCode, Message = exception.Message });

            }
            catch(Exception ex)
            {
                _logger.LogError($"Exception occured on retrieving url map for {shortUrl}.", ex);

                return new ObjectResult(new UrlShortnerErrorResponse() { HttpStatus = 500, Message = $"Internal Server Error! {ex.Message}" });
            }
            
        }

        /// <summary>
        /// Creates short url for a long url
        /// </summary>
        /// <param name="longUrl">Longurl</param>
        /// <returns>LongUrlShortUrlInfo</returns>
        /// <response code="200">Success</response>   
        /// <response code="400">If the longurl isn't valid</response>
        [HttpPost]
        [ProducesResponseType(typeof(LongUrlShortUrl), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody]string longUrl)
        {
            try
            {
                _logger.LogInformation($"Request received for creating short url for long url {longUrl}.");

                var result = await _urlService.CreateAsync(longUrl);

                return Ok(result);
            }
            catch (UrlShortnerException exception)
            {
                _logger.LogError($"Exception occured on creating short url for {longUrl}.", exception);

                if (exception.HttpStatusCode == 400)
                    return new BadRequestObjectResult(new UrlShortnerErrorResponse() { HttpStatus = exception.HttpStatusCode, Message = exception.Message });

                else
                    return new ObjectResult(new UrlShortnerErrorResponse() { HttpStatus = exception.HttpStatusCode, Message = exception.Message });

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occured on creating short url for {longUrl}.", ex);

                return new ObjectResult(new UrlShortnerErrorResponse() { HttpStatus = 500, Message = $"Internal Server Error! {ex.Message}" });
            }

        }

        /// <summary>
        /// Interface object for accessing UrlService 
        /// </summary>
        private readonly IUrlService _urlService;

        /// <summary>
        /// ILogger object for UrlShortnerController
        /// </summary>
        private readonly ILogger<UrlShortnerController> _logger;
    }
}
