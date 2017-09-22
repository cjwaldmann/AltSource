using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;

using AltSourceLedger_Objects;
using System.Threading;
using System.Security;
using System.Runtime.InteropServices;

//*********************************
//Author: Waldmann
//Purpos: Sample single account ledger system
//**********************************

namespace AltSourceLedger_Console
{
    class Program
    {
        static float amount = 0;
        static bool success = false;
        static string response;
        static bool loggedin = false;
        static string currentUser;
        static BankAccount account;

        static void Main(string[] args)
        {
            //initialize the cache
            //the cache will allow the user to create multiple accounts and interact
            //with those different accounts within the same program session
            //so, if a user creates user TEST, they could sign out and sign back in with
            //that account without haveing to recreate it or losing the transaction history from the program session
            AltSourceCache.InitializeCache();

            string command;
            bool quitNow = false;
            while (!quitNow)
            {
                success = false;
                if (!loggedin)
                {
                    //if the user is not currently logged in, they can only log in or create a new user
                    Console.WriteLine(@"Please enter a command:
R = Register a new account
L = Login
Q = Quit");
                }
                else
                {
                    //user is logged in, allow them to interact with their account
                    Console.WriteLine(@"Please enter a command:

D = Deposit
W = Withdrawal
T = Transaction History
B = Account Balance
O = Logout
Q = Exit program");
                }
                command = Console.ReadLine();
                switch (command.ToUpper())
                {
                    case "R":
                        //register
                        registerUser();
                        break;
                    case "L":
                        //login
                        login();
                        break;
                    case "O":
                        //logoff
                        currentUser = "";
                        loggedin = false;
                        break;
                    case "T":
                        //transactions
                        history();
                        break;

                    case "D":
                        //deposit
                        deposit();                    
                        break;
                    case "W":
                        //withdrawal
                        withdrawal();
                        break;
                    case "B":
                        //current balanace
                        balance();
                        break;
                    case "Q":
                        //Quit
                        quitNow = true;
                        break;

                    default:
                        Console.WriteLine("Unknown Command " + command);
                        break;
                }
            }
        }

        private static void registerUser()
        {
            //get the list of users
            List<AltSourceUser> users = (List < AltSourceUser >) AltSourceCache.Cache["users"];
            string uname;
            string pwd ;

            Console.WriteLine("Enter a unique user name. Enter 'C' to cancel:");

            while (!success)
            {
                response = Console.ReadLine().Trim();

                if (response.ToUpper() == "C")
                {
                    //user cancellation
                    return;
                }

                //check to ensure the user name is unique within the currently cached objects
                if (users.FirstOrDefault<AltSourceUser>(u => u.username.Equals(response, StringComparison.InvariantCultureIgnoreCase)) == null)
                {
                    //user name is unique, create the user and a new bank account
                    success = true;
                    uname = response;
                    pwd = CreatePassword();

                    AltSourceUser user = AltSourceUser.CreateUser(uname, pwd);
                    ((List<AltSourceUser>)AltSourceCache.Cache["users"]).Add(user);

                    BankAccount acct = BankAccount.NewAccount(user.username);
                    ((List<BankAccount>)AltSourceCache.Cache["accounts"]).Add(acct);

                    Console.WriteLine("Thank you for registering. Your new account # is " + acct.AccountID.ToString() + ".");

                }
                else
                {
                    //alert the user to the duplicate entry so that they can try again or Cancel
                    Console.WriteLine("This user name has already been registered. Please enter a new name or enter 'C' to cancel and login.");

                }
            }

        }

        //log into the system
        private static void login()
        {

            List<AltSourceUser> users = (List<AltSourceUser>)AltSourceCache.Cache["users"];
            string uname = "";
            string pwd = "";

            Console.WriteLine("Enter your user name. Enter 'C' to cancel:");

            while (!success)
            {
                response = Console.ReadLine().Trim();

                if (response.ToUpper() == "C")
                {
                    return;
                }

                //ensure that there is a user whose name the entry
                AltSourceUser user = users.FirstOrDefault<AltSourceUser>(u => u.username.Equals(response, StringComparison.InvariantCultureIgnoreCase));
                if (user  != null)
                {
                    success = true;
                    uname = response;
                    while (pwd != user.password)
                    {
                        //get the password
                        Console.WriteLine("Enter your password:");
                        pwd = GetMaskedPasswordEntry();
                        if (pwd != user.password)
                        {
                            //wrong password, try again
                            Console.WriteLine("Password is incorrect.");
                        }
                    }

                    loggedin = true;
                    currentUser = uname;

                    //create a new bank account if one does not exist for the user
                    account = ((List<BankAccount>)AltSourceCache.Cache["accounts"]).FirstOrDefault<BankAccount>(a => a.AccountOwner.Equals(currentUser, StringComparison.InvariantCultureIgnoreCase));
                    if (account == null)
                    {
                        account = BankAccount.NewAccount(user.username);
                        ((List<BankAccount>)AltSourceCache.Cache["accounts"]).Add(account);

                    }
                }
                else
                {
                    Console.WriteLine("We could not find this user name. Please try again or enter 'C' to cancel.");

                }
            }

        }



        //make a deposit to the user's current account
        private static bool deposit()
        {
            Console.WriteLine("Enter the amount in dollars ($) that you would like to deposit to your account and press ENTER. Enter 'C' to cancel.");
            if (account != null)
            {
                success = GetTransactionAmount(out amount);

                account.MakeDeposit(amount, currentUser);

                Console.WriteLine("You deposited " + amount.ToString("C") + " to your account. Your new balance is " + account.CurrentBalance.ToString("C"));
                return true;
            }else
            {
                Console.WriteLine("We could not find your account. Please logout and try again.");
                return false;
            }
            
            

        }

        //make a withdrawal from the user's current account
        private static bool withdrawal()
        {
            Console.WriteLine("Enter the amount in dollars ($) that you would like to withdraw from your account and press ENTER. Enter 'C' to cancel.");
            if (account != null)
            {
                success = GetTransactionAmount(out amount);

                try
                {
                    account.MakeWithdrawal(amount, currentUser);

                    Console.WriteLine("You withdrew " + amount.ToString("C") + " from your account. Your new balance is " + account.CurrentBalance.ToString("C"));

                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return true;
            }
            else
            {
                Console.WriteLine("We could not find your account. Please logout and try again.");
                return false;
            }



        }

        //Gets the amount for a deposit or withdrawal. Value must be a float
        private static bool GetTransactionAmount(out float amount)
        {
            amount = 0;
            while (!success)
            {
                response = Console.ReadLine();
                if (response.ToUpper() == "C")
                {
                    return false;
                }

                success = float.TryParse(response, out amount);

                if (!success)
                {
                    Console.WriteLine("Please enter a decimal value.");
                }

            }
            return true;
        }

        private static void balance()
        {
            Console.WriteLine("Account #:" + account.AccountID);
            Console.WriteLine("Current Balance:" + account.CurrentBalance.ToString("C"));

        }

        private static void history()
        {
            Console.WriteLine("Account #:" + account.AccountID);
            Console.Write("Date/Time\t\tType\t\tAmount\t\tNew Balance");
            Console.WriteLine();
            Console.WriteLine("_______________\t\t____\t\t______\t\t___________");
            foreach (AccountTransaction trans in account.TransactionHistory.OrderByDescending(h => h.TransactionDate))
            {
                Console.Write(trans.TransactionDate.ToString("g") + "\t");
                Console.Write(trans.transactionType == TransactionType.Deposit ? "Deposit\t\t" : "Withdrawal\t");
                Console.Write(trans.Amount.ToString("C") + "\t\t");
                Console.Write(trans.NewAccountBalance.ToString("C"));
                Console.WriteLine();

            }
        }

        //Creates the user's password
        private static string CreatePassword()
        {
            string pwd;
            Console.WriteLine("Enter a password (minimum 4 characters):");
            pwd = GetMaskedPasswordEntry();
            
            if (pwd.Length < 4)
            {
                return CreatePassword();
            }
            Console.WriteLine("Re-enter your password");

            if (GetMaskedPasswordEntry() != pwd)
            {
                Console.WriteLine("Your entries do not match.");
                pwd = CreatePassword();
            }

            return pwd;
        }

        //Gets the user's entered password, masking entry as it goes
        private static string GetMaskedPasswordEntry()
        {
            StringBuilder pwd = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.Remove(pwd.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd.Append(i.KeyChar);
                    Console.Write("*");
                }
            }
            Console.WriteLine();
            return pwd.ToString();
        }

    }
}
