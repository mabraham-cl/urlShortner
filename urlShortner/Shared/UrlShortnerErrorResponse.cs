using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace urlShortner.Shared
{
    public class UrlShortnerErrorResponse
    {
        /// <summary>
        /// Message which describes the error
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// The HTTP status code of the error
        /// </summary>
        [JsonProperty("status")]
        public int HttpStatus { get; set; }
    }
}
