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
        public const string FIREBASE_URL = "https://pitt-club-manager.firebaseio.com";
        public const string DB_NAME = "pitt-club-manager";
        public const string COLLECTION_CLUBS = "clubs";
        public const string COLLECTION_USERS = "users";
        public const string COLLECTION_PENDING_CLUBS = "pendingClubs";
        public const string EMAIL = "jmaciak14@gmail.com";
        public const string PASSWORD = "password";
        public const string WEB_KEY = "AIzaSyCN8Av2-nfNtsRdlWaZiaejPdwQ4QqA38c";

        public static async Task<Club> GetClub(string id)
        {
            FirebaseClient firebase = new FirebaseClient(FIREBASE_URL);
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
                var db = FirestoreDb.Create(DB_NAME);
                DocumentReference docRef = db.Collection(COLLECTION_CLUBS).Document(id);
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
            FirebaseClient firebase = new FirebaseClient(FIREBASE_URL);
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
            var db = FirestoreDb.Create(DB_NAME);
            IAsyncEnumerable<DocumentSnapshot> clubRef = db.Collection(COLLECTION_CLUBS).StreamAsync();
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

        public static PittClubManager.Models.User[] GetUsersFromIds(string[] ids)
        {
            FirebaseClient firebase = new FirebaseClient(FIREBASE_URL);
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(WEB_KEY));
            FirebaseAuthLink res = authProvider.SignInWithEmailAndPasswordAsync(EMAIL, PASSWORD).Result;
            FirestoreDb db = FirestoreDb.Create(DB_NAME);
            QuerySnapshot snap = db.Collection(COLLECTION_USERS).GetSnapshotAsync().Result;
            ArrayList users = new ArrayList();
            foreach(var doc in snap.Documents)
            {
                foreach(var id in ids)
                {
                    if(id.Equals(doc.Id))
                    {
                        users.Add(UserSnapshotToUser(doc));
                    }
                }
            }
            return (PittClubManager.Models.User[]) users.ToArray(typeof(PittClubManager.Models.User));
        }

        public static void CreateClub(String clubName, String description, String managerId)
        {
            FirebaseClient firebase = new FirebaseClient(FIREBASE_URL);
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(WEB_KEY));
            FirebaseAuthLink res = authProvider.SignInWithEmailAndPasswordAsync(EMAIL, PASSWORD).Result;
            FirestoreDb db = FirestoreDb.Create(DB_NAME);
            // todo: check to make sure this club doesn't already exist in created clubs
            var result = db.Collection(COLLECTION_PENDING_CLUBS).AddAsync(new Dictionary<String, String>()
            {
                {"name", clubName},
                {"description", description},
                {"managerId", managerId}
            }).Result;
        }

        public static Club[] GetPendingClubs()
        {
            FirebaseClient firebase = new FirebaseClient(FIREBASE_URL);
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(WEB_KEY));
            FirebaseAuthLink res = authProvider.SignInWithEmailAndPasswordAsync(EMAIL, PASSWORD).Result;
            FirestoreDb db = FirestoreDb.Create(DB_NAME);
            var snap = db.Collection(COLLECTION_PENDING_CLUBS).GetSnapshotAsync().Result;
            ArrayList pendingClubs = new ArrayList();
            foreach (var doc in snap.Documents)
            {
                        pendingClubs.Add(PendingClubSnapshotToClub(doc));                
            }
            return (PittClubManager.Models.Club[])pendingClubs.ToArray(typeof(PittClubManager.Models.Club));
        }

        public static void ApproveClub(String clubId)
        {
            FirebaseClient firebase = new FirebaseClient(FIREBASE_URL);
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(WEB_KEY));
            FirebaseAuthLink res = authProvider.SignInWithEmailAndPasswordAsync(EMAIL, PASSWORD).Result;
            FirestoreDb db = FirestoreDb.Create(DB_NAME);
            DocumentReference clubReference = db.Collection(COLLECTION_PENDING_CLUBS).Document(clubId);
            var snap = clubReference.GetSnapshotAsync().Result;
            if (snap.Exists)
            {
                var result = db.Collection(COLLECTION_CLUBS).AddAsync(new Dictionary<string, object>
                    {
                        {"name", snap.GetValue<string>("name")},
                        {"description", snap.GetValue<string>("description")},
                        {"managerId", snap.GetValue<string>("managerId")},
                        {"filter", "THIS NEEDS TO BE IMPLEMENTED"},
                        {"eventIds", new string[0]},
                        {"memberIds",new string[0]}
                    }).Result;
                // make sure to remove club from pending after approval
                var deleted = clubReference.DeleteAsync().Result;
            }
        }

        public static PittClubManager.Models.User GetUser(string id)
        {
            FirebaseClient firebase = new FirebaseClient(FIREBASE_URL);
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(WEB_KEY));
            FirebaseAuthLink res = authProvider.SignInWithEmailAndPasswordAsync(EMAIL, PASSWORD).Result;
            FirestoreDb db = FirestoreDb.Create(DB_NAME);
            DocumentSnapshot snap = db.Collection(COLLECTION_USERS).Document(id).GetSnapshotAsync().Result;
            return (snap.Exists) ? UserSnapshotToUser(snap) : null;
        }

        private static Models.User UserSnapshotToUser(DocumentSnapshot snap)
        {
            Models.User user = new Models.User("", "", "");

            user.SetId(snap.Id);
            user.SetFirstName(snap.GetValue<string>("firstName"));
            user.SetLastName(snap.GetValue<string>("lastName"));

            return user;
        }

        private static Club FirebaseSnapshotToClub(DocumentSnapshot snap)
        {
            Club c = new Club();
            System.Diagnostics.Debug.WriteLine("Firebase snapshot to club!");
            c.SetName(snap.GetValue<string>("name"));
            c.SetId(snap.Id);
            c.SetDescription(snap.GetValue<string>("description"));
            c.SetFilter(snap.GetValue<string>("filter"));
            String managerId = snap.GetValue<string>("managerId");
            //String[] eventIds = snap.GetValue<string[]>("eventIds");
            PittClubManager.Models.User manager = GetUser(managerId);
            c.SetManager(manager);

            string[] memberIds = snap.GetValue<string[]>("memberIds");
            PittClubManager.Models.User[] members = GetUsersFromIds(memberIds);
            c.SetMembers(members);
            return c;
        }

        private static Club PendingClubSnapshotToClub(DocumentSnapshot snap)
        {
            Club c = new Club();
            c.SetName(snap.GetValue<string>("name"));
            c.SetId(snap.Id);
            c.SetDescription(snap.GetValue<string>("description"));
            String managerId = snap.GetValue<string>("managerId");
            PittClubManager.Models.User manager = GetUser(managerId);
            c.SetManager(manager);
            return c;
        }
    }
}
