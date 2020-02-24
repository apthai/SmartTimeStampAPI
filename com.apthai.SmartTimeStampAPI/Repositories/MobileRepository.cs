using Dapper;
using com.apthai.SmartTimeStampAPI.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Hosting;
using com.apthai.SmartTimeStampAPI.Interfaces;
using com.apthai.SmartTimeStampAPI.Repositories;
using com.apthai.SmartTimeStampAPI;
//using com.apthai.SmartTimeStampAPI.Extention;

namespace com.apthai.SmartTimeStampAPI.Repositories
{
    public class MobileRepository : BaseRepository, IMobileRepository
    {


        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _hostingEnvironment;

        public MobileRepository(IHostingEnvironment environment, IConfiguration config) : base(environment, config)
        {
            _config = config;
            _hostingEnvironment = environment;

        }

        public int Register(SS_M_Register param)
        {
            param.RegisterDate = DateTime.Now;
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"Insert into SS_M_Register(EmpID,FirstName,LastName,RegisterDate,Password,PhoneNO,DeviceKey,DeviceID,MobileBrand,MobileModel,AndroidVersion,FirebaseKey,IMEI,IsActive)
Values(@EmpID,@FirstName,@LastName,getdate(),@Password,@PhoneNO,@DeviceKey,@DeviceID,@MobileBrand,@MobileModel,@AndroidVersion,@FirebaseKey,@IMEI,@IsActive);
Select @@Identity";
                conn.Open();
                var result = conn.Query<int>(sQuery
                    , new {
                        EmpID = param.EmpID,
                        //RegisterDate = param.RegisterDate,
                        FirstName = param.FirstName,
                        LastName = param.LastName,
                        Password = param.Password,
                        DeviceKey = param.DeviceKey,
                        DeviceID = param.DeviceID,
                        MobileBrand = param.MobileBrand,
                        MobileModel = param.MobileModel,
                        AndroidVersion = param.AndroidVersion,
                        FirebaseKey = param.FirebaseKey,
                        PhoneNO = param.PhoneNO,
                        IMEI = param.IMEI,
                        IsActive = 1
                    }
                    );
                
                return (int)result.FirstOrDefault();
            }
        }

        public void Login(SS_M_Login param)
        {
            param.LoginDate = DateTime.Now;
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"Insert into SS_M_Login(EmpID,LoginDate,Password,PhoneNO,DeviceID,MobileBrand,MobileModel,AndroidVersion,IMEI)
Values(@EmpID,getdate(),@Password,@PhoneNO,@DeviceID,@MobileBrand,@MobileModel,@AndroidVersion,@IMEI);";
                conn.Open();
                var result = conn.Query(sQuery
                    , new
                    {
                        EmpID = param.EmpID,
                        Password = param.Password,
                        DeviceID = param.DeviceID,
                        MobileBrand = param.MobileBrand,
                        MobileModel = param.MobileModel,
                        AndroidVersion = param.AndroidVersion,
                        PhoneNO = param.PhoneNO,
                        IMEI = param.IMEI
                    }
                    );
            }
        }

        public void ChangePassword(int empid,string newPassword)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"Update SS_M_Register
Set Password=@Password
Where Isnull(IsActive,0)=1
and EmpID=@EmpID";
                conn.Open();
                var result = conn.Query<int>(sQuery
                    , new
                    {
                        EmpID = empid,
                        Password = newPassword
                    }
                    );
            }
        }

        public bool CheckExistingDeviceKey(string deviceKey)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"Select DeviceKey From SS_M_Register Where DeviceKey=@DeviceKey and Isnull(DeviceKey,'')<>'' and IsActive=1";
                conn.Open();
                var result = conn.Query<string>(sQuery
                    , new
                    {
                        DeviceKey = deviceKey
                    }
                    );
                string str = result.FirstOrDefault<string>();
                return !string.IsNullOrEmpty(str);
            }
        }

        public void UpdateFirebaseKey(string deviceKey,string firebaseKey)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"Update SS_M_Register Set firebaseKey=@firebaseKey Where DeviceKey=@DeviceKey and Isnull(DeviceKey,'')<>''and IsActive=1";
                conn.Open();
                var result = conn.Query<int>(sQuery
                    , new
                    {
                        FirebaseKey = firebaseKey,
                        DeviceKey = deviceKey
                    }
                    );
            }
        }

        public Employee GetEmployeeProfile(string firstName,string lastName,string phoneNo)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"select EmpID,EmpCode,VendorCode,VendorName,WorkCatID,WorkCatName 
