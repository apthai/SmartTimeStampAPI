using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace com.apthai.SmartTimeStampAPI.Model
{
    public class Project
    {
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Radius { get; set; }
        public bool IsDefault { get; set; }
        public bool RequireCheckInPic { get; set; }
    }

    public partial class Param_GetProject
    {
        public string PhoneNO { get; set; }
        public string DeviceKey { get; set; }
    }
}
