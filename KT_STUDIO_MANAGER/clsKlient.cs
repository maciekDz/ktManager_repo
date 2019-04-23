using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KT_STUDIO_MANAGER
{
    class clsKlient : ClassError
    {
        public int Id { get; set; }
        public string Nazwa1 { get; set; }
        public string Nazwa2 { get; set; }
        public string Nazwa3 { get; set; }
        public string Ulica { get; set; }
        public string KodPocztowy { get; set; }
        public string Miasto { get; set; }
        public string KodMiasto { get; set; }
        public string Nip { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public string NazwaSkr { get; set; }
        public bool Aktywny { get; set; }
        public int FormatedAktywny
        {
            get { return Convert.ToInt32(Aktywny); }
        }

        public bool ValidModel()
        {
            try
            {
                bool a = ValidNazwa();
                bool n = ValidNazwaSkr();
                bool c = ValidAdres();
                bool b = ValidKodMiasto();
                bool d = ValidNip();

                if (a & n & b & c & d ) { return true; } else { return false; }
            }
            catch { return false; }

        }
        public bool ValidNazwa()
        {
            try
            {
                if ((Nazwa1 + Nazwa2+ Nazwa3)!="") { return true; } else { throw new Exception("Zła Nazwa KLienta"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidNazwaSkr()
        {
            try
            {
                if (NazwaSkr != "") { return true; } else { throw new Exception("Zła NazwaSkr"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidAdres()
        {
            try
            {
                if (Ulica != "") { return true; } else { throw new Exception("Zła ulica"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidKodMiasto()
        {
            try
            {
                if (KodMiasto != "") { return true; } else { throw new Exception("Zły kod/miasto"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidNip()
        {
            try
            {
               string newNip= new String(Nip.Where(Char.IsDigit).ToArray());
               if (newNip.Length==10) { Nip = newNip.Substring(0,3) +"-"+newNip.Substring(3,3) + "-"+newNip.Substring(6,2) + "-" + newNip.Substring(8, 2); return true; } else { throw new Exception("Zły NIP"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }

        public string DodajKlientaSql
        {
            get
            {
                return "Insert Into dbo.Klienci ([Nazwa1],[Nazwa2],[Nazwa3],[NazwaSkr],[Ulica],[KodMiasto],[NIP],[Telefon],[E-mail],[Aktywny]) " + 
                              "Values (N'" + Nazwa1 + "',N'" + Nazwa2 + "',N'" + Nazwa3 + "',N'" + NazwaSkr + "',N'" 
                                            + Ulica + "',N'" + KodMiasto + "','" + Nip + "','" + Telefon + "','" + Email + "'," + FormatedAktywny +")";
            }
        }
        public string KasujKlientaSql
        {
            get
            {
                return "Delete From dbo.Klienci Where [KlientID]=" + Id;
            }
        }
        public string ModyfikujKlientaSql
        {
            get
            {
                return "Update dbo.Klienci Set [Nazwa1]=N'" + Nazwa1 + "'," +
                                                        "[Nazwa2]=N'" + Nazwa2 + "'," +
                                                        "[Nazwa3]=N'" + Nazwa3 + "'," +
                                                        "[Ulica]=N'" + Ulica + "'," +
                                                        "[KodMiasto]=N'" + KodMiasto + "'," +
                                                        "[NIP]='" + Nip + "'," +
                                                        "[Telefon]='" + Telefon + "'," +
                                                        "[E-mail]='" + Email + "'," +
                                                        "[NazwaSkr]=N'" + NazwaSkr+ "'," +
                                                        "[Aktywny]=" + FormatedAktywny + " " +
                                                        "Where [KlientID]=" + Id;
            }
        }
    }
}