,TitleName,FirstName,LastName,PhoneNO,PositionID,PositionName,EmpTypeID,EmpTypeName,HireDate,Status
from SS_Employee
Where RTrim(Isnull(FirstName,''))=@FirstName and RTrim(Isnull(LastName,''))=@LastName and RTrim(Isnull(PhoneNO,''))=@PhoneNO
and Isnull(Status,1)=1
Order by HireDate desc";
                conn.Open();
                var result = conn.Query<Employee>(sQuery
                    , new
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        PhoneNO = phoneNo
                    }
                    );
                return (Employee)result.FirstOrDefault();
            }
        }

        public Employee GetEmployeeProfileByDeviceID(string deviceID, string phoneNo)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"select top 1 a.EmpID,b.EmpCode,b.VendorCode,b.VendorName,b.WorkCatID,b.WorkCatName 
,b.TitleName,b.FirstName,b.LastName,a.PhoneNO,b.PositionID,b.PositionName,b.EmpTypeID,b.EmpTypeName,b.HireDate,b.Status
,a.RegisterDate,a.DeviceID,a.DeviceKey,a.FirebaseKey
from SS_M_Register a
left Join SS_Employee b on a.EmpID=b.EmpID
Where RTrim(Isnull(a.DeviceID,''))=@DeviceID and RTrim(Isnull(a.PhoneNO,''))=@PhoneNO
and Isnull(b.Status,1)=1
and Isnull(a.IsActive,0)=1";
                conn.Open();
                var result = conn.Query<Employee>(sQuery
                    , new
                    {
                        DeviceID = deviceID,
                        PhoneNO = phoneNo
                    }
                    );
                return (Employee)result.FirstOrDefault();
            }
        }

        public Employee GetEmployeeProfileByDeviceKey(string deviceKey, string phoneNo)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"select top 1 a.EmpID,b.EmpCode,b.VendorCode,b.VendorName,b.WorkCatID,b.WorkCatName 
,b.TitleName,b.FirstName,b.LastName,a.PhoneNO,b.PositionID,b.PositionName,b.EmpTypeID,b.EmpTypeName,b.HireDate,b.Status
,a.RegisterDate,a.DeviceID,a.DeviceKey,a.FirebaseKey
from SS_M_Register a
left Join SS_Employee b on a.EmpID=b.EmpID
Where RTrim(Isnull(a.DeviceKey,''))=@DeviceKey and RTrim(Isnull(a.PhoneNO,''))=@PhoneNO
and Isnull(b.Status,1)=1
and Isnull(a.IsActive,0)=1";
                conn.Open();
                var result = conn.Query<Employee>(sQuery
                    , new
                    {
                        DeviceKey = deviceKey,
                        PhoneNO = phoneNo
                    }
                    );
                return (Employee)result.FirstOrDefault();
            }
        }

        public Employee GetEmployeeProfileByDeviceKeyAndDetail(string deviceKey, string phoneNo)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"
select  a.EmpID,b.EmpCode,b.VendorCode,b.VendorName,b.WorkCatID,b.WorkCatName 
,b.TitleName,b.FirstName,b.LastName,a.PhoneNO,b.PositionID,b.PositionName
,'EmpTypeID'=(Select Top 1 d.EmpTypeID 
			From SS_WorkAssign h
			 JOIN SS_WorkAssDetail d ON d.WorkAssID=h.WorkAssID
			WHERE 1=1
			 AND h.DeleteDate IS NULL
			 AND d.DeleteDate IS NULL
			 AND d.Status <> 9 
			 AND ISNULL(h.ApproveStatus,0) = 1
			 and d.EmpID=a.EmpID
			 Order by d.CreateDate desc)
