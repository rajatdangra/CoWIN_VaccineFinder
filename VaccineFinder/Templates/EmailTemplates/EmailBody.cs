using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VaccineFinder.Templates.EmailTemplates
{
    public class EmailBody
    {
        public static string CreateSlotsAvailableEmailBody(string templatePath, string userName, string pinCodes, string slotDetails, string url, string urlDescription)
        {
            string body = string.Empty;

            //using streamreader for reading my htmltemplate   
            using (StreamReader reader = new StreamReader(templatePath/*Server.MapPath("~/HtmlTemplate.html")*/))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{UserName}", userName); //replacing the required things  
            body = body.Replace("{PinCodes}", pinCodes);
            body = body.Replace("{SlotDetails}", slotDetails);
            body = body.Replace("{URL}", url);
            body = body.Replace("{URLDescription}", urlDescription);

            return body;
        }

        public static string CreateSlotsBookedEmailBody(string templatePath, string userName, string slotDetails, string url, string urlDescription)
        {
            string body = string.Empty;

            //using streamreader for reading my htmltemplate   
            using (StreamReader reader = new StreamReader(templatePath/*Server.MapPath("~/HtmlTemplate.html")*/))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{UserName}", userName); //replacing the required things  
            body = body.Replace("{BookingDetails}", slotDetails);
            body = body.Replace("{URL}", url);
            body = body.Replace("{URLDescription}", urlDescription);

            return body;
        }
    }
}
