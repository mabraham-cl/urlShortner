using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace urlShortner.Server.Models
{
    public interface IUrlMapDatabaseSettings
    {
        string UrlCollectionName { get; set; }

        string ConnectionString { get; set; }

        string DatabaseName { get; set; }
    }
}
