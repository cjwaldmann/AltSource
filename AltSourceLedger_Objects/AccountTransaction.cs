using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AltSourceLedger_Objects
{
    public enum TransactionType
    {
        Deposit = 1,
        Withdrawal = 2
    }
    public class AccountTransaction
    {
        public int AccountID { get; internal set; }
        

        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionType transactionType { get; internal set; }
        public float Amount { get; internal set; }
        public float NewAccountBalance { get; internal set; }
        public DateTime TransactionDate { get; internal set; }
        public DateTime EnteredBy { get; internal set; }



    }
}
