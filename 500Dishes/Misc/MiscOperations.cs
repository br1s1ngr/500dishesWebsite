using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace _500Dishes.Misc
{
    public static class MiscOperations
    {
        public static string SHA1HashStringForUTF8String(string s)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);

            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(bytes);
                return HexStringFromBytes(hashBytes);
            }
        }

        public static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new System.Text.StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        public static async Task<string> SendMail(string recipient, string subject, string content, HttpPostedFileBase file = null)
        {
            var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            var mail = new MailMessage();
            mail.To.Add(new MailAddress(recipient));  // replace with valid value 
            mail.From = new MailAddress("isiaq.oa@gmail.com");  // replace with valid value
            mail.Subject = "Your email subject";
            mail.Body = string.Format(body, "wale A", "isiaq.oa@gmail.com", content);
            mail.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                try
                {
                    var credential = new System.Net.NetworkCredential
                    {
                        UserName = "isiaq.oa@gmail.com",  // replace with valid value
                        Password = "easyhack1"  // replace with valid value
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //smtp.Port = 25;
                    smtp.Port = 465;
                    //smtp.Port = 587;
                    smtp.EnableSsl = true;
                    //smtp.Send(mail);
                    await smtp.SendMailAsync(mail);
                }
                catch (Exception ex)
                {
                    return "error";
                }
            }
            return "sent";
        }

        public static bool ContainsForSearch(this string text, string query)
        {
            if (String.IsNullOrWhiteSpace(query))
                return false;

            //int begin = query.IndexOf("\"");
            //int end = query.LastIndexOf("\"");
            if (query.StartsWith("\"") && query.EndsWith("\""))
            {
                query = query.Remove(0, 1);
                query = query.Remove(query.Length - 1);
                if (text.ToLower().Contains(query.ToLower()))
                    return true;
                return false;
            }

            string[] queryString = query.ToLower().Split(' ');
            string lowerText = text.ToLower();

            foreach (string s in queryString)
                if (lowerText.Contains(s))
                    return true;

            return false;
        }

        public static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            else
            {
                return ((url[0] == '/' && (url.Length == 1 ||
                        (url[1] != '/' && url[1] != '\\'))) ||   // "/" or "/foo" but not "//" or "/\"
                        (url.Length > 1 &&
                         url[0] == '~' && url[1] == '/'));   // "~/" or "~/foo"
            }
        }
    }
}