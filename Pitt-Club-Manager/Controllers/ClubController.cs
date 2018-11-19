using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Firebase.Auth;
using Firebase.Database;
using Google.Cloud.Firestore;
using Grpc.Core;
using Grpc.Core.Logging;
using PittClubManager.Models;
using PittClubManager.Util;

namespace PittClubManager.Controllers
{
    public class ClubController : Controller
    {
        /** 
         * endpont: /club/id/{id}
         * returns a club dashboard for a club of a given id
         **/

        String EMAIL = "jmaciak14@gmail.com";
        String PASSWORD = "password";

        private void DummyWebRequest() {
            string html = string.Empty;
            string url = @"https://api.stackexchange.com/2.2/answers?order=desc&sort=activity&site=stackoverflow";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            Console.WriteLine(html);
        }

        public ActionResult Id(String id)
        {
            // todo: this should eventually fetch the club from the db by id
            String s = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            GrpcEnvironment.SetLogger(new ConsoleLogger());
            System.Diagnostics.Debug.WriteLine("Environment var: " + s);

            //Models.User user = FirebaseHelper.GetUser("SAMPLE_USER").Result;
            var c = FirebaseHelper.GetClub(id).Result;
            /*Task<System.Collections.ArrayList> clTask = FirebaseHelper.GetClubList();
            clTask.Wait();
            //System.Collections.ArrayList cl = clTask.Result;
            //System.Diagnostics.Debug.WriteLine(cl.Count);
            for (int i = 0; i < cl.Count; i++)
            {
                Club cur = (PittClubManager.Models.Club)cl[i];
                System.Diagnostics.Debug.WriteLine(cur.GetId());
            }*/
            return View("Id", c);
        }
    }
}
