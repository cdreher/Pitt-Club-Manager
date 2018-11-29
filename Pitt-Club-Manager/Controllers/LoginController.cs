using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using System.Web.Security;
using Firebase.Auth;
using Firebase.Database;
using PittClubManager.Models;
using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace PittClubManager.Controllers
{
    public class LoginController : Controller
    {
        string _email { get; set; }
        string _password { get; set; }
        bool _rememberMe { get; set; }
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
        public async Task<ActionResult> Login(FormCollection formCollection)
        {
            _email = formCollection["login-email"];
            _password = formCollection["login-password"];
            if (formCollection["login-remember"] == null)
                _rememberMe = false;
            else
                _rememberMe = true;

            Console.WriteLine("rememberMe: {0}", _rememberMe);

            firebase = new FirebaseClient("https://pitt-club-manager.firebaseio.com");
            authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCN8Av2-nfNtsRdlWaZiaejPdwQ4QqA38c"));

            await authProvider.SignInWithEmailAndPasswordAsync(_email, _password).ContinueWith(task =>
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

                user.SetId(newUser.User.LocalId);

                int timeout = _rememberMe ? 525600 : 1;  //525600 min = 1 year
                var ticket = new FormsAuthenticationTicket(_email, _rememberMe, timeout);
                string encrypted = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                cookie.Expires = DateTime.Now.AddMinutes(timeout);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);


                //System.Diagnostics.Debug.WriteLine("Logged in as " + newUser.User.LocalId);
                user = Util.FirebaseHelper.GetUser(newUser.User.LocalId);
                //System.Diagnostics.Debug.WriteLine("User is " + user.GetId());
            }).ConfigureAwait(false);

            if (user.GetId().Equals("###"))
            {
                TempData["InvalidLogin"] = true;
                return RedirectToAction("Index", "Login");
            }

            Session["CurUser"] = user;

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<ActionResult> Register(FormCollection formCollection)
        {
            _email = formCollection["register-email"];
            _password = formCollection["register-password"];
            if (formCollection["register-remember"] == null)
                _rememberMe = false;
            else
                _rememberMe = true;

            Console.WriteLine("rememberMe: {0}", _rememberMe);
            var confirm_password = formCollection["confirm-password"];

            if(!(_password.Equals(confirm_password)))
            {
                TempData["InvalidRegistration"] = true;
                return RedirectToAction("Index", "Login");            
            }

            firebase = new FirebaseClient("https://pitt-club-manager.firebaseio.com");
            authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCN8Av2-nfNtsRdlWaZiaejPdwQ4QqA38c"));

            await authProvider.CreateUserWithEmailAndPasswordAsync(_email, _password).ContinueWith(async task =>
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

                int timeout = _rememberMe ? 525600 : 1;  //525600 min = 1 year
                var ticket = new FormsAuthenticationTicket(_email, _rememberMe, timeout);
                string encrypted = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                cookie.Expires = DateTime.Now.AddMinutes(timeout);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);


            }).ConfigureAwait(false);

            if (user.GetId().Equals("###"))
            {
                TempData["InvalidRegistration"] = true;
                return RedirectToAction("Index", "Login");
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }
    }
}
