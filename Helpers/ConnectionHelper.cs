using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Helpers
{
    public class ConnectionHelper : IDisposable
    {
        public MySqlConnection connection { get; }

        public readonly IConfiguration config;
        public ConnectionHelper(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
            
        }

        public void Dispose() => connection.Dispose();


    }
}
