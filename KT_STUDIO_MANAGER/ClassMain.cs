using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Net;
using System.Windows.Forms;
using System.Net.Mail;
using System.Threading;

namespace KT_STUDIO_MANAGER
{
    class ClassMain
    {
        
    }

    public class polaczenie_z_baza
    {
        protected string string_polaczeniowy = "Data Source=servername.database.windows.net;Initial Catalog=resource_name;User ID=user;Password=password!";
        public SqlConnection polaczenie;
        public bool polacz()
        {
            try
            {

                polaczenie = new SqlConnection(string_polaczeniowy);
                polaczenie.Open();
                return true;
            }
            catch
            {
                MessageBox.Show("Błąd 02");
                return false;
            }
        }
       
        public bool rozlacz()
        {
            try
            {
                polaczenie.Dispose();
               //polaczenie.Close();
               return true;
            }
            catch
            {
               
                return false;
            }
        }

        public bool query(string query)
        {
            SqlCommand xquery = new SqlCommand(query, polaczenie);
            try
            {
                xquery.ExecuteNonQuery();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public DataSet select(string query)
        {
            SqlCommand xquery = new SqlCommand(query, polaczenie);
           
            SqlDataAdapter xdata = new SqlDataAdapter(xquery);
            
            DataSet res = new DataSet();

            try
            {
                xdata.Fill(res);
            }
            catch{return null;}

            return res;
        }

    }
}
