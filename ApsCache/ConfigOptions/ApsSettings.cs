using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApsCache.ConfigOptions
{

    public interface IApsSettings
    {
        public string Endpoint { get; set; }
    }
    public class ApsSettings:IApsSettings
    {
        public const string ApsConfigSectionName = "APSSettings";

        public string Endpoint { get; set; }
    }
}
