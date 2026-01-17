using Prakt15.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prakt15.Services
{
     public class DBService
    {
        private static readonly Lazy<DBService> _instance = new Lazy<DBService>(() => new DBService());
        public static DBService Instance => _instance.Value;

        public Prak15Context Context { get; private set; }

        private DBService()
        {
            Context = new Prak15Context();
        }

        public void RefreshContext()
        {
            Context.Dispose();
            Context = new Prak15Context();
        }
    }
}
