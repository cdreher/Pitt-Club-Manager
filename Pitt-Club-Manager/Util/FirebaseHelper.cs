﻿using System;
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
            c.SetId(snap.GetValue<string>("id"));
            String managerId = snap.GetValue<string>("managerId");
            String[] memberIds = new String[0];
            String[] eventIds = new String[0];
            String[] memberRequestIds = new String[0];
            try
            {
                memberIds = snap.GetValue<string[]>("memberIds"); // put all of these into own try catch
                eventIds = snap.GetValue<string[]>("eventIds");
                memberRequestIds = snap.GetValue<string[]>("memberRequests");
            }
            catch (Exception e)
            {
            }
            PittClubManager.Models.User manager = GetUser(managerId);
            PittClubManager.Models.User[] members = new PittClubManager.Models.User[memberIds.Length];
            PittClubManager.Models.User[] memberRequests = new PittClubManager.Models.User[memberRequestIds.Length];
            PittClubManager.Models.Event[] events = new PittClubManager.Models.Event[eventIds.Length];
            for (int i = 0; i < memberIds.Length; i++)
            {
                members[i] = GetUser(memberIds[i]);
            }
            for (int i = 0; i < memberRequestIds.Length; i++)
            {
                memberRequests[i] = GetUser(memberRequestIds[i]);
            }
            for (int i = 0; i < eventIds.Length; i++)
            {
                //members[i] = GetEvent(eventIds[i]).Result;
            }
            c.SetManager(manager);
            c.SetMembers(members);
            //c.SetEvents(events);
            c.SetMemberRequests(memberRequests);
            return c;
        }
    }
}
