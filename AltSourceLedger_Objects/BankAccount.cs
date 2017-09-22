using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltSourceLedger_Objects
{
    public class BankAccount
    {
        public Guid AccountID { get;  set; }
        public float CurrentBalance { get;  set; }
        public List<AccountTransaction> TransactionHistory { get; set; }
        public string AccountOwner { get; private set; }

        public static BankAccount NewAccount(string owner)
        {
            BankAccount ba = new BankAccount();
            ba.AccountID = Guid.NewGuid();
            ba.CurrentBalance = 0;
            ba.AccountOwner = owner;
            ba.TransactionHistory = new List<AccountTransaction>();

            return ba;
        }

        public float MakeDeposit(float amount, string madeBy)
        {
            AccountTransaction t = new AccountTransaction();

            t.Amount = amount;
            t.TransactionDate = DateTime.Now;
            t.transactionType = TransactionType.Deposit;
            t.NewAccountBalance = CurrentBalance + amount;
            CurrentBalance = t.NewAccountBalance;

            TransactionHistory.Add(t);

            return CurrentBalance;
        }

        public float MakeWithdrawal(float amount, string madeBy)
        {
            AccountTransaction t = new AccountTransaction();

            if (amount > CurrentBalance)
            {
                throw new Exception("This exceeds the current account balance of " + CurrentBalance.ToString("C") + ". The transaction has been cancelled.");
            }
            t.Amount = amount;
            t.TransactionDate = DateTime.Now;
            t.transactionType = TransactionType.Withdrawal;
            t.NewAccountBalance = CurrentBalance - amount;
            CurrentBalance = t.NewAccountBalance;

            TransactionHistory.Add(t);

            return CurrentBalance;
        }
    }

}
