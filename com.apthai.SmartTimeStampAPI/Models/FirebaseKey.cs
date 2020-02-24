using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace com.apthai.SmartTimeStampAPI.Model
{
    public partial class M_FirebaseKey
    {
        public string PhoneNO { get; set; }
        public string DeviceKey { get; set; }
        public string FirebaseKey { get; set; }
        public DateTime LastUpdateFirebaseKey { get; set; }
    }

    public partial class Param_FirebaseKey
    {
        public string DeviceKey { get; set; }
        public string FirebaseKey { get; set; }
    }
}
