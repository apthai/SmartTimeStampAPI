//using com.apthai.SmartTimeStampAPI.Extention;
using com.apthai.SmartTimeStampAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.apthai.SmartTimeStampAPI.Interfaces
{
    public interface IMobileRepository
    {
        int Register(SS_M_Register param);
        void Login(SS_M_Login param);
        void UpdateFirebaseKey(string deviceKey, string firebaseKey);
        bool CheckExistingDeviceKey(string deviceKey);
        Employee GetEmployeeProfile(string firstName, string lastName, string phone);
        bool CheckRegisterStatus(int empid);
        object GetNews();
        Task<vwGetActiveAppVersion> GetActiveAppVersion();
        Employee GetEmployeeProfileByDeviceKey(string deviceKey, string phoneNo);
        Employee GetEmployeeProfileByDeviceID(string deviceID, string phoneNo);
        DateTime CheckInOut(CheckInOut param);
        List<CheckInOut> GetCheckInOutHistory(int empID);
        void ChangePassword(int empid, string newPassword);
        Employee GetEmployeeProfileByDeviceKeyAndDetail(string deviceKey, string phoneNo);
        List<Project> GetProject(int empid);
        List<Position> GetPosition(int empid, string projectCode);
        List<TimeShiftDTO> GetWorkround(int empid, string projectCode, string positionID);
    }
}
