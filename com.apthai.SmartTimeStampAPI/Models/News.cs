using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace com.apthai.SmartTimeStampAPI.Model
{
    public class News
    {
        public int NewsID { get; set; }
        public int Seq { get; set; }
        public string Subject { get; set; }
        public string Detail { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public partial class Param_GetNews
    {
        public string PhoneNO { get; set; }
        public string DeviceKey { get; set; }
    }
}
