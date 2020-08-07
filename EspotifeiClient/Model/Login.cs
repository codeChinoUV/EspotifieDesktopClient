using System;
using Newtonsoft.Json;

namespace Model
{
    [JsonObject]
    [Serializable]
    public class Login
    {
        public string User { get; set; }

        public string Password { get; set; }
        
    }
}