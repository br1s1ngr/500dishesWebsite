using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using RestSharp;
using System.Runtime.Serialization.Json;

namespace _500Dishes.Misc
{
    public static class PayStackAPI
    {
        const string SECRET_KEY = "sk_test_a369777638adcda058592959a2716543e0861b6f";
        const string PUBLIC_KEY = "";

        public static async Task<string> CreatePlan(string planName, string planDescription, int planAmount)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            const string INTERVAL = "weekly";
            string uri = "https://api.paystack.co/plan";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.paystack.co/plan");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SECRET_KEY);


                //RestClient c = new RestClient("https://api.paystack.co/plan");
                //var request = new RestRequest("", Method.POST);
                //request.AddHeader("Authorization", "Bearer " + SECRET_KEY);
                //request.AddHeader("Content-Type", "application/json");
                //request.AddParameter("name", planName);
                //request.AddParameter("interval", "weekly");
                //request.AddParameter("amount", (planAmount * 100));

                ////IRestResponse resp = c.Execute(request);
                ////var content = resp.Content;
                //bool status = false;
                //object a;
                //c.ExecuteAsync(request, response =>
                //{
                //    a = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);
                //    status = response.StatusCode == HttpStatusCode.Created ? true : false;
                //});

                //return status;


                var postData = new JsonObject();
                postData.Add("name", planName);
                postData.Add("interval", INTERVAL);
                postData.Add("amount", planAmount * 100);

