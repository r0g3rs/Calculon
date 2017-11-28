using Calculon.Models;
using System.Collections.Generic;

namespace Calculon.Storage
{
    public class DataContext
    {
        public DataContext()
        {
            Users = new List<User>();
        }

        private static DataContext _database;

        public List<User> Users { get; set; }

        public static DataContext Database()
        {
            if (_database == null)
            {
                _database = new DataContext();
            }

            return _database;
        }
    }
}