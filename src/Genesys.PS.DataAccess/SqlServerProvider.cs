using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.PS.DataAccess
{
    public class SqlServerProvider
    {
        public static string ConnectionString { get; set; }

        public static DataTable RunCommand(string script, CommandType commandType, Dictionary<string, object> parameters = null)
        {
            try
            {
                var ret = new DataTable();

                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    using (var command = new SqlCommand(script, conn))
                    {
                        command.CommandType = commandType;
                        if (parameters != null)
                        {
                            foreach (var p in parameters)
                            {
                                var sqlParameter = new SqlParameter(p.Key, p.Value != null ? p.Value : (object)DBNull.Value);
                                command.Parameters.Add(sqlParameter);
                            }
                        }
                        command.CommandTimeout = 60;
                        var dataAdapter = new SqlDataAdapter(command);
                        dataAdapter.Fill(ret);
                        dataAdapter.Dispose();
                    }

                    conn.Close();
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
