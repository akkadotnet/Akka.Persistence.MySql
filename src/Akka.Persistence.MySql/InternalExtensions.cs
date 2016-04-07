using System;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.MySql
{
    internal static class InternalExtensions
    {
        public static string QuoteTable(this string sqlQuery, string tableName)
        {
            var cb = new MySqlCommandBuilder();
            return string.Format(sqlQuery, cb.QuoteIdentifier(tableName));
        }
    }
}