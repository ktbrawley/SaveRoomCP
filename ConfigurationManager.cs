using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SaveRoomCP
{
    public static class ConfigurationManager
    {
        private static readonly IConfiguration _configuration = new ConfigurationBuilder()
           .AddJsonFile("./App_Config/appsettings.json", true, true)
           .Build();

        public static string GetConfigurationValue(string key)
        {
            return _configuration.GetSection(key).Value;
        }
    }
}
