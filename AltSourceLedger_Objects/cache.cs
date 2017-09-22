using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace AltSourceLedger_Objects
{
    public class AltSourceCache
    {
        public static readonly MemoryCache Cache = new MemoryCache("altsourceledger");

        public static void InitializeCache()
        {
            CacheAccounts("accounts", new List<BankAccount>());
            CacheUsers("users", new List<AltSourceUser>());
        }
        private static void CacheAccounts(string key, object accounts)
        {
            CacheEntryRemovedCallback accountItemRemovedCallback = AccountItemRemovedCallback;
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(60),
                RemovedCallback = accountItemRemovedCallback
            };

            Cache.Set(key, accounts, policy);
        }

        private static void CacheUsers(string key, object users)
        {
            CacheEntryRemovedCallback usersItemRemovedCallback = UsersItemRemovedCallback;
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(60),
                RemovedCallback = usersItemRemovedCallback
            };

            Cache.Set(key, users, policy);
        }


        private static  void AccountItemRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            CacheAccounts(arguments.CacheItem.Key, new List<BankAccount>());
          
        }

        private static void UsersItemRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            CacheUsers(arguments.CacheItem.Key, new List<BankAccount>());

        }

    }
}
