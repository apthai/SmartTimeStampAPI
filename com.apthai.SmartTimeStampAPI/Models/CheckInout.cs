using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace com.apthai.SmartTimeStampAPI.Model
{
    public class CheckInOut
    {
        public int CheckInOutID { get; set; }
        public int EmpID { get; set; }
        public string CheckInOutType { get; set; }
        public DateTime CheckInOutDate { get; set; }
        public string PositionID { get; set; }
        public string PositionName { get; set; }
        public string EmpTypeID { get; set; }
        public string EmpTypeName { get; set; }
        public string WorkRoundID { get; set; }
        public string WorkRoundName { get; set; }
        public decimal WorkStartTime { get; set; }
        public decimal WorkEndTime { get; set; }
        public string ProjectCode { get; set; }
    }

    public class Param_CheckInOut
    {
        public string PhoneNO { get; set; }
        public string DeviceKey { get; set; }
        public int EmpID { get; set; }
        public string PositionID { get; set; }
        public string EmpTypeID { get; set; }
        public string WorkRoundID { get; set; }
        public string ProjectCode { get; set; }
    }

    public partial class Param_GetHistory
    {
        public string PhoneNO { get; set; }
        public string DeviceKey { get; set; }
    }
}
