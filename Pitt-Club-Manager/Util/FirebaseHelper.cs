using System;
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
            FirebaseAuthLink res = await authProvider.SignInWithEmailAndPasswordAsync(EMAIL, PASSWORD);
            FirestoreDb db = FirestoreDb.Create(DB_NAME);
            DocumentSnapshot snap = await db.Collection(COLLECTION_CLUBS).Document(id).GetSnapshotAsync();
            return null;
        }

        public static async Task<Models.User> GetUser(string id)
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
                c.SetName(snap.GetValue<string>("name"));
                snap.GetValue<string>("managerId");
                snap.GetValue<string[]>("memberIds");
                snap.GetValue<string[]>("eventIds");

            }
            catch (Exception exp)
            {
                return null;
            }
            return null;
        }
    }
}
