using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class PaypalConfiguration
    {
        //Variables for storing the clientID and clientSecret key
        public readonly static string ClientId;
        public readonly static string ClientSecret;
        //Constructor
        static PaypalConfiguration()
        {
            var config = GetConfig();
            ClientId = "AR4DX16lAGdXowbBIcl_QgglW3ffUvVRxDV2B4PPQ1WhQTXHbrqmsNwkAvY3wue01t-1j-cSFEaEIel1";
            ClientSecret = "EJng7OIjEgxyCNYIk08ZQ0uO2W73s4MWyYwb4ZCjobDrgyeRFlAbBlLT7XC-qm7RdW_Et0lu9NkL6Qdv";
        }
        // getting properties from the web.config
        public static Dictionary<string, string> GetConfig()
        {
            return PayPal.Api.ConfigManager.Instance.GetProperties();
        }
        private static string GetAccessToken()
        {
            // getting accesstocken from paypal               
            string accessToken = new OAuthTokenCredential
                    (ClientId, ClientSecret, GetConfig()).GetAccessToken();
            return accessToken;
        }
        public static APIContext GetAPIContext()
        {
            // return apicontext object by invoking it with the accesstoken
            APIContext apiContext = new APIContext(GetAccessToken());
            apiContext.Config = GetConfig();
            return apiContext;
        }
    }
}