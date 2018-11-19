using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1Beta1;
using PittClubManager.Models;

namespace PittClubManager.Util
{
    public class FirebaseHelper
    {
        public const string DB_NAME = "";
        public const string COLLECTION_CLUBS = "clubs";
        public const string COLLECTION_USERS = "users";

        public const string EMAIL = "jmaciak14@gmail.com";
        public const string PASSWORD = "password";
        public const string WEB_KEY = "AIzaSyCN8Av2-nfNtsRdlWaZiaejPdwQ4QqA38c";

        public static async Task<Club> GetClub(string id)
        {
            FirebaseClient firebase = new FirebaseClient("https://pitt-club-manager.firebaseio.com");
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(WEB_KEY));
            Club club = new Club();
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
                DocumentReference docRef = db.Collection("clubs").Document(id);
                DocumentSnapshot snapshot = docRef.GetSnapshotAsync().Result; // this might be improper but I could only get to work by blocking..
                //System.Diagnostics.Debug.WriteLine("Snapshot taken of document " + id + "!");
                var exists = snapshot.Exists;
                if (exists)
                {
                    //System.Diagnostics.Debug.Write(snapshot.Id);
                    //System.Diagnostics.Debug.WriteLine("Club exists!");
                    club = FirebaseSnapshotToClub(snapshot);
                    return;
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("Club nonexistent!");
                    club.SetId("Nonexistent");
                    club.SetName("Nonexistent club");
                    club.SetManager(new Models.User("100", "David", "Dimond"));
                }
            }).ConfigureAwait(false);
            return club;

        }

        public static async Task<ArrayList> GetClubList()
        {
            FirebaseClient firebase = new FirebaseClient("https://pitt-club-manager.firebaseio.com");
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(WEB_KEY));
            ArrayList clubList = new ArrayList();
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
            }).ConfigureAwait(false);
            var db = FirestoreDb.Create("pitt-club-manager");
            IAsyncEnumerable<DocumentSnapshot> clubRef = db.Collection("clubs").StreamAsync();
            int i = 0;
            var enumerator = clubRef.GetEnumerator();
            while (await enumerator.MoveNext())
            {
                //System.Diagnostics.Debug.WriteLine("Found item " + i + "!");
                //System.Diagnostics.Debug.WriteLine(enumerator.Current.Id);

                //System.Diagnostics.Debug.WriteLine("Attempting to add club " + enumerator.Current.Id + "!");
                clubList.Add(GetClub(enumerator.Current.Id).Result);
                //System.Diagnostics.Debug.WriteLine("Added club " + clubList[i] + "!");
                i++;
            }
            System.Diagnostics.Debug.WriteLine("Returning a club list with capacity " + clubList.Capacity + "!");
            return clubList;
        }

        public static async Task<PittClubManager.Models.User> GetUser(string id)
        {
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(WEB_KEY));
            FirebaseAuthLink res = await authProvider.SignInWithEmailAndPasswordAsync(EMAIL, PASSWORD);

            FirebaseClient firebase = new FirebaseClient("https://pitt-club-manager.firebaseio.com", new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(res.FirebaseToken)
            });
            FirestoreDb db = FirestoreDb.Create("pitt-club-manager");
            DocumentSnapshot snap = await db.Collection(COLLECTION_USERS).Document(id).GetSnapshotAsync();
            return UserSnapshotToUser(snap);
        }

        public static Models.User UserSnapshotToUser(DocumentSnapshot snap)
        {
            Models.User user = new Models.User("", "", "");
            try
            {
                user.SetId(snap.Id);
                user.SetFirstName(snap.GetValue<string>("firstName"));
                user.SetLastName(snap.GetValue<string>("lastName"));
                return user;
            }
            catch (Exception exp)
            {
                return null;
            }
        }
        private static Club FirebaseSnapshotToClub(DocumentSnapshot snap)
        {
            Club c = new Club();
            try
            {
                System.Diagnostics.Debug.WriteLine("Firebase snapshot to club!");
                c.SetName(snap.GetValue<string>("name"));
                c.SetId(snap.GetValue<string>("id"));
                String managerId = snap.GetValue<string>("managerId");
                //String[] memberIds = snap.GetValue<string[]>("memberIds");
                //String[] eventIds = snap.GetValue<string[]>("eventIds");

                PittClubManager.Models.User manager = GetUser(managerId).Result;
                c.SetManager(manager);
                System.Diagnostics.Debug.WriteLine("Firebase snapshot to club 2! " + c.GetName());

            }
            catch (Exception exp)
            {
                return null;
            }
            return c;
        }
    }
}