,'EmpTypeName'=(Select Top 1 d.EmpTypeName 
			From SS_WorkAssign h
			 JOIN SS_WorkAssDetail d ON d.WorkAssID=h.WorkAssID
			WHERE 1=1
			 AND h.DeleteDate IS NULL
			 AND d.DeleteDate IS NULL
			 AND d.Status <> 9 
			 AND ISNULL(h.ApproveStatus,0) = 1
			 and d.EmpID=a.EmpID
			 Order by d.CreateDate desc)
,b.HireDate,b.Status
,a.RegisterDate,a.DeviceID,a.DeviceKey,a.FirebaseKey
from SS_M_Register a
left Join SS_Employee b on a.EmpID=b.EmpID
Where RTrim(Isnull(a.DeviceKey,''))=@DeviceKey and RTrim(Isnull(a.PhoneNO,''))=@PhoneNO
and Isnull(b.Status,1)=1
and Isnull(a.IsActive,0)=1";
                conn.Open();
                var result = conn.Query<Employee>(sQuery
                    , new
                    {
                        DeviceKey = deviceKey,
                        PhoneNO = phoneNo
                    }
                    );
                Employee emp = (Employee)result.FirstOrDefault();
                if (emp != null)
                {
                    emp.Projects = GetProject(emp.EmpID);
                    //emp.Positions = GetPosition(emp.EmpID);
                    //emp.TimeShifts = GetTimeShift(emp.EmpID);
                    if (emp.Projects.Count == 1) emp.Projects[0].IsDefault = true;
                    //if (emp.Positions.Count == 1) emp.Positions[0].IsDefault = true;
                    //if (emp.TimeShifts.Count == 1) emp.TimeShifts[0].IsDefault = true;
                    return emp;
                }
                else return null;
            }
        }

        /// <summary>
        /// ตรวจสอบสถานะ Register ของ EmpID
        /// true = EmpID นี้ Register แล้ว
        /// false = EmpID นี้ยังไม่ได้ Register
        /// </summary>
        /// <param name="empid"></param>
        /// <returns></returns>
        public bool CheckRegisterStatus(int empid)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"select *
