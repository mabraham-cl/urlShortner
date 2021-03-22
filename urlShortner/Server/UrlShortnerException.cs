using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace urlShortner.Server
{
    public class UrlShortnerException : Exception
    {
        /// <summary>
        /// Custom Exception that can be thrown from API
        /// </summary>
        /// <param name="message">Message to the user</param>
        /// <param name="httpStatusCode">Http status code</param>
        public UrlShortnerException(string message, int httpStatusCode) : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// HTTPS Status Code
        /// </summary>
        public int HttpStatusCode { get; set; }
    }
}
