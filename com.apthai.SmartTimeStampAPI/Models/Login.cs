using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace com.apthai.SmartTimeStampAPI.Model
{
    [Table("SS_M_Login")]
    public partial class SS_M_Login
    {
        public int LoginID { get; set; }
        public int EmpID { get; set; }
        public DateTime LoginDate { get; set; }
        public string PhoneNO { get; set; }
        public string IMEI { get; set; }
        public string Password { get; set; }
        public string DeviceID { get; set; }
        public string MobileBrand { get; set; }
        public string MobileModel { get; set; }
        public string AndroidVersion { get; set; }
    }

    public partial class SS_M_LoginV2 : SS_M_Login
    {
       public bool RequireCheckInPic { get; set; }
    }

    public partial class Param_Login
    {
        public string PhoneNO { get; set; }
        public string Password { get; set; }
        public string IMEI { get; set; }     
        public string DeviceID { get; set; }
        public string MobileBrand { get; set; }
        public string MobileModel { get; set; }
        public string AndroidVersion { get; set; }
    }

    public partial class Param_ReturnLogin
    {
        public string DeviceKey { get; set; }
    }

    public partial class Param_ForgotPassword
    {
        public string PhoneNO { get; set; }
        public string DeviceID { get; set; }
    }
}