                //var args = new
                //{
                //    name = planName,
                //    interval = INTERVAL,
                //    amount = planAmount * 100
                //};
                //var param = Newtonsoft.Json.JsonConvert.SerializeObject(new
                //{
                //    name = planName,
                //    interval = INTERVAL,
                //    amount = planAmount * 100
                //});
                //WebRequest req = WebRequest.Create()
                //System.Net.WebClient().DownloadString("");
                //var xxx = new WebClient();


                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(uri, postData);
                    //HttpResponseMessage response = await client.PostAsync(uri, new StringContent(abc.ToString(), System.Text.Encoding.UTF8, "application/json"));
                    response.EnsureSuccessStatusCode();    // Throw if not a success code.
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    var responseAsJSon = Newtonsoft.Json.JsonConvert.DeserializeObject<PayStackClass>(responseMsg);
                    if (!responseAsJSon.status)
                        return null;
                    foreach (var item in responseAsJSon.data)
                        if (item.Key == "plan_code")
                            return item.Value.ToString();
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
            }
            return null;
        }

        // finish up
        public static async Task<bool> UpdatePlan(string planCode, string planName, string planDescription, int planAmount, string planInterval, bool sendInvoices, bool sendSms, string currency)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            string uri = "https://api.paystack.co/plan";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.paystack.co/plan/planCode");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SECRET_KEY);

                var postData = new JsonObject();
                postData.Add("name", planName);
                postData.Add("interval", planInterval);
                postData.Add("amount", planAmount * 100);
                postData.Add("description", planDescription);
                postData.Add("send_invoices", sendInvoices);
                postData.Add("send_sms", sendSms);
                postData.Add("currency", currency);

                try
                {
                    HttpResponseMessage response = await client.PutAsJsonAsync(uri, postData);
                    
                    response.EnsureSuccessStatusCode();    // Throw if not a success code.
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    var responseAsJSon = Newtonsoft.Json.JsonConvert.DeserializeObject<PayStackClass>(responseMsg);
                    return responseAsJSon.status;
                }
                catch (HttpRequestException e)
                {
                    return false;
                }
            }
        }

        public static async Task<string> CreateCustomer(string customerEmail, string f_name, string l_name, string customerPhone)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            string uri = "https://api.paystack.co/customer";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.paystack.co/customer");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SECRET_KEY);

                var postData = new JsonObject();
                postData.Add("email", customerEmail);
                postData.Add("phone", customerPhone);
                postData.Add("first_name", f_name);
                postData.Add("last_name", l_name);

                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(uri, postData);
                    response.EnsureSuccessStatusCode();    // Throw if not a success code.
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    var responseAsJSon = Newtonsoft.Json.JsonConvert.DeserializeObject<PayStackClass>(responseMsg);
                    if (!responseAsJSon.status)
                        return null;
                    foreach (var item in responseAsJSon.data)
                        if (item.Key == "customer_code")
                            return item.Value.ToString();
                }
                catch (HttpRequestException e)
                { return null; }
            }
            return null;
        }

        public static async Task<string> CreateSubscription(string customerEmail, string planCode)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            string uri = "https://api.paystack.co/subscription";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.paystack.co/subscription");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SECRET_KEY);

                var postData = new JsonObject();
                postData.Add("customer", customerEmail);
                postData.Add("plan", planCode);

                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(uri, postData);
                    //response.EnsureSuccessStatusCode();    // Throw if not a success code.
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    var responseAsJSon = Newtonsoft.Json.JsonConvert.DeserializeObject<PayStackClass>(responseMsg);
                    if (!responseAsJSon.status)
                        return null;
                    string ss = "";
                    foreach (var item in responseAsJSon.data)
                    {
                        if (item.Key == "subscription_code")
                            ss += item.Value.ToString();
                        if (item.Key == "email_token")
                            ss += "%" + item.Value.ToString();
                    }
                    return ss;
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
            }
        }

        public static async Task<string> FetchSubscription(string subscriptionCode)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            string uri = "https://api.paystack.co/subscription/" + subscriptionCode;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.paystack.co/subscription/"+subscriptionCode);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SECRET_KEY);
                
                try
                {
                    HttpResponseMessage response = await client.GetAsync(uri);
                    //response.EnsureSuccessStatusCode();    // Throw if not a success code.
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    var responseAsJSon = Newtonsoft.Json.JsonConvert.DeserializeObject<PayStackClass>(responseMsg);
                    if (!responseAsJSon.status)
                        return null;
                    
                    return responseAsJSon.data["email_token"].ToString();
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
            }
        }

        public static async Task<bool> EnableSubscription(string subscriptionCode, string token)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            string uri = "https://api.paystack.co/subscription/enable";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.paystack.co/subscription/enable");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SECRET_KEY);

                var postData = new JsonObject();
                postData.Add("code", subscriptionCode);
                postData.Add("token", token);

                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(uri, postData);
                    //response.EnsureSuccessStatusCode();    // Throw if not a success code.
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    var responseAsJSon = Newtonsoft.Json.JsonConvert.DeserializeObject<PayStackClass>(responseMsg);
                    return responseAsJSon.status;
                }
                catch (HttpRequestException e)
                {
                    return false;
                }
            }
        }

        public static async Task<bool> DisableSubscription(string subscriptionCode, string token)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            string uri = "https://api.paystack.co/subscription/disable";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.paystack.co/subscription/disable");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SECRET_KEY);

                var postData = new JsonObject();
                postData.Add("code", subscriptionCode);
                postData.Add("token", token);

                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(uri, postData);
                    //response.EnsureSuccessStatusCode();    // Throw if not a success code.
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    var responseAsJSon = Newtonsoft.Json.JsonConvert.DeserializeObject<PayStackClass>(responseMsg);
                    return responseAsJSon.status;
                }
                catch (HttpRequestException e)
                {
                    return false;
                }
            }
        }

        public static async Task<string> InitializeTransaction(int amount, string customerEmail, string planCode, string callbackUrl)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            string uri = "https://api.paystack.co/transaction/initialize";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.paystack.co/transaction/initialize");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SECRET_KEY);

                var postData = new JsonObject();
                postData.Add("amount", (amount * 100).ToString());
                postData.Add("email", customerEmail);
                postData.Add("plan", planCode);
                postData.Add("callback_url", callbackUrl);

                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(uri, postData);
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    var responseAsJSon = Newtonsoft.Json.JsonConvert.DeserializeObject<PayStackClass>(responseMsg);
                    if (!responseAsJSon.status)
                        return null;
                    string url_trxref = "";
                    foreach (var item in responseAsJSon.data)
                    {
                        if (item.Key == "authorization_url")
                            url_trxref += item.Value.ToString();
                        else if (item.Key == "reference")
                            url_trxref += "%" + item.Value.ToString();
                    }
                    return url_trxref;
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
            }
            return null;
        }

        public static async Task<bool> VerifyTransaction( string trxref)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            string uri = "https://api.paystack.co/transaction/verify/" + trxref;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.paystack.co/transaction/initialize");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SECRET_KEY);

                try
                {
                    HttpResponseMessage response = await client.GetAsync(uri);
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    var responseAsJSon = Newtonsoft.Json.JsonConvert.DeserializeObject<PayStackClass>(responseMsg);
                    return responseAsJSon.status;
                }
                catch (HttpRequestException e)
                {
                    return false;
                }
            }
        }

    }
}