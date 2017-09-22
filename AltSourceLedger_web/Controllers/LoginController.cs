using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using AltSourceLedger_Objects;

//*********************************
//Author: Waldmann
//Purpos: Login/Account Creation screen
//**********************************
namespace AltSourceLedger_Web.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            ViewBag.Title = "AltSource Ledger - Login";

            //clear any lingering session variables to log out the user
            Session["CurrentAccount"] = null;
            Session["CurrentUser"] = null;
            return View();
        }


        [HttpPost]
        public ActionResult RegisterUser(string userInfo)
        {
            //convert the arguement into an anonymous type for processing
            var userDef = new { userName = "", passWord = "" };
            var user = JsonConvert.DeserializeAnonymousType(userInfo, userDef);

            string result;

            List<AltSourceUser> users = (List<AltSourceUser>)AltSourceCache.Cache["users"];
            //check to see if user already exists
            if (users.FirstOrDefault<AltSourceUser>(u => u.username.Equals(user.userName, StringComparison.InvariantCultureIgnoreCase)) == null)
            {
                //user doesn't exist create and return success message
                AltSourceUser newuser = AltSourceUser.CreateUser(user.userName, user.passWord);
                ((List<AltSourceUser>)AltSourceCache.Cache["users"]).Add(newuser);

                //create a new bank account for the new user
                BankAccount acct = BankAccount.NewAccount(newuser.username);
                ((List<BankAccount>)AltSourceCache.Cache["accounts"]).Add(acct);

                result = "SUCCESS";
            }
            else
            {
                //let interface know of duplicate account isssue
                result = "DUPLICATE";

            }

            return Content(result, "text/plain");
        }


        [HttpPost]
        public ActionResult LoginUser(string userInfo)
        {
            //convert the arguement into an anonymous type for processing            List<AltSourceUser> users = (List<AltSourceUser>)AltSourceCache.Cache["users"];
            var userDef = new { userName = "", passWord = "" };
            var vUser = JsonConvert.DeserializeAnonymousType(userInfo, userDef);

            string result;

            List<AltSourceUser> users = (List<AltSourceUser>)AltSourceCache.Cache["users"];
            AltSourceUser user = users.FirstOrDefault<AltSourceUser>(u => u.username.Equals(vUser.userName, StringComparison.InvariantCultureIgnoreCase) && u.password.Equals(vUser.passWord));
            //make sure the user exists and that the password is correct
            if (user != null)
            {
                //everything checks out, 
                result = "SUCCESS";
                Session["currentUser"] = user.username;

                //make sure that the user has an account (they should, but good to check
                BankAccount account;
                account = ((List<BankAccount>)AltSourceCache.Cache["accounts"]).FirstOrDefault<BankAccount>(a => a.AccountOwner.Equals(user.username, StringComparison.InvariantCultureIgnoreCase));
                if (account == null)
                {
                    account = BankAccount.NewAccount(user.username);
                    ((List<BankAccount>)AltSourceCache.Cache["accounts"]).Add(account);

                }

                Session["CurrentAccount"] = account; 
            }
            else
            {
                //notify interface of login failure
                result = "FAILURE"; ;
            }

            return Content(result, "text/plain");
        }
    }
}