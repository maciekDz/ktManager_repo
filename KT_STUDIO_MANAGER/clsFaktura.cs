using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace KT_STUDIO_MANAGER
{


    class clsFaktura:ClassError
    {
        public string DodajFaktureSql
        {
            get { return "Insert Into dbo.FakturySprzedaz ([Id],[NrFaktury],[DataSprzedazy],[Dzien],[Miesiac],[Rok],[KlientID],[Slownie]," + 
                            "[FormaPlatnosci],[Termin],[Netto8],[Netto23],[Brutto8],[Brutto23]) " + "" +
                                "Values (" + Id +",'" + NrFaktury + "','" + FormatedDataSprzedazy + "'," + Dzien + "," + Miesiac + "," + Rok + "," + Klient.Id + ",N'" + 
                                        Slownie + "',N'" + Platnosc + "',N'" + Termin + "'," + FormatedNetto8 + "," + FormatedNetto23 + "," + FormatedBrutto8 + "," + FormatedBrutto23 + ") "; }
        }
        public string KasujFaktureSql
        {
            get
            {
                return "Delete From dbo.FakturySprzedaz Where [ID]=" + Id;
            }
        }
        public string KasujPozycjeSql
        {
            get
            {
                return "Delete From dbo.PozycjeFaktury Where [Faktura_Id]=" + Id + "";
            }
        }
        public string ModyfikujFaktureSql
        {
            get
            {
                return "Update dbo.FakturySprzedaz Set [NrFaktury]='" + NrFaktury + "'," +
                                                        "[DataSprzedazy]='" + FormatedDataSprzedazy + "'," +
                                                        "[Dzien]=" + Dzien + "," +
                                                        "[Miesiac]=" + Miesiac + "," +
                                                        "[Rok]=" + Rok + "," +
                                                        "[KlientID]=" + Klient.Id + "," +
                                                        "[Slownie]=N'" + Slownie + "'," +
                                                        "[FormaPlatnosci]=N'" + Platnosc + "'," +
                                                        "[Termin]=N'" + Termin + "'," +
                                                        "[Netto8]=" + FormatedNetto8 + "," +
                                                        "[Netto23]=" + FormatedNetto23 + "," +
                                                        "[Brutto8]=" + FormatedBrutto8 + "," +
                                                        "[Brutto23]=" + FormatedBrutto23 + " " +
                                                        "Where [Id]=" + Id;
            }
        }


        public int Id { get; set; } = 0;
        public string NrFaktury { get; set; } = "";
        public DateTime DataSprzedazy { get; set; } = DateTime.Parse("9999-12-31");
        public string FormatedDataSprzedazy
        {
            get
            {
                return DataSprzedazy.ToString("yyyy-MM-dd");
            }
        }
        public clsKlient Klient { get; set; }
        public int Dzien {
            get { return DataSprzedazy.Day; }
        }
        public int Miesiac
        {
            get { return DataSprzedazy.Month; }
        }
        public int Rok
        {
            get { return DataSprzedazy.Year; }
        }
        private decimal netto8=0 ;
        public decimal Netto8 { get { return Math.Round(netto8, 2); } set { netto8 = value; } }
        public string FormatedNetto8
        {
            get { return Netto8.ToString().Replace(",", "."); }
        }
        public decimal Brutto8 {
            get { return Math.Round(Netto8 * decimal.Parse((1.08).ToString()), 2); }
        }
        public string FormatedBrutto8
        {
            get { return Brutto8.ToString().Replace(",", "."); }
        }
        public decimal Vat8
        {
            get { return Math.Round(Netto8 * decimal.Parse((0.08).ToString()), 2); }
        }
        public string FormatedVat8
        {
            get { return Vat8.ToString().Replace(",", "."); }
        }
        public decimal Netto23 { get; set; } = 0;
        public string FormatedNetto23
        {
            get { return Netto23.ToString().Replace(",", "."); }
        }
        public decimal Brutto23
        {
            get { return Math.Round(Netto23 * decimal.Parse((1.23).ToString()), 2); }
        }
        public string FormatedBrutto23
        {
            get { return Brutto23.ToString().Replace(",", "."); }
        }
        public decimal Vat23
        {
            get { return Math.Round(Netto23 * decimal.Parse((0.23).ToString()), 2); }
        }
        public decimal RazemNetto
        {
            get { return Netto23 + Netto8; }
        }
        public decimal RazemVat
        {
            get { return Vat23 + Vat8; }
        }
        public decimal RazemBrutto
        {
            get { return Brutto23 + Brutto8; }
        }
        public string Slownie { get; set; } = "";
        public string Platnosc { get; set; } = "";
        public string Termin { get; set; } = "";

        public List<clsPozycja> Pozycje = new List<clsPozycja> { };
        public void DodajPozycje(clsPozycja pozycja)
        {
            Pozycje.Add(pozycja);
        }

        public bool ValidModel()
        {
            try
            {
                bool a = ValidNr();
                bool c = ValidKlient();
                bool b = ValidSlownie();
                bool d = ValidPlatnosc();
                bool e = ValidTermin();
                bool f = ValidPozycje();

                if (a & b & c & d & e & f) { return true; } else { return false; }
            }
            catch { return false; }

        }
        public bool ValidPozycje()
        {
            try
            {
                if (Pozycje.Count>0) { return true; } else { throw new Exception("Brak Pozycji"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidTermin()
        {
            try
            {
                if (Termin != "") { return true; } else { throw new Exception("Zły Termin"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidPlatnosc()
        {
            try
            {
                if (Platnosc != "") { return true; } else { throw new Exception("Zły Sposób Płatności"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidSlownie()
        {
            try
            {
                if (Slownie != "") { return true; } else { throw new Exception("Złe Słownie"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }
        public bool ValidKlient()
        {
            const string ZLY_KLIENT = "Zły Klient";

            try
            {
                if (Klient.Id > 0) { return true; } else { throw new Exception(); }
            }
            catch 
            {
                BuildErrMsg(ZLY_KLIENT);
                return false;
            }
        }
        public bool ValidNr()
        {
            try
            {
                if (NrFaktury != "") { return true; } else { throw new Exception("Zły Numer Faktury"); }
            }
            catch (Exception ex)
            {
                BuildErrMsg(ex.Message.ToString());
                return false;
            }
        }



    }
}
