using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.apthai.SmartTimeStampAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Swashbuckle.AspNetCore.Annotations;
using com.apthai.SmartTimeStampAPI.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;
using com.apthai.SmartTimeStampAPI.Model;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace com.apthai.SmartTimeStampAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileController : ControllerBase
    {
        private readonly IMobileRepository _mobileRepo;
        private readonly IHostingEnvironment _hostingEnvironment;

        public MobileController(IMobileRepository smsRep, IHostingEnvironment environment)
        {
            _mobileRepo = smsRep;
            _hostingEnvironment = environment;
        }

        [ApiExplorerSettings(IgnoreApi = true)]//จะไม่ gen ใน Swagger
        public bool VerifyHeader(out string ErrorMsg)
        {
            StringValues api_key;
            StringValues api_token;

            string authHeaderKey = SettingServiceProvider.API_Key;
            string authHeaderToken = SettingServiceProvider.API_Token;

            if (Request.Headers.TryGetValue("api_key", out api_key) && Request.Headers.TryGetValue("api_token", out api_token))
            {
                if (api_key == authHeaderKey && (api_token == authHeaderToken))
                {
                    ErrorMsg = "";
                    return true;
                }
            }

            else
            {

                ErrorMsg = " :: Missing Authorization Header.";
                return false;

            }

            ErrorMsg = "SomeThing Wrong with Header Contact Developer ASAP";
            return false;
        }

        [HttpPost("Register")]
        [SwaggerOperation(Summary = "ลงทะเบียน User", Description = "" +
            "1.ต้องส่ง เบอร์โทรศัพท์,ชื่อ,นามสกุล,รหัสผ่าน,DeviceID" + "<br/>" +
            "2.ข้อมูล เบอร์โทรศัพท์,ชื่อ,นามสกุล จะต้องตรงกับ  => table SS_Employee Status=1" + "<br/>" +
            "3.เบอร์โทรนี้จะต้องยังไม่เคย Register ไม่มีค่าใน SS_M_Register IsActive=1")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> Register([FromBody]Param_Register param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.FirstName == null || param.FirstName.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ ชื่อ"
                };
            }
            if (param.LastName == null || param.LastName.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ นามสกุล"
                };
            }
            if (param.Password == null || param.Password.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ รหัสผ่าน"
                };
            }
            if (param.DeviceID == null || param.DeviceID.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceID"
                };
            }
            #endregion

            #region VerifyRegister
            //Find employee by PhoneNO,FirstName,LastName
            Employee emp = _mobileRepo.GetEmployeeProfile(param.FirstName, param.LastName, param.PhoneNO);
            if (emp == null)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "ไม่พบข้อมูล ชื่อ,นามสกุล,เบอร์โทร นี้ในระบบ กรุณาตรวจสอบรายละเอียด หรือ ติดต่อเจ้าหน้าที่"
                };
            }
            if (_mobileRepo.CheckRegisterStatus(emp.EmpID))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "เบอร์โทรนี้ทำการลงทะเบียนแล้ว ไม่สามารถลงทะเบียนซ้ำได้"
                };
            }
            #endregion

            SS_M_Register regisParam = new SS_M_Register();
            regisParam.PhoneNO = CleanPhoneNo(param.PhoneNO);
            regisParam.DeviceKey = Guid.NewGuid().ToString();
            regisParam.FirstName = param.FirstName;
            regisParam.LastName = param.LastName;
            regisParam.AndroidVersion = param.AndroidVersion;
            regisParam.DeviceID = param.DeviceID;
            regisParam.EmpID = emp.EmpID;
            regisParam.FirebaseKey = param.FirstName;
            regisParam.IMEI = param.IMEI;
            regisParam.MobileBrand = param.MobileBrand;
            regisParam.MobileModel = param.MobileModel;
            regisParam.Password = param.Password;

            #region SaveToDB
            try
            {
                regisParam.RegisterID = _mobileRepo.Register(regisParam);
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = "",
                    message = "พบข้อผิดพลาดในการบันทึกข้อมูล\r\n406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = regisParam.RegisterID,
                message = "ลงทะเบียนสำเร็จ"
            };
        }

        [HttpPost("ChangePassword")]
        [SwaggerOperation(Summary = "เปลี่ยน Password", Description = "" +
            "1.ต้องส่ง เบอร์โทรศัพท์,รหัสผ่าน(ใหม่),DeviceID" + "<br/>" +
            "2.ข้อมูล เบอร์โทรศัพท์,DeviceID จะต้องทำการ Register แล้ว  => table SS_M_Register IsActive=1" + "<br/>")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> ChangePassword([FromBody]Param_ChangePassword param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.Password == null || param.Password.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ รหัสผ่าน"
                };
            }
            if (param.DeviceID == null || param.DeviceID.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceID"
                };
            }
            #endregion

            #region VerifyRegister
            //Find employee by PhoneNO,FirstName,LastName
            Employee emp = _mobileRepo.GetEmployeeProfileByDeviceID(param.DeviceID, param.PhoneNO);
            if (emp == null)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "ไม่พบข้อมูล Device ID,เบอร์โทร นี้ในระบบ กรุณาตรวจสอบรายละเอียด หรือ ติดต่อเจ้าหน้าที่"
                };
            }
            if (!_mobileRepo.CheckRegisterStatus(emp.EmpID))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "เบอร์โทรนี้ยังไม่ลงทะเบียน"
                };
            }
            #endregion

            #region SaveToDB
            try
            {
                _mobileRepo.ChangePassword(emp.EmpID, param.Password.Trim());
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "พบข้อผิดพลาดในการบันทึกข้อมูล\r\n406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = new Object(),
                message = "เปลี่ยนรหัสผ่านสำเร็จ"
            };
        }

        [HttpPost("Login")]
        [SwaggerOperation(Summary = "Login เข้าระบบ", Description = "" +
            "1.ต้องส่ง เบอร์โทรศัพท์,รหัสผ่าน,DeviceID" + "<br/>" +
            "2.เบอร์โทรศัพท์ นี้จะต้องทำการ register แล้ว => table SS_M_Register IsActive=1" + "<br/>" +
            "3.เมื่อ Login สำเร็จ จะ return DeviceKey กลับไปให้" + "<br/>" +
            "4.เมื่อ Login สำเร็จ เก็บประวัติการ Login ไว้ที่ SS_M_Login")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> Login([FromBody]Param_Login param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            string phoneNo = "";
            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.Password == null || param.Password.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ รหัสผ่าน"
                };
            }
            if (param.DeviceID == null || param.DeviceID.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceID"
                };
            }
            phoneNo = CleanPhoneNo(param.PhoneNO);
            #endregion

            #region VerifyLogin
            Employee emp = GetEmployeeByDeviceID(param.DeviceID, phoneNo);
            if (emp == null)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "ไม่พบข้อมูลการลงทะเบียนของหมายเลข " + param.PhoneNO + " กรุณาลงทะเบียนหรือติดต่อเจ้าหน้าที่"
                };
            }
            #endregion
            SS_M_LoginV2 loginParam = new SS_M_LoginV2();
            loginParam.AndroidVersion = param.AndroidVersion;
            loginParam.DeviceID = param.DeviceID;
            loginParam.EmpID = emp.EmpID;
            loginParam.IMEI = param.IMEI;
            loginParam.MobileBrand = param.MobileBrand;
            loginParam.MobileModel = param.MobileModel;
            loginParam.Password = param.Password;
            loginParam.PhoneNO = param.PhoneNO;

            #region SaveToDB
            try
            {
                _mobileRepo.Login(loginParam);
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "พบข้อผิดพลาดในการบันทึกข้อมูล\r\n406: " + er.Message
                };
            }
            #endregion
            Param_ReturnLogin obj = new Param_ReturnLogin();
            obj.DeviceKey = emp.DeviceKey;
            //
            return new
            {
                success = true,
                data = obj,// JsonConvert.SerializeObject(obj).Replace("\\",""),
                message = "เข้าสู่ระบบสำเร็จ"
            };
        }

        [HttpPost("UpdateFirebaseKey")]
        [SwaggerOperation(Summary = "Update Key จาก Firebase เข้าระบบ", Description = "" +
            "1.ต้องส่ง DeviceKey,FirebaseKey" + "<br/>" +
            "2.ตรวจสอบ DeviceKey ว่ามีในระบบ หรือไม่ => table SS_M_Register IsActive=1" + "<br/>" +
            "3.Update ค่า FirebaseKey ที่ table SS_M_Register")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> UpdateFirebaseKey([FromBody]Param_FirebaseKey param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.DeviceKey == null || param.DeviceKey.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceKey"
                };
            }
            if (param.FirebaseKey == null || param.FirebaseKey.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ Firebase Key"
                };
            }
            if (!VerifyDeviceKey(param.DeviceKey.Trim()))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "ไม่พบ DeviceKey นี้ในระบบ"
                };
            }
            #endregion

            #region SaveToDB
            try
            {
                _mobileRepo.UpdateFirebaseKey(param.DeviceKey.Trim(), param.FirebaseKey.Trim());
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "พบข้อผิดพลาดในการบันทึกข้อมูล\r\n406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = new Object(),
                message = "Update Firebase key สำเร็จ"
            };
        }

        [HttpPost("ForgotPassword")]
        [SwaggerOperation(Summary = "ทำการ Reset Password แล้วส่ง Password เป็น SMS ให้ User", Description = "ยังทำไม่เสร็จ เหลือการส่ง sms")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> ForgotPassword([FromBody]Param_ForgotPassword param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion


            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.DeviceID == null || param.DeviceID.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceID"
                };
            }
            #endregion

            #region SaveToDB
            try
            {
                Employee emp = GetEmployeeByDeviceID(param.DeviceID, param.PhoneNO);
                if (emp == null)
                {
                    Employee obj = new Employee();
                    return new
                    {
                        success = false,
                        data = obj,
                        message = "ไม่พบข้อมูล DeviceID ในระบบ"
                    };
                }

                //Gen new password
                //string newPassword = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6).ToLower().Replace("o","a");
                string newPassword = new Random().Next(100000, 999999).ToString("000000");

                //Send new password sms to user
                var client = new HttpClient();
                param.PhoneNO = "0888296298";
                string str = "[{'MobileNumber': '" + param.PhoneNO + "','SendByApp': 'SmartTimeStampAPI','Message': 'ระบบได้ทำการเปลี่ยนรหัสผ่านเข้าระบบ Smart Time Stamp เป็น " + newPassword + "','Ref1':'ChangePassword'}]";
                var content = new StringContent(str, Encoding.UTF8);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                //content.Headers.ContentType = new En("application/json");
                //content.Headers.ContentEncoding = Encoding.UTF8;
                //content.Headers.Add("api_key", "z66PDOx/wrdRcfI38UAWy+eb6pw7ivpNdpz+eYJQScNX19mbFiA87KZvF2/qnLx+6JWUaNbBSrUtC4rYjOB4HIayTQU=");
                //content.Headers.Add("api_token", "CtOcOl54qoQVGAp6XaRnfI/PC/77cjN6c4tVj76uXwT+sEjnimRK8j2Dw+7uWPtqSSpt+rr/vZcswsd1o+V1phuvsBv0Ag==");
                string PostUrl = SettingServiceProvider.SMSApiUrl;// webdirectory + "api/authorize/UserLogin";
                var respond = await client.PostAsync(PostUrl, content);
                if (respond.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    //return new AutorizeData();
                }
                //var responddata = await respond.Content.ReadAsStringAsync();
                //AutorizeDataJWT result = JsonConvert.DeserializeObject<AutorizeDataJWT>(responddata);
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = new Object(),
                message = "เปลี่ยนรหัสผ่านสำเร็จ ระบบจะทำการส่งรหัสผ่านใหม่ไปให้ท่านทาง SMS"
            };
        }

        [HttpPost("GetEmployeeProfile")]
        [SwaggerOperation(Summary = "ดึงข้อมูลรายละเอียด User", Description = "ยังทำไม่เสร็จ")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> GetEmployeeProfile([FromBody]Param_GetProfile param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.DeviceKey == null || param.DeviceKey.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceKey"
                };
            }
            #endregion

            #region SaveToDB
            Employee emp = null;
            try
            {
                emp = _mobileRepo.GetEmployeeProfileByDeviceKeyAndDetail(param.DeviceKey, param.PhoneNO);
                if (emp == null)
                {
                    return new
                    {
                        success = false,
                        data = new Object(),
                        message = "ไม่สามารถโหลดข้อมูลพนักงานได้ ไม่พบ DeviceKey นี้ในระบบ"
                    };
                }

            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = emp,
                    message = "406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = emp,
                message = "ดึงข้อมูล User สำเร็จ"
            };
        }

        [HttpPost("GetProject")]
        [SwaggerOperation(Summary = "ดึงข้อมูล โครงการ ที่ User สามารถเข้างานได้", Description = "ยังทำไม่เสร็จ"
            + "1.ต้องส่ง PhoneNO,DeviceKey" + "<br/>"
            + "" + "<br/>"
            + "" + "<br/>"
            + "" + "<br/>")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> GetProject([FromBody]Param_GetProject param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.DeviceKey == null || param.DeviceKey.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceKey"
                };
            }
            #endregion

            #region GetFromDB
            object proj = null;
            try
            {
                Employee emp = GetEmployeeByDeviceKey(param.DeviceKey, param.PhoneNO);
                if (emp == null)
                {
                    return new
                    {
                        success = false,
                        data = new Object(),
                        message = "ไม่สามารถโหลดข้อมูลโครงการได้ ไม่พบ DeviceKey นี้ในระบบ"

                    };
                }

                proj = _mobileRepo.GetProject(emp.EmpID);

            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = proj,
                message = "ดึงข้อมูล Project สำเร็จ"
            };
        }

        [HttpPost("GetPosition")]
        [SwaggerOperation(Summary = "ดึงข้อมูล ตำแหน่ง ที่ User สามารถเข้างานได้", Description = "ยังทำไม่เสร็จ"
            + "1.ต้องส่ง PhoneNO,DeviceKey" + "<br/>"
            + "" + "<br/>"
            + "" + "<br/>"
            + "" + "<br/>")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> GetPosition([FromBody]Param_GetPosition param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.DeviceKey == null || param.DeviceKey.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceKey"
                };
            }
            if (param.ProjectCode == null || param.ProjectCode.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ ProjectCode"
                };
            }
            #endregion

            #region GetFromDB
            object obj = null;
            try
            {
                Employee emp = GetEmployeeByDeviceKey(param.DeviceKey, param.PhoneNO);
                if (emp == null)
                {
                    return new
                    {
                        success = false,
                        data = new Object(),
                        message = "ไม่สามารถโหลดข้อมูลตำแหน่งได้ ไม่พบ DeviceKey นี้ในระบบ"
                    };
                }
                obj = _mobileRepo.GetPosition(emp.EmpID, param.ProjectCode);
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = obj,
                message = "ดึงข้อมูล Position สำเร็จ"
            };
        }

        [HttpPost("GetTimeShift")]
        [SwaggerOperation(Summary = "ดึงข้อมูล กะงาน ที่ User สามารถเข้างานได้", Description = "ยังทำไม่เสร็จ"
            + "1.ต้องส่ง PhoneNO,DeviceKey" + "<br/>"
            + "" + "<br/>"
            + "" + "<br/>"
            + "" + "<br/>")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> GetTimeShift([FromBody]Param_GetTimeShift param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.DeviceKey == null || param.DeviceKey.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceKey"
                };
            }
            if (param.ProjectCode == null || param.ProjectCode.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ ProjectCode"
                };
            }
            if (param.PositionID == null || param.PositionID.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ PositionID"
                };
            }
            #endregion

            #region GetFromDB
            object obj = null;
            try
            {
                Employee emp = GetEmployeeByDeviceKey(param.DeviceKey, param.PhoneNO);
                if (emp == null)
                {
                    return new
                    {
                        success = false,
                        data = new Object(),
                        message = "ไม่สามารถโหลดข้อมูลกะงานได้ ไม่พบ DeviceKey นี้ในระบบ"
                    };
                }
                obj = _mobileRepo.GetWorkround(emp.EmpID, param.ProjectCode, param.PositionID);
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = obj,
                message = "ดึงข้อมูล Time Shift สำเร็จ"
            };
        }

        [HttpPost("CheckIn")]
        [SwaggerOperation(Summary = "บันทึกลงเวลาเข้างาน", Description = "ยังทำไม่เสร็จ")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> CheckIn([FromBody]Param_CheckInOut param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.DeviceKey == null || param.DeviceKey.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceKey"
                };
            }
            #endregion

            CheckInOut check = new CheckInOut();
            check.EmpID = param.EmpID;
            check.CheckInOutType = "CheckIn";
            check.PositionID = param.PositionID;
            check.EmpTypeID = param.EmpTypeID;
            check.WorkRoundID = param.WorkRoundID;
            check.ProjectCode = param.ProjectCode;

            #region SaveToDB
            DateTime checkInOutTime = DateTime.Now;
            try
            {
                checkInOutTime = _mobileRepo.CheckInOut(check);
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = checkInOutTime,
                message = "บันทึกเข้างานสำเร็จ"
            };
        }

        [HttpPost("CheckOut")]
        [SwaggerOperation(Summary = "บันทึกลงเวลาออกงาน", Description = "ยังทำไม่เสร็จ")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> CheckOut([FromBody]Param_CheckInOut param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.DeviceKey == null || param.DeviceKey.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceKey"
                };
            }
            #endregion

            CheckInOut check = new CheckInOut();
            check.EmpID = param.EmpID;
            check.CheckInOutType = "CheckOut";
            check.PositionID = param.PositionID;
            check.EmpTypeID = param.EmpTypeID;
            check.WorkRoundID = param.WorkRoundID;
            check.ProjectCode = param.ProjectCode;

            #region SaveToDB
            DateTime checkInOutTime = DateTime.Now;
            try
            {
                checkInOutTime = _mobileRepo.CheckInOut(check);
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = checkInOutTime,
                message = "บันทึกออกงานสำเร็จ"
            };
        }

        [HttpPost("GetHistory")]
        [SwaggerOperation(Summary = "ดึงข้อมูลประวัติการเข้าออกงานของ User", Description = "ยังทำไม่เสร็จ")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> GetHistory([FromBody]Param_GetHistory param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.DeviceKey == null || param.DeviceKey.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceKey"
                };
            }
            #endregion


            #region SaveToDB
            List<CheckInOut> check = new List<CheckInOut>();
            try
            {
                Employee emp = GetEmployeeByDeviceKey(param.DeviceKey, param.PhoneNO);
                if (emp == null)
                {
                    return new
                    {
                        success = false,
                        data = new Object(),
                        message = "ไม่สามารถโหลดข้อมูลตำแหน่งได้ ไม่พบ DeviceKey นี้ในระบบ"
                    };
                }
                check = _mobileRepo.GetCheckInOutHistory(emp.EmpID);
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = check,
                message = "ดึงข้อมูลประวัติการลงเวลาสำเร็จ"
            };
        }

        [HttpPost("GetNews")]
        [SwaggerOperation(Summary = "ดึงข้อมูลข่าว", Description = "ยังทำไม่เสร็จ")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> GetNews([FromBody]Param_GetNews param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.DeviceKey == null || param.DeviceKey.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceKey"
                };
            }
            #endregion

            #region GetFromDB
            object obj = null;
            try
            {
                Employee emp = GetEmployeeByDeviceKey(param.DeviceKey, param.PhoneNO);
                if (emp == null)
                {
                    return new
                    {
                        success = false,
                        data = new Object(),
                        message = "ไม่สามารถโหลดข้อมูลข่าวได้ ไม่พบ DeviceKey นี้ในระบบ"
                    };
                }
                obj = _mobileRepo.GetNews();
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = obj,
                message = "ดึงข้อมูลข่าวสารสำเร็จ"
            };
        }

        [HttpPost("CallFirebase")]
        [SwaggerOperation(Summary = "", Description = "ยังทำไม่เสร็จ")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        [SwaggerResponse(404, "BAD REQUEST")]
        [SwaggerResponse(405, "Parameter Error")]
        [SwaggerResponse(406, "Database Error")]
        public async Task<object> CallFirebase([FromBody]Param_Register param)
        {
            #region VerifyHeaderKey
            string ErrorMsg = "";
            if (!VerifyHeader(out ErrorMsg))
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = ErrorMsg
                };
            }
            #endregion

            #region ValidateParameter
            if (param.PhoneNO == null || param.PhoneNO.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ เบอร์โทรศัพท์"
                };
            }
            if (param.FirstName == null || param.FirstName.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ ชื่อ"
                };
            }
            if (param.LastName == null || param.LastName.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ นามสกุล"
                };
            }
            if (param.Password == null || param.Password.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ รหัสผ่าน"
                };
            }
            if (param.DeviceID == null || param.DeviceID.Trim() == "")
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "กรุณาระบุ DeviceID"
                };
            }
            #endregion

            #region VerifyRegister

            #endregion

            SS_M_Register regisParam = new SS_M_Register();
            regisParam.PhoneNO = param.PhoneNO.Replace(" ", "").Replace("-", "");

            #region SaveToDB
            try
            {
                regisParam.RegisterID = _mobileRepo.Register(regisParam);
            }
            catch (Exception er)
            {
                return new
                {
                    success = false,
                    data = new Object(),
                    message = "406: " + er.Message
                };
            }
            #endregion
            //
            return new
            {
                success = true,
                data = regisParam.RegisterID,
                message = "ลงทะเบียนสำเร็จ"
            };
        }

        [HttpGet("GetActiveAppVersion")]
        //[Route("appversion")]
        [SwaggerOperation(Summary = "", Description = "ยังทำไม่เสร็จ")]
        [SwaggerResponse(200, "เสร็จสมบูรณ์")]
        public async Task<object> GetActiveAppVersion()
        {
            try
            {
                var dataResult = await _mobileRepo.GetActiveAppVersion();
                return new
                {
                    success = true,
                    data = dataResult
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("upload")]
        [Consumes("multipart/form-data")]
        public async Task<object> Upload([FromForm] ParamUploadPlanImage data)
        {
            try
            {
                List<string> tempUploadPath = new List<string>();
                //List<PlanImageUploadResult> tempUploadFiles = new List<PlanImageUploadResult>();
                string message = "";
                List<SSMTResource> sSMTResources = new List<SSMTResource>();
                if (!string.IsNullOrEmpty(data.sSMTResources))
                    sSMTResources = JsonConvert.DeserializeObject<List<SSMTResource>>(data.sSMTResources);

                if (data.ListPicture != null && data.ListPicture.Count() > 0)
                {
                    //var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "data", "uploads");
                    foreach (var formFile in data.ListPicture)
                    {

                        long size = data.ListPicture.Sum(f => f.Length);

                        var fileName = formFile.FileName;// string.Format("{0}{1}" , DateTime.Now.ToString("DDMMyy") , Path.GetExtension(formFile.FileName)); //Path.GetFileName(Path.GetTempFileName());
                        var yearPath = DateTime.Now.Year;
                        var MonthPath = DateTime.Now.Month;
                        string dataPath = ""; string dataPath2 = "";

                        if (sSMTResources != null && sSMTResources.Count() > 0)
                        {
                            //dataPath = sSMTResources.FirstOrDefault().MobileModel;
                            dataPath = sSMTResources.FirstOrDefault().ProjectID.ToString();
                        }

                        var dirPath = $"{yearPath}/{MonthPath}/P{dataPath}";
                        var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "data", "uploads/" + dirPath);
                        var savePath = new FileInfo(Path.Combine(uploads, fileName));

                        if (!Directory.Exists(uploads))
                        {
                            Directory.CreateDirectory(uploads);
                        }

                        if (formFile.Length > 0)
                        {
                            var filePath = Path.Combine(uploads, formFile.FileName);
                            if (System.IO.File.Exists(filePath))
                            {
                                string fileNameOnly = Path.GetFileNameWithoutExtension(filePath);
                                string extension = Path.GetExtension(filePath);
                                string path = Path.GetDirectoryName(filePath);
                                string newFullPath = filePath;
                                string tempFileName = string.Format("{0}({1})", fileNameOnly, Guid.NewGuid().ToString());
                                newFullPath = Path.Combine(path, tempFileName + extension);
                                using (var fileStream = new FileStream(newFullPath, FileMode.Create))
                                {
                                    message = formFile.Length.ToString();
                                    await formFile.CopyToAsync(fileStream);
                                    fileName = tempFileName;
                                }
                            }
                            else
                            {
                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    message = formFile.Length.ToString();
                                    await formFile.CopyToAsync(fileStream);
                                }
                            }
                            if (System.IO.File.Exists(filePath))
                            {
                                filePath = filePath.Replace("\\", "/");

                                var ImageId = "";
                                if (string.IsNullOrEmpty(ImageId))
                                    ImageId = Guid.NewGuid().ToString("N");

                                var tempUploadFile = savePath.FullName;

                                string mainResizePath = System.IO.Path.Combine(savePath.DirectoryName, string.Format("PLM{0}.jpg", ImageId));
                                mainResizePath = mainResizePath.Replace("\\", "/");
                                // Resize/Crop MainImage
                                var mainImage = ProcessResizeMainImage(tempUploadFile, mainResizePath);
                                // Split MainImage to Multiple Image
                                //var childImages = ProcesSplitImages(string.Format("{0}", ImageId), mainResizePath, savePath.DirectoryName.Replace("\\", "/"));

                                /*--- New Code ---*/

                                long ResFileLength = 0;

                                var tempUploadad = new FileInfo(mainResizePath);
                                ResFileLength = tempUploadad.Length;

                                string ResId = Guid.NewGuid().ToString();

                                SSMTResource sSMT = new SSMTResource();
                                sSMT.ResourceType = 100;
                                sSMT.FileName = fileName;
                                sSMT.FilePath = filePath;
                                sSMT.Description = data.Description;
                                sSMT.IsActive = true;
                                sSMT.CreateDeviceId = data.CreateDeviceId;
                                sSMT.CreateUserId = data.UserID;
                                sSMT.SSMEmpID = data.UserID;
                                sSMT.ClientDataType = "Picture-Check-IN";
                                sSMT.ProjectID = Convert.ToInt32(data.ProjectID);
                                sSMT.MobileModel = data.MobileModel;

                                //For Save Main Image
                                var valid = _mobileRepo.InsertSSMTResource(sSMT);

                            }
                        }
                    }
                }

                return new
                {
                    success = true,
                    data = "",
                    // data = _data.ToList(),
                    //message = string.Format("found {0} items", _data.Count)
                };

            }
            catch (Exception ex)
            {

                return StatusCode(500, "Internal server error :: " + ex.Message);
            }

        }

        private Employee GetEmployeeByDeviceID(string deviceID, string phoneNo)
        {
            Employee emp = _mobileRepo.GetEmployeeProfileByDeviceID(deviceID, phoneNo);
            return emp;
        }

        private Employee GetEmployeeByDeviceKey(string deviceKey, string phoneNo)
        {
            Employee emp = _mobileRepo.GetEmployeeProfileByDeviceKey(deviceKey, phoneNo);
            return emp;
        }

        private string CleanPhoneNo(string phoneNo)
        {
            return phoneNo.Replace(" ", "").Replace("-", "").Trim();
        }

        private bool VerifyDeviceKey(string deviceKey)
        {
            return _mobileRepo.CheckExistingDeviceKey(deviceKey.Trim());
        }

        private FileInfo ProcessResizeMainImage(string srcImagePath, string descImagePath)
        {
            int width = 2000;
            int height = 2000;

            using (Image<Rgba32> img = Image.Load(srcImagePath)) 
            {
                var newImg = img.Clone();
                if (newImg.Height > height || newImg.Width > width)
                {
                    newImg = img.Clone(x => x.Resize(new ResizeOptions()
                    {
                        Mode = ResizeMode.Max,
                        Position = AnchorPositionMode.Center,
                        Size = new SixLabors.Primitives.Size(width, height)
                    }));
                }
                newImg.Save(descImagePath, new JpegEncoder() { Quality = 100 });


                //if (img.Height == img.Width) // vertical
                //{
                //    //Crops to a square (in place)
                //    var newImg = img.Clone(x => x.Resize(new ResizeOptions() { Mode = ResizeMode.Max , Position = AnchorPositionMode.Center , Size = new SixLabors.Primitives.Size(2000,2000)  }));
                //    newImg.Save(descImagePath, new JpegEncoder() { Quality = 90 });
                //    //ImageBuilder.Current.Build(srcImagePath, descImagePath, new ResizeSettings("width=800&height=800&format=jpg&crop=auto"));

                //}
                //else if (img.Height > img.Width) // vertical
                //{
                //    //Crops to a square (in place)
                //    ImageBuilder.Current.Build(srcImagePath, descImagePath, new ResizeSettings("width=600&height=800&format=jpg&crop=auto"));
                //}
                //else
                //{
                //    //Crops to a square (in place)
                //    ImageBuilder.Current.Build(srcImagePath, descImagePath, new ResizeSettings("width=800&height=600&format=jpg&crop=auto"));
                //}

            }


            return new FileInfo(descImagePath);

        }

        //[ApiExplorerSettings(IgnoreApi = true)]// ถ้า Method เป็น Public แล้วไม่ต้องการให้แสดงใน Swagger ให้ใส่ บรรทัดนี้
        //private async Task<bool> func_SaveImageAsync(string ResourceClientId, string dataPath, string dataPath2, string imagePath, string ImageClientID, bool IsMainImage, long fileLength, string Description, string createDeviceId, int? createUserId, DateTime? createDeviceDate, List<SSMTResource> ImageUnitMappingData, string CreatorFullName, int UserID = 0)
        //{
        //    try
        //    {
        //        //var valid = true;

        //        /*--- Step 1 : Add Image Resource ---*/
        //        // var itemImage = await _unitOfWork.MasterRepository.GetImageManagementDetail(ImageId);
        //        SSMTResource sSMTResource = new SSMTResource();
        //        sSMTResource.ProjectID = 
        //        //itemImageRes.ResourceClientId = ResourceClientId;
        //        //itemImageRes.CreatedDate = DateTime.Now;
        //        ////itemImageRes.cre = createDeviceId;
        //        //itemImageRes.CreateUserId = createUserId;
        //        //itemImageRes.Description = Description;

        //        //itemImageRes.IsActive = true;
        //        ////itemImageRes.pro = _ImageUnitMappingData.FirstOrDefault().ProjectID;
        //        //itemImageRes.FileLength = fileLength;

        //        //if (IsMainImage)
        //        //{
        //        //    itemImageRes.ResourceTagCode = "FLMain";
        //        //}
        //        //else
        //        //{
        //        //    itemImageRes.ResourceTagCode = "FLSplit";
        //        //}
        //        //var fileName = Path.GetFileName(imagePath);
        //        //var baseStoragePath = await base.GetQISResourceStoragePhysicalPathAsync(storageServerId);
        //        //var relativeFileName = imagePath.Replace(baseStoragePath, string.Empty);
        //        //var relativeFileNameFixed = relativeFileName.Replace("\\", "/").TrimStart('/');
        //        //itemImageRes.FileName = fileName;
        //        //itemImageRes.FilePath = relativeFileNameFixed.Substring(12);
        //        //itemImageRes.FileLength = fileLength;
        //        //itemImageRes.MineType = GetMimeType(imagePath);
        //        //itemImageRes.IsActive = true;
        //        //itemImageRes.StorageServerId = storageServerId;
        //        //itemImageRes.CreateUserId = UserID;
        //        //itemImageRes.CreatedDate = DateTime.Now;
        //        //itemImageRes.ModifiedUserId = UserID;
        //        //itemImageRes.ModifiedDate = DateTime.Now;

        //        Tuple<bool, string> valid = await _unitOfWork.MasterRepository.SaveImageResource(ImageClientID, itemImageRes, _mapping, dataPath, dataPath2, Description, createDeviceId, createDeviceDate, CreatorFullName, IsMainImage);

        //        return true;


        //    }
        //    catch (Exception ex)
        //    {


        //        return false;

        //    }

        //}

    }
}


