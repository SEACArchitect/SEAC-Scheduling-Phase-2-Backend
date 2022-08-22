using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Win32;
using MySqlConnector;

/// <summary>
/// Summary description for SQLCommon
/// </summary>
public class SQLCommon
{
    public SQLCommon()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static string GetDBScalar(string strConstring, string strQuery)
    {
        string strResult = "";

        // MySqlConnection
        MySqlConnection Con = new MySqlConnection(strConstring);
        MySqlCommand Comm = new MySqlCommand();
        Comm.Connection = Con;
        Comm.CommandType = CommandType.Text;
        Comm.CommandText = strQuery;

        if (Con.State == ConnectionState.Open) Con.Close();

        Con.Open();

        object objResult = Comm.ExecuteScalar();

        if (objResult == null)
            strResult = null;
        else
            strResult = objResult.ToString();

        Con.Close();

        return strResult;
    }

    public static int GetCntDataFromDB(string strConstring, string strQuery)
    {
        int strResult;

        MySqlConnection Con = new MySqlConnection(strConstring);
        MySqlCommand Comm = new MySqlCommand();
        Comm.Connection = Con;
        Comm.CommandType = CommandType.Text;
        Comm.CommandText = strQuery;

        if (Con.State == ConnectionState.Open) Con.Close();

        Con.Open();

        object objResult = Comm.ExecuteScalar();

        if (objResult != null)
            strResult = (int)objResult;
        else
            strResult = 0;

        Con.Close();

        return strResult;
    }

    public static DataSet GetDataSet(string strConstring, string strQuery)
    {
        // MySqlConnection
        MySqlConnection Con = new MySqlConnection(strConstring);
        MySqlCommand Comm = new MySqlCommand(strQuery, Con);
        Comm.CommandType = CommandType.Text;
        MySqlDataAdapter Adapter = new MySqlDataAdapter(Comm);

        DataSet DS = new DataSet();

        try
        {
            Con.Open();
            Adapter.Fill(DS, "tbl");
        }
        catch (MySqlException)
        {
            throw new ApplicationException("Data error.");
        }
        finally
        {
            Con.Close();
        }

        return DS;

    }

    public static DataTable GetDataTable(string strConstring, string strQuery)
    {
        // MySqlConnection
        MySqlConnection Con = new MySqlConnection(strConstring);
        MySqlCommand Comm = new MySqlCommand(strQuery, Con);
        Comm.CommandType = CommandType.Text;
        MySqlDataAdapter Adapter = new MySqlDataAdapter(Comm);

        DataSet DS = new DataSet();

        try
        {
            Con.Open();
            Adapter.Fill(DS, "tbl");
        }
        catch (MySqlException)
        {
            throw new ApplicationException("Data error.");
        }
        finally
        {
            Con.Close();
        }

        return DS.Tables["tbl"];

    }

    public static int ComExecute(string strConstring, string strQuery)
    {
        int nResult = 0;

        // MySqlConnection
        MySqlConnection myConnection = new MySqlConnection(strConstring);
        MySqlCommand myCommand = new MySqlCommand();

        MySqlTransaction myTrans;

        myConnection.Open();
        myCommand.Connection = myConnection;
        myTrans = myConnection.BeginTransaction();
        myCommand.Transaction = myTrans;

        try
        {
            myCommand.CommandText = strQuery;
            nResult = myCommand.ExecuteNonQuery();
            myTrans.Commit();
        }
        catch (Exception)
        {
            myTrans.Rollback();
        }
        finally
        {
            myConnection.Close();
        }

        return nResult;
    }
}
