using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace urlShortner.Server.Models
{
    public class UrlMapDatabaseSettings : IUrlMapDatabaseSettings
    {
        public string UrlCollectionName { get; set; }

        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }
    }
}
