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
        public const string COLLECTION_EVENTS = "events";
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

        public async static Task<bool> RequestJoinClub(string uid, string cid)
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
            System.Diagnostics.Debug.WriteLine("Beginning membership request write!");
            var db = FirestoreDb.Create(DB_NAME);
            DocumentReference docRef = db.Collection("clubs").Document(cid);
            Club curClub = GetClub(cid).Result;
            
            //recreate member requests with new uid
            Models.User[] memberRequests = curClub.GetMemberRequests();
            String[] newMemberRequests = new String[memberRequests.Length + 1];
            int i = 0;
            for(i = 0; i < memberRequests.Length; i++)
            {
                newMemberRequests[i] = memberRequests[i].GetId();
            }
            newMemberRequests[i] = uid;
            Dictionary<FieldPath, object> updates = new Dictionary<FieldPath, object>
            {
                { new FieldPath("memberIds"), 1},
            };

            Google.Cloud.Firestore.WriteResult writeResult = await docRef.UpdateAsync("memberRequests", newMemberRequests);
            return true;
        }

        public static void RequestLeaveClub(string uid, string cid)
        {

        }

        public async static Task<bool> ApproveJoinClub(string uid, string cid)
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

            // step 1: remove request from membership requests
            System.Diagnostics.Debug.WriteLine("Removing " + uid + " from membership requests!");
            var db = FirestoreDb.Create(DB_NAME);
            DocumentReference docRef = db.Collection("clubs").Document(cid);
            Club curClub = GetClub(cid).Result;

            //recreate member requests with new uid
            Models.User[] memberRequests = curClub.GetMemberRequests();
            foreach (var item in memberRequests)
            {

                System.Diagnostics.Debug.WriteLine("Mem requests: " + item.GetId());
            }
            System.Diagnostics.Debug.WriteLine("Mem requests: " + memberRequests.Length.ToString());
            string[] newMemRequests = new string[memberRequests.Length-1];
            int i = 0;
            int j = 0;
            for (i = 0; i < memberRequests.Length; i++)
            {
                if(memberRequests[i].GetId() != uid)
                {
                    newMemRequests[j] = memberRequests[i].GetId();
                    j++;
                }
            }
            System.Diagnostics.Debug.WriteLine("New requests: " + newMemRequests.Length.ToString());
            foreach (var item in newMemRequests)
            {

                System.Diagnostics.Debug.WriteLine("New mem requests: " + item);
            }
            Dictionary<FieldPath, object> updates = new Dictionary<FieldPath, object>
            {
                { new FieldPath("memberIds"), 1},
            };
            Google.Cloud.Firestore.WriteResult writeResult = await docRef.UpdateAsync("memberRequests", newMemRequests);

            // step 2: add to member list
            System.Diagnostics.Debug.WriteLine("Adding " + uid + " to members requests!");

            //recreate member requests with new uid
            Models.User[] memberIds = curClub.GetMembers();
            String[] newMembers = new String[memberIds.Length + 1];
            for (i = 0; i < memberIds.Length; i++)
            {
                newMembers[i] = memberIds[i].GetId();
            }
            newMembers[i] = uid;
            writeResult = await docRef.UpdateAsync("memberIds", newMembers);

            return true;
        }

        public static void DenyJoinClub(string uid, string cid)
        {

        }

        public static bool UserIsManager(string uid, string cid)
        {
            Club curClub = GetClub(cid).Result;
            return curClub.GetManager().GetId() == uid;
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

        public static bool UserInClub(string uid, string cid)
        {
            Club curClub = GetClub(cid).Result;
            foreach(var mem in curClub.GetMembers())
            {
                if(mem.GetId() == uid)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool UserPendingClubApproval(string uid, string cid)
        {
            Club curClub = GetClub(cid).Result;
            foreach (var mem in curClub.GetMemberRequests())
            {
                if (mem.GetId() == uid)
                {
                    return true;
                }
            }
            return false;
        }

        public static PittClubManager.Models.User[] GetUsersFromIds(string[] ids)
        {
            if (ids == null) return new Models.User[0];
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

        public static void CreateClub(String clubName, String description, String filter, String email, String managerId)
        {
            FirebaseClient firebase = new FirebaseClient(FIREBASE_URL);
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(WEB_KEY));
            FirebaseAuthLink res = authProvider.SignInWithEmailAndPasswordAsync(EMAIL, PASSWORD).Result;
            FirestoreDb db = FirestoreDb.Create(DB_NAME);
            // todo: check to make sure this club doesn't already exist in created clubs
            var result = db.Collection(COLLECTION_PENDING_CLUBS).AddAsync(new Dictionary<String, String>()
            {
                {"name", clubName},
                {"filter", filter},
                {"description", description},
                {"email", email},
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
                        {"filter", snap.GetValue<string>("filter")},
                        {"email", snap.GetValue<string>("email")},
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
            c.SetName(snap.GetValue<string>("name"));
            c.SetId(snap.Id);
            c.SetDescription(snap.GetValue<string>("description"));
            c.SetFilter(snap.GetValue<string>("filter"));
            String managerId = snap.GetValue<string>("managerId");
            String[] memberIds;
            String[] eventIds;
            String[] memberRequestIds;
            snap.TryGetValue<string[]>("memberIds", out memberIds); // put all of these into own try catch
            snap.TryGetValue<string[]>("eventIds", out eventIds);
            snap.TryGetValue<string[]>("memberRequests", out memberRequestIds);

            PittClubManager.Models.User manager = GetUser(managerId);
            c.SetManager(manager);

            PittClubManager.Models.User[] members = GetUsersFromIds(memberIds);
            PittClubManager.Models.User[] memberRequests = GetUsersFromIds(memberRequestIds);

            PittClubManager.Models.Event[] events = GetEventsFromIds(eventIds);

            c.SetMembers(members);
            c.SetMemberRequests(memberRequests);
            c.SetEvents(events);
            return c;
        }

        public static PittClubManager.Models.Event[] GetEventsFromIds(string[] ids)
        {
            if (ids == null) return new Models.Event[0];
            FirebaseClient firebase = new FirebaseClient(FIREBASE_URL);
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(WEB_KEY));
            FirebaseAuthLink res = authProvider.SignInWithEmailAndPasswordAsync(EMAIL, PASSWORD).Result;
            FirestoreDb db = FirestoreDb.Create(DB_NAME);
            QuerySnapshot snap = db.Collection(COLLECTION_EVENTS).GetSnapshotAsync().Result;
            ArrayList events = new ArrayList();
            foreach (var doc in snap.Documents)
            {
                foreach (var id in ids)
                {
                    if (id.Equals(doc.Id))
                    {
                        events.Add(EventSnapshotToEvent(doc));
                    }
                }
            }
            return (PittClubManager.Models.Event[])events.ToArray(typeof(PittClubManager.Models.Event));
        }

        private static Models.Event EventSnapshotToEvent(DocumentSnapshot snap)
        {
            Models.Event _event = new Models.Event("", DateTime.Now, "", "");

            _event.SetId(snap.Id);
            _event.SetName(snap.GetValue<string>("name"));
            _event.SetLocation(snap.GetValue<string>("location"));
            _event.SetStart(snap.GetValue<DateTime>("datetime"));

            return _event;
        }

        private static Club PendingClubSnapshotToClub(DocumentSnapshot snap)
        {
            Club c = new Club();
            c.SetName(snap.GetValue<string>("name"));
            c.SetId(snap.Id);
            c.SetDescription(snap.GetValue<string>("description"));
            c.SetFilter(snap.GetValue<string>("filter"));
            String managerId = snap.GetValue<string>("managerId");
            PittClubManager.Models.User manager = GetUser(managerId);
            c.SetManager(manager);
            return c;
        }
    }
}
