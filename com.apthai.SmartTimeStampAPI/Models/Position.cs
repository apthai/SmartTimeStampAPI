using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace com.apthai.SmartTimeStampAPI.Model
{
    public class Position
    {
        public string PositionID { get; set; }
        public string PositionName { get; set; }
        public bool IsDefault { get; set; }
    }

    public partial class Param_GetPosition
    {
        public string PhoneNO { get; set; }
        public string DeviceKey { get; set; }
        public string ProjectCode { get; set; }
    }
}
