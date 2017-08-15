using System;
using System.Data;
using System.Data.SqlClient;

namespace GRB.DataAccess
{
    public class DBExecute
    {
        private static Int32 _Command_Timeout = 7200;

        public static DataTable ExecuteProcedureReader(string StoreProcedureName, bool ErrorOnNoResult, params SqlParameter[] SqlParameter)
        {
            return ExecuteProcedureReader(StoreProcedureName, 0, ErrorOnNoResult, SqlParameter);
        }

        public static bool ExecuteTextNonQuery(string textQuery)
        {
            SqlConnection DBConnect = DBConnection.GetDBConnection();

            SqlCommand DBCommand = new SqlCommand();
            DBCommand.Connection = DBConnect;
            DBCommand.CommandType = CommandType.Text;
            DBCommand.CommandText = textQuery;
            DBCommand.CommandTimeout = _Command_Timeout;
            try
            {
                DBConnect.Open();
                DBCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception) { throw; }
            finally { DBConnect.Close(); }
        }

        public static DataTable ExecuteProcedureReader(string StoreProcedureName, Int32 ReturnColumnTotal, bool ErrorOnNoResult, params SqlParameter[] SqlParameter)
        {
            string ResultTableName = "TableData";
            SqlConnection DBConnect = DBConnection.GetDBConnection();
            //SqlConnection DBConnect = new SqlConnection();
            //DBConnect.ConnectionString = "Data Source=DESKTOP-LA9PNGG\\SQLEXPRESS;Initial Catalog=Gun_Registration;Persist Security Info=True;User ID=sa;Password=nextwaver;Connect Timeout=0";

            SqlCommand DBCommand = new SqlCommand();
            DBCommand.Connection = DBConnect;
            DBCommand.CommandType = CommandType.StoredProcedure;
            DBCommand.CommandText = StoreProcedureName;
            DBCommand.CommandTimeout = _Command_Timeout;

            foreach (SqlParameter SqlParameterItem in SqlParameter)
            {
                if ((SqlParameterItem != null))
                {
                    DBCommand.Parameters.Add(SqlParameterItem);
                }
            }

            SqlDataReader DBReader = default(SqlDataReader);
            DataTable TableResult = default(DataTable);

            try
            {
                DBConnect.Open();
                DBReader = DBCommand.ExecuteReader();
                //bool bbba = (bool)(DBCommand.ExecuteScalar());
                if (DBReader.HasRows)
                {
                    if (DBReader.FieldCount == ReturnColumnTotal | ReturnColumnTotal == 0)
                    {
                        TableResult = new DataTable();
                        TableResult.Load(DBReader);
                        TableResult.TableName = ResultTableName;

                        return TableResult;
                    }
                    else
                    {
                        //throw new LIB_Cmn.DatabaseException("Store " + StoreProcedureName + " not return " + ReturnColumnTotal + " column");

                        throw new Exception();
                    }
                }
                else
                {
                    if (ErrorOnNoResult)
                    {
                        //throw new LIB_Cmn.DatabaseException("Store " + StoreProcedureName + " not return result");

                        throw new Exception();
                    }
                    else
                    {
                        TableResult = new DataTable();
                        TableResult.Load(DBReader);
                        TableResult.TableName = ResultTableName;

                        return TableResult;
                    }
                }
            }
            catch (Exception exDb)
            {
                throw exDb;
            }
            finally
            {
                DBConnect.Close();
            }

        }
    }
}
