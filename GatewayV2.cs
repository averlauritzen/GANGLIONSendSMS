using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GANGLIONSendSMS
{
    class GatewayV2: IDisposable
    {

        RestSharp.RestClient client = new RestSharp.RestClient("https://gatewayapi.com/rest");
        string apiKey = "OCD_YYTUaFRIBLnWjOKAFwg7";
        string apiSecret = "nuevEigXi!w*^vDfWfHaKZWGT(tZg1dkw3IMkL%)";
        RestSharp.RestRequest request = new RestSharp.RestRequest("mtsms", RestSharp.Method.POST);


        public GatewayV2( )
        {          
            client.Authenticator = RestSharp.Authenticators.OAuth1Authenticator.ForRequestToken(apiKey, apiSecret);      
                    
        }


     public SmsReciept SendThisSms(string afsender, string besked, string modtager)
        {

            SmsReciept reciept = new SmsReciept();

            List<string> smsID = new List<string>();

            request.AddJsonBody(new
            {
                sender = afsender,
                message = besked,
            //  callback_url = "http://5.103.26.75:5005/smsCallbackApI/api/sms",
                callback_url = "http://smsapi.ganglion.dk/api/sms",                
                recipients = new[] { new { msisdn = modtager } }

            });
            var response = client.Execute(request);

            // On 200 OK, parse the list of SMS IDs else print error
            if ((int)response.StatusCode == 200)
            {
                Newtonsoft.Json.Linq.JObject res = Newtonsoft.Json.Linq.JObject.Parse(response.Content);
                foreach (int i in res["ids"])
                {
                    Console.WriteLine(i);
                    reciept.SmsIDNumbers.Add(i.ToString());
                }
                Newtonsoft.Json.Linq.JObject usage = (Newtonsoft.Json.Linq.JObject)res["usage"];

                reciept.Valuta = usage["currency"].ToString();
                reciept.Pris = usage["total_cost"].ToString();
            }
            else if (response.ResponseStatus == RestSharp.ResponseStatus.Completed)
            {
                Console.WriteLine(response.Content);
            }
            else
            {
                Console.WriteLine(response.ErrorMessage);
            }
            return reciept;
        }

        public void Dispose()
        {
            client = null;
            request = null;
        }
    }
}
