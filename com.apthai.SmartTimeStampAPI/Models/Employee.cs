using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace com.apthai.SmartTimeStampAPI.Model
{
    public class Employee
    {
        public int EmpID { get; set; }
        public string EmpCode { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string WorkCatID { get; set; }
        public string WorkCatName { get; set; }
        public string TitleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNO { get; set; }
        public string PositionID { get; set; }
        public string PositionName { get; set; }
        public string EmpTypeID { get; set; }
        public string EmpTypeName { get; set; }
        public DateTime? HireDate { get; set; }
        public string Status { get; set; }
        public string DeviceKey { get; set; }
        public string DeviceID { get; set; }
        public string FirebaseKey { get; set; }
        public DateTime RegisterDate { get; set; }
        public List<Project> Projects { get; set; }
        public List<Position> Positions { get; set; }
        public List<TimeShiftDTO> TimeShifts { get; set; }
    }

    public partial class Param_GetProfile
    {
        public string PhoneNO { get; set; }
        public string DeviceKey { get; set; }
    }
}
