using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Firebase.Auth;
using Firebase.Database;
using Google.Cloud.Firestore;
using PittClubManager.Models;

namespace PittClubManager.Controllers
{
    public class ClubController : Controller
    {
        /** 
         * endpont: /club/id/{id}
         * returns a club dashboard for a club of a given id
         **/

        String EMAIL = "";
        String PASSWORD = "";

        public ActionResult Id(int id)
        {
            // todo: this should eventually fetch the club from the db by id
            Club club = GetClub(id).Result;
            return View("Id", club);
        }

        private async Task<Club> GetClub(int id)
        {

            FirebaseClient firebase = new FirebaseClient("https://pitt-club-manager.firebaseio.com");
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig("WEB_API_KEY"));

            await authProvider.SignInWithEmailAndPasswordAsync(EMAIL, PASSWORD).ContinueWith(async task =>
            {
                if (task.IsCanceled)
                {
                    System.Console.WriteLine("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    System.Console.WriteLine("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                FirebaseAuth newUser = task.Result;


                var db = FirestoreDb.Create("pitt-club-manager");
                DocumentReference docRef = db.Collection("clubs").Document(id.ToString());
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            });

            Club club = new Club();


            // Fill in club details here with values from firebase


            /*mock.SetId(id);
            mock.SetName("Pitt Lawn Sports");
            mock.SetManager(new Models.User(1, UserPermission.MANAGER, "Josh", "Maciak"));
            mock.SetMembers(new Models.User[] { new Models.User(2, UserPermission.MANAGER, "Member", "McMemberface") });
            mock.SetEvents(new Event[] { new Event(1, DateTime.Now, "Party", "A super cool party") });*/
            return club;

        }
    }
}
