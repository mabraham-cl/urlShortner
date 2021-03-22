using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using urlShortner.Server.Models;
using urlShortner.Shared;

namespace urlShortner.Server.Services
{
    /// <summary>
    /// public interface for UrlService
    /// </summary>
    public interface IUrlService
    {
        /// <summary>
        /// Retrieves all urls in the system
        /// </summary>
        /// <returns>All urls</returns>
        Task<List<LongUrlShortUrl>> GetAsync();

        /// <summary>
        /// Retrieves the longurl for a shorturl
        /// </summary>
        /// <param name="shortUrl">Shorturl</param>
        /// <returns>Url details</returns>
        Task<LongUrlShortUrl> GetAsync(string shortUrl);

        /// <summary>
        /// Creates shorturl for a longurl
        /// </summary>
        /// <param name="longUrl"></param>
        /// <returns></returns>
        Task<LongUrlShortUrl> CreateAsync(string longUrl);

    }
}
