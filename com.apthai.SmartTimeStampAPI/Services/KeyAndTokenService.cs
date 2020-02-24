using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.apthai.SmartTimeStampAPI.Services
{
    public static class KeyAndTokenExtention
    {
      static public List<KeyAndTokenObject> KeyAndTokenProvider = new List<KeyAndTokenObject>();
    }
    public class KeyAndTokenObject
    {
        public string ApiHeader { get; set; }
        public string ApiKey { get; set; }
    }
}
