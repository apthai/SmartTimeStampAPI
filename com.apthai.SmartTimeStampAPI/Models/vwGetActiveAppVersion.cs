using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace com.apthai.SmartTimeStampAPI.Model
{
    public class vwGetActiveAppVersion
    {
        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public string AppDescription { get; set; }
        public string UpdateFileUrl { get; set; }
        public string UpdateHomePageUrl { get; set; }
        public string UpdateInfoUrl { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool IsUpdateDate { get; set; }
        public string AppUpdateListNotes { get; set; }
    }
}
