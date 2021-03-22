using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using urlShortner.Server.Models;
using urlShortner.Shared;
using System.Security.Cryptography;
using System.Text;

namespace urlShortner.Server.Services
{
    /// <summary>
    /// Service class that facilitates all the business logic for UrlShortnerController.
    /// This is a singleton service. Only one object will be created for the entire applications life time.
    /// It means that after the initial request of the service, every subsequent request will use the same instance.
    /// The singleton service lifetime is most appropriate because UrlService takes a direct dependency on MongoClient. 
    /// Per the official Mongo Client reuse guidelines, MongoClient should be registered in DI with a singleton service lifetime.
    /// https://mongodb.github.io/mongo-csharp-driver/2.8/reference/driver/connecting/#re-use
    /// The mongo db connection used in this Service is shared between requests.
    /// Connection string option set for retryable writes which will retry 1 time if it fails.
    /// The options 'write concern' is set to "w=majority". This is to ensure your data is successfully written to your database and persisted.
    /// More Info : https://docs.atlas.mongodb.com/resilient-application/
    /// There is no need of disposing of mongo client/mongo db objects as the application takes care of that.
    /// </summary>
    public class UrlService : IUrlService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Database settings</param>
        public UrlService(IUrlMapDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);

            var database = client.GetDatabase(settings.DatabaseName);

            _urlMap = database.GetCollection<LongurlShorturlMap>(settings.UrlCollectionName);
        }

        /// <summary>
        /// Retrieves all urls in the system
        /// </summary>
        /// <returns>All urls</returns>
        public async Task<List<LongUrlShortUrl>> GetAsync()
        {
            List<LongUrlShortUrl> urlList = new List<LongUrlShortUrl>();

            await _urlMap.Find(x => true).ForEachAsync(urlMap =>
            {
                urlList.Add(new LongUrlShortUrl() { 
                    LongUrl = urlMap.LongUrl,
                    ShortUrl =urlMap.ShortUrl
                });

            });

            return urlList;
        }

        /// <summary>
        /// Retrieves the longurl for a shorturl
        /// </summary>
        /// <param name="shortUrl">Shorturl</param>
        /// <returns>Url details</returns>
        public async Task<LongUrlShortUrl> GetAsync(string shortUrl)
        {
            var result = await GetUrlAsync(shortUrl, null);

            if(result == null)
                throw new UrlShortnerException($"Short url not found.", 404);

            return new LongUrlShortUrl() { LongUrl = result.LongUrl, ShortUrl = result.ShortUrl };
        }

        /// <summary>
        /// Creates shorturl for a longurl
        /// </summary>
        /// <param name="longUrl"></param>
        /// <returns></returns>
        public async Task<LongUrlShortUrl> CreateAsync(string longUrl)
        {
            try
            {
                // Validate url
                Uri uriResult;
                bool result = Uri.TryCreate(longUrl, UriKind.Absolute, out uriResult);

                if (!result)
                    throw new UrlShortnerException("Invalid url entered.", 400);

                // Check if the longurl exists in database
                var urlExists = await GetUrlAsync(null, longUrl);

                // Return the already existing short url if there is an entry for longurl
                if (urlExists != null)
                    return new LongUrlShortUrl() { LongUrl = longUrl, ShortUrl = urlExists.ShortUrl };

                // Generate shorturl. In case of any conflicts in duplicates it retries and regenerate a different shorturl
                string shortUrl = await GenerateShortUrl(longUrl);

                if (shortUrl == null)
                    throw new UrlShortnerException("Alias not available. Please try again later.", 204);

                // Insert to mongo db
                LongurlShorturlMap urlMap = new LongurlShorturlMap() { LongUrl = longUrl, ShortUrl = shortUrl };
                await _urlMap.InsertOneAsync(urlMap, null);

                return new LongUrlShortUrl() { LongUrl = longUrl, ShortUrl = shortUrl };
            }
            catch(AggregateException)
            {
                // Can do Log exceptions here if needed for trouble shooting. 

                // If cannot generate a unique short url. Throw a UrlShortnerException with the message

                throw new UrlShortnerException("Alias not available. Please try again later.", 204);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// This method generates shorturl. In case of any conflicts in duplicates it retries and regenerate a different shorturl.
        /// </summary>
        /// <param name="longUrl">Longurl</param>
        /// <param name="maxAttemptCount">How many attempts it should try if unsuccessfull in creating a unique shorturl</param>
        /// <returns></returns>
        private async Task<string> GenerateShortUrl(string longUrl, int maxAttemptCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    // Call to the method that actually creates the short url.
                    // There is a possible chance of duplicates as we take only the first 7 characters. 
                    // In case the shorturl(first 7 character) already exists for another longurl.
                    // Regererate it until successfull. Can try until max attempt.
                    // If even after it couldnt generate a unique shorturl. throw  as aggregate exception. 
                    string newShortUrl = CreateShortUrl();

                    // Check if the shorturl created exists in the db
                    var urlExists = await GetUrlAsync(newShortUrl, null);

                    if (urlExists != null)
                        throw new Exception($"{newShortUrl} already exists.");
                    else
                        return newShortUrl;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException(exceptions);
        }

        /// <summary>
        /// A common method for filtering data based on shorturl/longurl
        /// </summary>
        /// <param name="shortUrl">Short url</param>
        /// <param name="longUrl">Long url</param>
        /// <returns></returns>
        private async Task<LongUrlShortUrl> GetUrlAsync(string shortUrl, string longUrl)
        {
            var filter = Builders<LongurlShorturlMap>.Filter.Where(x => (shortUrl == null || x.ShortUrl == shortUrl) && (longUrl == null || x.LongUrl == longUrl));

            var result = await _urlMap.Find(filter).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return new LongUrlShortUrl() { LongUrl = result.LongUrl, ShortUrl = result.ShortUrl };
        }

        /// <summary>
        /// This creates the short url.
        /// </summary>
        /// <returns>7 character long short url</returns>
        private string CreateShortUrl()
        {
            // creates a new guid.
            // A GUID is a 128 - bit integer(16 bytes) that can be used across all computers and networks wherever a unique identifier is required.
            // Such an identifier has a very low probability of being duplicated. 
            Guid newGuid = Guid.NewGuid();

            // Base 64 string representation of guid
            string encoded = Convert.ToBase64String(newGuid.ToByteArray());

            // Replacing '/' and '+' of base64 encoded string with '_' and '-' respectively.
            encoded = encoded.Replace("/", "_").Replace("+", "-");

            // Returns the first 7 characters of the encoded string
            return encoded.Substring(0, 7);
        }

        /// <summary>
        /// MongoCollection object
        /// </summary>
        private readonly IMongoCollection<LongurlShorturlMap> _urlMap;
    }
}
