using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace urlShortner.Server.Models
{
    public class LongurlShorturlMap
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string LongUrl { get; set; }

        public string ShortUrl { get; set; }
    }
}
