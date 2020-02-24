using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.apthai.SmartTimeStampAPI
{
    public static class SettingServiceProvider
    {
        public static IConfiguration Configuration { get; set; }

        public static string OutputPath { get; set; }
        public static string VirtualDirectory { get; set; }
        public static string API_Key { get; set; }
        public static string API_Token { get; set; }
        public static string SMSApiUrl { get; set; }
    }
}
       