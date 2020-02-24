using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace com.apthai.SmartTimeStampAPI.Model
{
    public class TimeShift
    {
        public string WorkRoundID { get; set; }
        public string WorkRoundName { get; set; }
        public decimal TimeStart { get; set; }
        public decimal TimeEnd { get; set; }
        public bool IsDefault { get; set; }
    }

    public class TimeShiftDTO
    {
        public string WorkRoundID { get; set; }
        public string WorkRoundName { get; set; }
        public bool IsDefault { get; set; }
    }

    public partial class Param_GetTimeShift
    {
        public string PhoneNO { get; set; }
        public string DeviceKey { get; set; }
        public string ProjectCode { get; set; }
        public string PositionID { get; set; }
    }
}
