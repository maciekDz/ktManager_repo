using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace KT_STUDIO_MANAGER
{
    class clsPozycja:ClassError
    {

        public decimal vat8 = decimal.Parse((0.08).ToString());
        public decimal vat23 = decimal.Parse((0.23).ToString());

        public string DodajPozycjeSql
        {
            get
            {
                return "Insert Into dbo.PozycjeFaktury ([Faktura_Id],[NrFaktury],[Lp],[Nazwa],[Qty],[Ilosc],[StVat],[CenaNetto],[WartoscNetto],[WartoscVat],[WartoscBrutto]) " + "" +
                          "Values (" + Faktura.Id + ",'" + Faktura.NrFaktury + "'," + Lp + ",N'" + Nazwa + "',N'" + Jm + "'," + Ilosc + "," + FormatedStVat + "," + FormatedCenaNetto + "," + FormatedWartoscNetto + "," + FormatedWartoscVat + "," + FormatedWartoscBrutto + ") ";
            }
        }

        public int Id { get; set; }
        public clsFaktura Faktura { get; set; }
        public string NrFaktury { get; set; }
        public int Lp { get; set; }
        public string Nazwa { get; set; }
        public string Jm { get; set; }
        public decimal Ilosc { get; set; }
        public decimal CenaNetto { get; set; }
        public string FormatedCenaNetto
        {
             get {return CenaNetto.ToString().Replace(",","."); } 
        }
        public decimal WartoscNetto
        {
            get { return Math.Round(CenaNetto * Ilosc, 2); }
        }
        public string FormatedWartoscNetto
        {
            get { return WartoscNetto.ToString().Replace(",", "."); }
        }
        public decimal StVat { get; set; }
        public string FormatedStVat
        {
            get { return StVat.ToString().Replace(",", "."); }
        }
        public decimal Vat { get; set; }
        public decimal WartoscVat
        {
            get { return Math.Round(CenaNetto *Ilosc * StVat, 2); }
        }
        public string FormatedWartoscVat
        {
            get { return WartoscVat.ToString().Replace(",", "."); }
        }
        public decimal CenaBrutto
        {
            get { return Math.Round(CenaNetto * (1+ StVat), 2); }
        }
        public decimal WartoscBrutto
        {
            get { return Math.Round(WartoscNetto + WartoscVat, 2); }
        }
        public string FormatedWartoscBrutto
        {
            get { return WartoscBrutto.ToString().Replace(",", "."); }
        }

        public bool ValidModel()
        {
            try
            {
                bool a = ValidLp();
                bool c = ValidNazwa();
                bool b = ValidIlosc();
                bool d = ValidJm();
                bool e = ValidVat();
                bool f = ValidCenaNetto();

                if (a & b & c & d  & e & f) { return true; } else { return false; }
            }
            catch { return false; }

        }
        public bool ValidLp()
        {
            try
            {
                if (Lp>0) { return true; } else {throw new Exception("Nie wybrano pozycji"); }
            }
            catch(Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidIlosc()
        {
            const string ZLA_ILOSC = "Zła Ilość";

            try
            {
                if (Ilosc > 0){return true; } else { throw new Exception(ZLA_ILOSC); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidNazwa()
        {
            try
            {
                if (Nazwa != "") { return true; } else { throw new Exception("Zły towar"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidJm()
        {
            try
            {
                if (Jm != "") { return true; } else { throw new Exception("Zła jednostka"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidVat()
        {
            try
            {
                if (StVat != 0) { return true; } else { throw new Exception("Zły VAT"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidCenaNetto()
        {
            try
            {
                if (CenaNetto != 0) { return true; } else { throw new Exception("Zła cena netto"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
    }
}
