// <auto-generated />
//
// This file was automatically generated by PocosGenerator.csx, inspired from the PetaPoco T4 Template
// Do not make changes directly to this file - edit the PocosGenerator.GenerateClass() method in the PocosGenerator.Core.csx file instead
// 

using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace com.apthai.SmartTimeStampAPI.Model
{
    [Table("SS_M_Register")]
    public partial class SS_M_Register
    {
        public int RegisterID { get; set; }
        public int EmpID { get; set; }
        public DateTime RegisterDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNO { get; set; }
        public string IMEI { get; set; }
        public string Password { get; set; }
        public string DeviceKey { get; set; }
        public string DeviceID { get; set; }
        public string MobileBrand { get; set; }
        public string MobileModel { get; set; }
        public string AndroidVersion { get; set; }
        public string FirebaseKey { get; set; }
        public bool IsActive { get; set; }
    }

    public partial class Param_Register
    {
        public string PhoneNO { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }        
        public string IMEI { get; set; }
        public string Password { get; set; }
        public string DeviceID { get; set; }
        public string MobileBrand { get; set; }
        public string MobileModel { get; set; }
        public string AndroidVersion { get; set; }
    }

    public partial class Param_ChangePassword
    {
        public string PhoneNO { get; set; }
        public string Password { get; set; }
        public string DeviceID { get; set; }
    }
}
