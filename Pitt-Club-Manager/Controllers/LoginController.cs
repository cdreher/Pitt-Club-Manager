using System;
using System.Web.Mvc;
using Firebase.Auth;
using Firebase.Database;
using PittClubManager.Models;

namespace PittClubManager.Controllers
{
    public class LoginController : Controller
    {
        string _email { get; set; }
        string _password { get; set; }
        Models.User user = new Models.User("###", "John", "Doe");
        public static FirebaseClient firebase { get; set; }
        public static FirebaseAuthProvider authProvider { get; set; }

        public ActionResult Index()
        {
            if (TempData["InvalidLogin"] == null)
            {
                TempData["InvalidLogin"] = false;
            }
            if (TempData["InvalidRegistration"] == null)
            {
                TempData["InvalidRegistration"] = false;
            }
            return View ();
        }

        [HttpPost]
        public ActionResult Login(FormCollection formCollection)
        {
            _email = formCollection["login-email"];
            _password = formCollection["login-password"];

            firebase = new FirebaseClient("https://pitt-club-manager.firebaseio.com");
            authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCN8Av2-nfNtsRdlWaZiaejPdwQ4QqA38c"));

            authProvider.SignInWithEmailAndPasswordAsync(_email, _password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Console.WriteLine("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Console.WriteLine("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                FirebaseAuth newUser = task.Result;
                Console.WriteLine("SignInWithEmailAndPasswordAsync success: {0}, {1}", _email, _password);
                //System.Diagnostics.Debug.WriteLine("Logged in as " + newUser.User.LocalId);
                user = Util.FirebaseHelper.GetUser(newUser.User.LocalId);
                //System.Diagnostics.Debug.WriteLine("User is " + user.GetId());

            }).ConfigureAwait(false);
            System.Threading.Thread.Sleep(3000); // Needed for SignInWithEmailAndPasswordAsync to finish
            if (user.GetId().Equals("###"))
            {
                TempData["InvalidLogin"] = true;
                return RedirectToAction("Index", "Login");
            }

            Session["CurUser"] = user;

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Register(FormCollection formCollection)
        {
            _email = formCollection["register-email"];
            _password = formCollection["register-password"];
            var confirm_password = formCollection["confirm-password"];

            if(!(_password.Equals(confirm_password)))
            {
                TempData["InvalidRegistration"] = true;
                return RedirectToAction("Index", "Login");            
            }

            firebase = new FirebaseClient("https://pitt-club-manager.firebaseio.com");
            authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCN8Av2-nfNtsRdlWaZiaejPdwQ4QqA38c"));

            authProvider.CreateUserWithEmailAndPasswordAsync(_email, _password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Console.WriteLine("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Console.WriteLine("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                FirebaseAuth newUser = task.Result;
                Console.WriteLine("CreateUserWithEmailAndPasswordAsync success: {0}, {1}", _email, _password);

                user.SetId(newUser.User.LocalId);


            });
            System.Threading.Thread.Sleep(1500); // Needed for CreateUserWithEmailAndPasswordAsync to finish

            if (user.GetId().Equals("###"))
            {
                TempData["InvalidRegistration"] = true;
                return RedirectToAction("Index", "Login");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
