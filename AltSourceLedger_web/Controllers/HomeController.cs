using AltSourceLedger_Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

//*********************************
//Author: Waldmann
//Purpos: Main account interface page
//**********************************
namespace AltSourceLedger_Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "AltSource Ledger - Account Details";
            //ensure the user is logged in
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Index", "Login");

            }
            return View();
        }

        public ActionResult GetAccount()
        {
            //ensure the user is logged in
            if (Session["CurrentAccount"] == null)
            {
                //throw eror leting the interface know to redirect to login
                return new HttpStatusCodeResult(502, "Your session has expired."); // Unauthorized
            }

            //convert the account listing to a json representation and
            //send tot he interface
            var ji = JsonConvert.SerializeObject((BankAccount) Session["CurrentAccount"]);
            return Content(ji, "application/json");

        }


        [HttpPost]
        public ActionResult ExecuteTransaction(string transaction)
        {
            if (Session["CurrentAccount"] == null)
            {
                //throw eror leting the interface know to redirect to login
                return new HttpStatusCodeResult(502, "Your session has expired."); // Unauthorized
            }

            //translate the passed json arguement into an anonymous type for processing
            var transDev = new {  amount = 0.00,  transactiontype = "" };
            var vTrans = JsonConvert.DeserializeAnonymousType(transaction, transDev);

            switch (vTrans.transactiontype)
            {
                case "d":
                    //deposit
                    ((BankAccount)Session["CurrentAccount"]).MakeDeposit(Convert.ToSingle(vTrans.amount), Session["CurrentUser"].ToString());
                    break;
                case "w":
                    //if the account has sufficient funds, withdraw, otherwise throw an error
                    if (((BankAccount)Session["CurrentAccount"]).CurrentBalance >= vTrans.amount)
                    {
                        ((BankAccount)Session["CurrentAccount"]).MakeWithdrawal(Convert.ToSingle(vTrans.amount), Session["CurrentUser"].ToString());
                    }else
                    {
                        return new HttpStatusCodeResult(501, "This amount exceeds the current available balance."); // Unauthorized

                    }
                    break;
            }

            //convert the  updated account listing to a json representation and
            //send tot he interface
            var ji = JsonConvert.SerializeObject((BankAccount)Session["CurrentAccount"]);

            return Content(ji, "application/json");

        }
    }
}