from SS_M_Register
Where Isnull(EmpID,0)=@EmpID
and Isnull(IsActive,1)=1";
                conn.Open();
                var result = conn.Query<SS_M_Register>(sQuery
                    , new
                    {
                        EmpID = empid
                    }
                    );
                if (result.Count() > 0) return true;//EmpID นี้ Register แล้ว
                else return false;//EmpID นี้ยังไม่ได้ Register
            }
        }

        public List<Project> GetProject(int empid)
        {
            using (IDbConnection conn = Connection)
            {
                //                string sQuery = @"Select distinct top 10 a.ProjectCode,a.ProjectName,Isnull(b.Latitude,0)Latitude,Isnull(b.Longitude,0)Longitude,Isnull(Radius ,0)Radius
                //From vw_SS_EmpLogbookSecure a
                //Left Join Project c on c.ProjectCode=a.ProjectCode
                //Left Join SS_MSTProject b on c.WBS=b.Plant
                //Where 1=1 and a.EmpID=@EmpID
                //Order by 1";
                string sQuery = "exec sp_SS_M_GetProjectByEmpIDV2 @EmpID";
                conn.Open();
                var result = conn.Query<Project>(sQuery
                    , new
                    {
                        EmpID = empid
                    }
                    );
                List<Project> prj = result.ToList<Project>();
                if(prj.Count>0 )prj[0].IsDefault = true;
                return prj;
            }
        }

        public List<Position> GetPosition(int empid, string projectCode)
        {
            using (IDbConnection conn = Connection)
            {
                //                string sQuery = @"select top 10  positionid,positionName from SS_MSTPosition
                //Where status=1  --and EmpID=@EmpID
                //Order by positionid";
                string sQuery = "exec sp_SS_M_GetPositionByEmpID @EmpID,@ProjectCode";
                conn.Open();
                var result = conn.Query<Position>(sQuery
                    , new
                    {
                        EmpID = empid
                        ,ProjectCode = projectCode
                    }
                    );
                List<Position> obj = result.ToList<Position>();
                if (obj.Count > 0) obj[0].IsDefault = true;
                return obj;
            }
        }

        public List<TimeShiftDTO> GetWorkround(int empid, string projectCode,string positionID)
        {
            using (IDbConnection conn = Connection)
            {
                //                string sQuery = @"select WorkRoundID,'WorkRoundName'=WorkRoundName+' [ '+convert(nvarchar(10),TimeStart)+'-'+convert(nvarchar(10),TimeEnd)+' ]' from SS_MSTWorkRound
                //Where status=1  --and EmpID=@EmpID
                //Order by WorkRoundID";
                string sQuery = "exec sp_SS_M_GetWorkroundByEmpID @EmpID,@ProjectCode,@PositionID";
                conn.Open();
                var result = conn.Query<TimeShiftDTO>(sQuery
                    , new
                    {
                        EmpID = empid
                        ,
                        ProjectCode = projectCode
                        ,
                        PositionID = positionID
                    }
                    );
                List<TimeShiftDTO> obj = result.ToList<TimeShiftDTO>();
                if (obj.Count > 0) obj[0].IsDefault = true;
                return obj;
            }
        }

        public object GetNews()
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"select top 50 * from SS_M_News 
Where Isnull(IsActive,0)=1 and getdate() between StartDate and EndDate 
order by Seq,StartDate desc";
                conn.Open();
                var result = conn.Query<News>(sQuery);
                return result.ToList<News>();
            }
        }

        public async Task<vwGetActiveAppVersion> GetActiveAppVersion()
        {
            using (IDbConnection conn = Connection)
            {

                conn.Open();
                var result = await conn.QueryFirstOrDefaultAsync<vwGetActiveAppVersion>("select top 1 * from vw_SS_M_GetActiveAppVersion");
                return result;

            }
        }

        public DateTime CheckInOut(CheckInOut param)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"
Declare @CheckTime datetime2  Set @CheckTime=getdate();
Insert into SS_M_CheckInOut ([EmpID],[CheckInOutType],[CheckInOutDate],[PositionID],[EmpTypeID],[WorkRoundID],[ProjectCode] )
Values(@EmpID,@CheckInOutType,@CheckTime,@PositionID,@EmpTypeID,@WorkRoundID,@ProjectCode)

Select @CheckTime CheckTime;";
                conn.Open();
                var result = conn.ExecuteScalar<DateTime>(sQuery
                    , new
                    {
                        EmpID = param.EmpID,
                        CheckInOutType = param.CheckInOutType,
                        PositionID = param.PositionID,
                        EmpTypeID = param.EmpTypeID,
                        WorkRoundID = param.WorkRoundID,
                        ProjectCode = param.ProjectCode
                    }
                    );
                return (DateTime)result;
            }
        }

        public List<CheckInOut> GetCheckInOutHistory(int empID)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"
select Top 50 a.CheckInOutID,a.EmpID,b.EmpCode,b.FirstName,b.LastName
,b.PositionID,b.PositionName
,e.EmpTypeID,e.EmpTypeName
,c.WorkRoundID,c.WorkRoundName,c.TimeStart,c.TimeEnd
,d.ProjectCode,d.ProjectName
,a.CheckInOutType,a.CheckInOutDate
from SS_M_CheckInOut a
Left join ss_employee b on a.EmpID=b.EmpID
Left join SS_MSTWorkRound c on c.WorkRoundID=a.WorkRoundID
left join Project d on d.ProjectCode=a.ProjectCode
left join SS_MSTEmpType e on e.EmpTypeID=a.EmpTypeID
Where a.EmpID=@EmpID
Order by a.CheckInOutDate desc";
                conn.Open();
                var result = conn.Query< CheckInOut>(sQuery
                    , new
                    {
                        EmpID = empID
                    }
                    );
                return result.ToList< CheckInOut>();
            }
        }
        
    }
}
