using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using LiczbyNaSlowaNET;
using System.Data.SqlClient;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.pdf.codec.wmf;


namespace KT_STUDIO_MANAGER
{
    public partial class frmMain : Form 

    {
        public const string SQL_FAKTURY_INIT= "select * from dbo.V_Faktury1 ORDER BY [Rok] desc,[Miesiac] desc, [dzien] desc, [NrFaktury] desc ";
        public const string SQL_NAZWA_SKR_INIT = "select * from dbo.Klienci order by nazwaskr";
        public const string SQL_KLIENCI1_INIT = "select * from dbo.V_Klienci1 order by nazwaskr";
        public const string SQL_POZYCJE_INIT = "select * from dbo.V_Pozycje1 where id=0 order by lp asc";
        public const string SQL_TERMIN_INIT = "Select * from dbo.TerminPlatnosci";
        public const string SQL_TOWAR_INIT = "Select * from dbo.Towary order by NazwaTowaru";

        public bool enableEvents = true;
        public bool formLoading = false;

        public frmMain()
        {

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //MessageBox.Show(LiczbyNaSlowaNET.NumberToText.Convert(decimal.Parse("22.3"), LiczbyNaSlowaNET.Currency.PLN,false));
            formLoading = true;
            WypelnijTabeleDanymi();
            Formatuj();
            formLoading = false;
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            string path = System.IO.Path.GetTempPath() + @"\Faktury\";

            if (Directory.Exists(path))
            {
                var dir = new DirectoryInfo(path);

                dir.Delete(true);

            }
        }

        private void dgvPozycje_DataSourceChanged(object sender, EventArgs e)
        {
            this.dgvPozycje.Columns["Id"].Visible = false;
            this.dgvFaktury.AutoResizeColumns();
        }
        private void dgvPozycje_Click(object sender, EventArgs e)
        {
            enableEvents = false;
            CzyscPozycje();
            PokazPozycje();
            enableEvents = true;
        }
        private void dgvFaktury_Click(object sender, EventArgs e)
        {
            if (this.chEdytuj.Checked)

            {
                CzyscPozycje();
                PokazFakture(0);
            }
        }
        private void dgvFaktury_DataSourceChanged(object sender, EventArgs e)
        {
            FormatujTabele("Faktury");
        }
        private void dgvFaktury_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                int offset;
                if (e.KeyCode == Keys.Down)
                {
                    offset = 1;
                }
                else
                {
                    offset = -1;
                }

                PokazFakture(offset);
            }
            catch { }
            
        }

        private void dgvKlienci_Click(object sender, EventArgs e)
        {
            if (this.chKlientEdytuj.Checked)

            {
                CzyscKlienta();
                PokazKlienta(0);
            }
        }
        private void dgvKlienci_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                int offset;
                if (e.KeyCode == Keys.Down)
                {
                    offset = 1;
                }
                else
                {
                    offset = -1;
                }

                PokazKlienta(offset);
            }
            catch { }
        }

        private void btnZamknij_Click_1(object sender, EventArgs e)
        {
            Close();
        }
        private void btnDodajPozycje_Click(object sender, EventArgs e)
        {
            enableEvents = false;
            try
            {
                var cPozycja = new clsPozycja
                {
                    Lp = NextLp(),
                    Ilosc = My.Convert.toDecimal(this.Ilosc.Text) ,
                    Nazwa = this.Towar.Text,
                    Jm = SprawdzJednostke(),
                    StVat = SprawdzVat(),
                    CenaNetto = My.Convert.toDecimal(this.Netto.Text)
                };

                if (cPozycja.ValidModel())
                {

                    DataRow row = dtsPozycje.Tables[0].NewRow();// dgvPozycje.Rows[0].Clone();
                    row[0] = 0;
                    row[1] = "";
                    row[2] = cPozycja.Lp;
                    row[3] = cPozycja.Nazwa;
                    row[4] = cPozycja.Jm;
                    row[5] = cPozycja.Ilosc;
                    row[6] = cPozycja.StVat;
                    row[7] = cPozycja.CenaNetto;
                    row[8] = cPozycja.WartoscNetto;
                    row[9] = cPozycja.WartoscVat;
                    row[10] = cPozycja.WartoscBrutto;

                    dtsPozycje.Tables[0].Rows.Add(row);
                    dtsPozycje.AcceptChanges();
                    dgvPozycje.Refresh();

                    PodsumujPozycje();

                    dgvPozycje.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                }
                else
                {
                    MessageBox.Show(cPozycja.FormatedErrMsg);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }

            enableEvents = true;
        }
        private void btnModyfikujPozycje_Click(object sender, EventArgs e)


        {
            try
            {
                var cPozycja = new clsPozycja();

                cPozycja.Lp = int.Parse(this.Lp.Text);
                cPozycja.Ilosc = My.Convert.toDecimal(this.Ilosc.Text);
                cPozycja.Nazwa = this.Towar.Text;
                cPozycja.Jm = SprawdzJednostke();
                cPozycja.StVat = SprawdzVat();
                cPozycja.CenaNetto = My.Convert.toDecimal(this.Netto.Text);
                

                if (cPozycja.ValidModel())
                {

                    foreach (DataGridViewRow row in dgvPozycje.Rows)
                    {
                        if (row.Cells["Lp"].Value.ToString() == cPozycja.Lp.ToString())
                        {
                            row.Cells["Nazwa"].Value = cPozycja.Nazwa;
                            row.Cells["J.m"].Value = cPozycja.Jm;
                            row.Cells["Ilość"].Value = cPozycja.Ilosc;
                            row.Cells["VAT"].Value = cPozycja.StVat;
                            row.Cells["Cena Netto"].Value = cPozycja.CenaNetto;
                            row.Cells["Wartość Netto"].Value = cPozycja.WartoscNetto;
                            row.Cells["Wartość Vat"].Value = cPozycja.WartoscVat;
                            row.Cells["Wartość Brutto"].Value = cPozycja.WartoscBrutto;

                            dgvPozycje.Refresh();

                            PodsumujPozycje();
                            
                        }
                    }
                    dgvPozycje.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                }
                else
                {
                    MessageBox.Show(cPozycja.FormatedErrMsg);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
        }
        private void btnUsunPozycje_Click(object sender, EventArgs e)
        {
            try
            {

                var cPozycja = new clsPozycja();

                cPozycja.Lp = int.Parse(this.Lp.Text);

                
                foreach (DataGridViewRow row in dgvPozycje.Rows)
                {
                    if (row.Cells["Lp"].Value.ToString() == cPozycja.Lp.ToString())
                    {
                        dgvPozycje.Rows.RemoveAt(row.Index);

                        dgvPozycje.Refresh();

                        PodsumujPozycje();
                        //return;
                    }
                }

                int index=1;
                
                foreach (DataGridViewRow row in dgvPozycje.Rows)
                {
                    row.Cells["Lp"].Value = index;
                    index += 1;
                }
                dgvPozycje.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void btnDodajFakture_Click(object sender, EventArgs e)
        {
            string msg = "";

            try
            {
                var cFaktura = new clsFaktura();
                cFaktura.Id = NextId("ID", "dbo.FakturySprzedaz");
                ZbierzFaktureZforma(cFaktura);


                if (cFaktura.ValidModel())
                {
                    if (WyslanoFakture(cFaktura))
                    {
                        WyczyscFormularz();
                        PolaczFaktury(SQL_FAKTURY_INIT);
                        msg = "Dodano Fakture";
                    }
                    else
                    {
                        msg = "Nie Dodano Faktury";
                    }
                }
                else { msg = cFaktura.FormatedErrMsg; }
            }
            catch { msg = "Coś poszło nie tak. Faktura nie dodana"; }

            MessageBox.Show(msg);
        }
        private void btnModyfikujFakture_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Na pewno zmodyfikować fakture?", "Modyfikacja Istniejącej Faktury", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string msg = "";
                try
                {
                    var cFaktura = new clsFaktura();
                    cFaktura.Id = My.Convert.toInt(this.tId.Text);
                    ZbierzFaktureZforma(cFaktura);


                    if (cFaktura.ValidModel())
                    {
                        if (ZmodyfikowanoFakture(cFaktura))
                        {
                            PdfCreated();
                            WyczyscFormularz();
                            PolaczFaktury(SQL_FAKTURY_INIT);
                            msg = "Zmodyfikowano Fakture";
                        }
                        else
                        {
                            msg = "Nie Zmodyfikowano Faktury";
                        }
                    }
                    else { msg = cFaktura.FormatedErrMsg; }
                }
                catch { msg = "Coś poszło nie tak. Faktura nie zmodyfikowana"; }

                MessageBox.Show(msg);
            }

        }
        private void btnUsunFakture_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Na pewno usunąc fakture?", "Usuwanie Istniejącej Faktury", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string msg = "";
                try
                {
                    var cFaktura = new clsFaktura();
                    cFaktura.Id = My.Convert.toInt(this.tId.Text);
                    if (cFaktura.Id != 0)
                    {
                        if (SkasowanoFakture(cFaktura))
                        {
                            WyczyscFormularz();
                            PolaczFaktury(SQL_FAKTURY_INIT);
                            msg = "Skasowano Fakture";
                        }
                        else
                        {
                            msg = "Nie Skasowano Faktury";
                        }
                    }

                }
                catch { msg = "Coś poszło nie tak. Faktura nie skasowana"; }

                MessageBox.Show(msg);
            }
            
        }
        private void btnPreview_Click(object sender, EventArgs e)
        {
            {
                if (!PdfCreated(true))
                {

                }
            }
        }
        private void btnWyczyscFakture_Click(object sender, EventArgs e)
        {
            WyczyscFormularz();
        }
        private void btnDodajKlienta_Click(object sender, EventArgs e)
        {
            string msg = "";

            try
            {
                var cKlient = new clsKlient();
                ZbierzKlientaZforma(cKlient);

                if (cKlient.ValidModel())
                {
                    if (WyslanoKlienta(cKlient))
                    {
                        PolaczNazwaSkr(SQL_NAZWA_SKR_INIT, NazwaSkr);
                        PolaczNazwaSkr(SQL_NAZWA_SKR_INIT, NazwaSkr1);
                        PolaczNazwaSkrList(SQL_NAZWA_SKR_INIT, dgvKlienci);
                        msg = "Dodano KLienta";
                    }
                    else
                    {
                        msg = "Nie Dodano KLienta";
                    }
                }
                else { msg = cKlient.FormatedErrMsg; }
            }
            catch { msg = "Coś poszło nie tak. Klient nie dodany"; }

            MessageBox.Show(msg);
        }
        private void btnModyfikujKlienta_Click(object sender, EventArgs e)
        {
            string msg = "";

            try
            {
                var cKlient = new clsKlient();
                cKlient.Id = My.Convert.toInt(this.tKlientId.Text);
                ZbierzKlientaZforma(cKlient);


                if (cKlient.ValidModel())
                {
                    if (ZmodyfikowanoKlienta(cKlient))
                    {
                        PolaczNazwaSkr(SQL_NAZWA_SKR_INIT, NazwaSkr);
                        PolaczNazwaSkr(SQL_NAZWA_SKR_INIT, NazwaSkr1);
                        PolaczNazwaSkrList(SQL_NAZWA_SKR_INIT, dgvKlienci);
                        
                        msg = "Zmodyfikowano Klienta";
                    }
                    else
                    {
                        msg = "Nie Zmodyfikowano Klienta";
                    }
                }
                else { msg = cKlient.FormatedErrMsg; }
            }
            catch { msg = "Coś poszło nie tak. KLient nie zmodyfikowany"; }

            MessageBox.Show(msg);
        }
        private void btnUsunKlienta_Click(object sender, EventArgs e)
        {
            string msg = "";
            try
            {
                var cKlient = new clsKlient();
                cKlient.Id = My.Convert.toInt(this.tKlientId.Text);
                if (cKlient.Id != 0)
                {
                    if (SkasowanoKlienta(cKlient))
                    {
                        CzyscKlienta();
                        PolaczNazwaSkr(SQL_NAZWA_SKR_INIT, NazwaSkr);
                        PolaczNazwaSkr(SQL_NAZWA_SKR_INIT, NazwaSkr1);
                        PolaczNazwaSkrList(SQL_NAZWA_SKR_INIT, dgvKlienci);
                        msg = "Skasowano Klienta";
                    }
                    else
                    {
                        msg = "Nie Skasowano Klienta";
                    }
                }

            }
            catch { msg = "Coś poszło nie tak. Klient nie skasowany"; }

            MessageBox.Show(msg);
        }

        private void Netto_TextChanged(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                WyczyscKontrolki(this.Netto);
                enableEvents = true;
            }
            
        }
        private void WartNetto_TextChanged(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                WyczyscKontrolki(this.WartNetto);
                enableEvents = true;
            }
            

        }
        private void Vat_TextChanged(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                WyczyscKontrolki(this.Vat);
                enableEvents = true;
            }
           

        }
        private void WartVat_TextChanged_1(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                WyczyscKontrolki(this.WartVat);
                enableEvents = true;
            }
            

        }
        private void Brutto_TextChanged(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                WyczyscKontrolki(this.Brutto);
                enableEvents = true;
            }
            
        }
        private void WartBrutto_TextChanged(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                WyczyscKontrolki(this.WartBrutto);
                enableEvents = true;
            }
            
        }

        private void Netto_Leave(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                ObliczNaPodstawie("Netto");
                enableEvents = true;
            }

        }
        private void WartNetto_Leave(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                ObliczNaPodstawie("WartNetto");
                enableEvents = true;
            }

        }
        private void Vat_Leave(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                ObliczNaPodstawie("Vat");
                enableEvents = true;
            }
        }
        private void WartVat_Leave(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                ObliczNaPodstawie("WartVat");
                enableEvents = true;
            }

        }
        private void Brutto_Leave(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                ObliczNaPodstawie("Brutto");
                enableEvents = true;
            }

        }
        private void WartBrutto_Leave(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                ObliczNaPodstawie("WartBrutto");
                enableEvents = true;
            }

        }
        private void Ilosc_Leave(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                ObliczNaPodstawie("Ilosc");
                enableEvents = true;
            }

        }

        private void rVat8_Click(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                ObliczNaPodstawie("StVat");
                enableEvents = true;
            }
        }
        private void rVat23_Click(object sender, EventArgs e)
        {
            if (enableEvents)
            {
                enableEvents = false;
                ObliczNaPodstawie("StVat");
                enableEvents = true;
            }
        }

        private void btnFiltruj_Click(object sender, EventArgs e)
        {
            try
            {
                string nSkr = "";
                if (this.NazwaSkr1.SelectedValue.IsNull())
                {
                    nSkr = "";
                }
                else
                {
                    nSkr = this.NazwaSkr1.SelectedValue.ToString();
                }
                decimal nazwaSkr = My.Convert.toDecimal(nSkr);                
                decimal miesiac = My.Convert.toDecimal(this.Miesiac.Text ?? "");
                decimal rok = My.Convert.toDecimal(this.Rok.Text ?? "");
                string contition = "";

                if (nazwaSkr!=0)
                {
                    contition = "where [KlientId]=" + nazwaSkr;
                }

                if (miesiac!=0)
                {
                    if (contition != "")
                    {
                        contition = contition + " and [miesiac]=" + miesiac.ToString();
                    }
                    else
                    {
                        contition = "where [miesiac]=" + miesiac.ToString();
                    }
                }
                if (rok != 0)
                {
                    if (contition != "")
                    {
                        contition = contition + " and [rok]=" + rok.ToString();
                    }
                    else
                    {
                        contition = "where [rok]=" + rok.ToString();
                    }
                }

                string query = "select * from V_Faktury1 " + contition + " ORDER BY [Rok] desc,[Miesiac] desc, [dzien] desc, [NrFaktury] desc ";

                PolaczFaktury(query);

            }
            catch
            {

                
            }

        }
        private void btnWyczyscFiltr_Click(object sender, EventArgs e)
        {
            PolaczFaktury(SQL_FAKTURY_INIT);
            this.NazwaSkr1.Text = "";
            this.Miesiac.Text = "";
            this.Rok.Text = "";
        }

        private void ZbierzFaktureZforma(clsFaktura cFaktura)
        {
            var cPozycja = new clsPozycja();
            try
            {
                cFaktura.NrFaktury = this.tNrFaktury.Text;
                cFaktura.DataSprzedazy = this.tDataSprzedazy.Value;
                var cKlient = new clsKlient();
                cFaktura.Klient = new clsKlient();
                cFaktura.Klient.Id = My.Convert.toInt(this.NazwaSkr.SelectedValue.ToString());
                cFaktura.Slownie = this.tSlownie.Text;
                cFaktura.Netto8 = My.Convert.toDecimal(this.tNetto8.Text);
                cFaktura.Netto23 = My.Convert.toDecimal(this.tNetto23.Text);

                if (rbGotowka.Checked)
                {
                    cFaktura.Platnosc = rbGotowka.Text;
                }
                if (rbPrzelew.Checked)
                {
                    cFaktura.Platnosc = rbPrzelew.Text;
                }
                cFaktura.Termin = this.cTermin.Text;

                try
                {
                    foreach (DataGridViewRow row in dgvPozycje.Rows)
                    {
                        cPozycja = new clsPozycja();

                        cPozycja.Faktura = cFaktura;
                        cPozycja.NrFaktury = cFaktura.NrFaktury;
                        cPozycja.Lp = My.Convert.toInt(row.Cells["Lp"].Value.ToString());
                        cPozycja.Nazwa = row.Cells["Nazwa"].Value.ToString();
                        cPozycja.Jm = row.Cells["J.m"].Value.ToString();
                        cPozycja.Ilosc = My.Convert.toInt(row.Cells["Ilość"].Value.ToString());
                        cPozycja.StVat = My.Convert.toDecimal(row.Cells["VAT"].Value.ToString());
                        cPozycja.CenaNetto = cPozycja.Vat = My.Convert.toDecimal(row.Cells["Cena Netto"].Value.ToString());

                        if (cPozycja.ValidModel())
                        {
                            cFaktura.DodajPozycje(cPozycja);
                        }
                    }

                    if (cFaktura.Pozycje.Count != dgvPozycje.Rows.Count)
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    cFaktura.Pozycje.Clear();
                }
            }
            catch
            { }
        }
        private void ZbierzKlientaZforma(clsKlient cKlient)
        {
            try
            {
                cKlient.Id = My.Convert.toInt(this.tKlientId.Text);
                cKlient.Nazwa1 = this.tNazwa1.Text;
                cKlient.Nazwa2 = this.tNazwa2.Text;
                cKlient.Nazwa3 = this.tNazwa3.Text;
                cKlient.NazwaSkr = this.tNazwaSkr.Text;
                cKlient.Ulica = this.tAdres.Text;
                cKlient.KodMiasto = this.tKodMiasto.Text;
                cKlient.Email = this.tEmail.Text;
                cKlient.Telefon = this.tTelefon.Text;
                cKlient.Nip = this.tNip.Text;
                cKlient.Aktywny = this.chAktywny.Checked;
            }
            catch
            { }
        }
        private void wpiszDaneDoBazy(string Wartosc)
        {
            polaczenie_z_baza polacz = new polaczenie_z_baza();
            polacz.polacz();
            polacz.query(Wartosc);
            polacz.rozlacz();
        }
        private bool WyslanoFakture(clsFaktura cFaktura)
        {
            try
            {
                polaczenie_z_baza polacz = new polaczenie_z_baza();
                polacz.polacz();

                using (SqlConnection conn = polacz.polaczenie)
                {
                    SqlTransaction sqlTran = null;
                    try
                    {
                        sqlTran = conn.BeginTransaction();
                        SqlCommand command = conn.CreateCommand();
                        command.Transaction = sqlTran;

                        command.CommandText = cFaktura.DodajFaktureSql;
                        command.ExecuteNonQuery();

                        foreach (clsPozycja pozycja in cFaktura.Pozycje)
                        {
                            command.CommandText = pozycja.DodajPozycjeSql;
                            command.ExecuteNonQuery();
                        }

                        if (!PdfCreated())
                        {
                            throw new Exception();
                        }

                        sqlTran.Commit();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            sqlTran.Rollback();
                        }
                        catch { }
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool ZmodyfikowanoFakture(clsFaktura cFaktura)
        {
            try
            {
                polaczenie_z_baza polacz = new polaczenie_z_baza();
                polacz.polacz();

                using (SqlConnection conn = polacz.polaczenie)
                {
                    SqlTransaction sqlTran = null;
                    try
                    {
                        sqlTran = conn.BeginTransaction();
                        SqlCommand command = conn.CreateCommand();
                        command.Transaction = sqlTran;

                        command.CommandText = cFaktura.ModyfikujFaktureSql;
                        command.ExecuteNonQuery();

                        command.CommandText = cFaktura.KasujPozycjeSql;
                        command.ExecuteNonQuery();

                        foreach (clsPozycja pozycja in cFaktura.Pozycje)
                        {
                            command.CommandText = pozycja.DodajPozycjeSql;
                            command.ExecuteNonQuery();
                        }

                        sqlTran.Commit();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            sqlTran.Rollback();
                        }
                        catch { }
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool SkasowanoFakture(clsFaktura cFaktura)
        {
            try
            {
                polaczenie_z_baza polacz = new polaczenie_z_baza();
                polacz.polacz();
                using (SqlConnection conn = polacz.polaczenie)
                {
                    SqlTransaction sqlTran = null;
                    try
                    {
                        sqlTran = conn.BeginTransaction();
                        SqlCommand command = conn.CreateCommand();
                        command.Transaction = sqlTran;

                        command.CommandText = cFaktura.KasujFaktureSql;
                        command.ExecuteNonQuery();

                        if (PdfDeleted())
                        {
                            sqlTran.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            sqlTran.Rollback();
                        }
                        catch { }
                        return false;
                    }
                }
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool WyslanoKlienta(clsKlient cKlient)
        {
            try
            {
                polaczenie_z_baza polacz = new polaczenie_z_baza();
                polacz.polacz();

                using (SqlConnection conn = polacz.polaczenie)
                {
                    SqlTransaction sqlTran = null;
                    try
                    {
                        sqlTran = conn.BeginTransaction();
                        SqlCommand command = conn.CreateCommand();
                        command.Transaction = sqlTran;

                        command.CommandText = cKlient.DodajKlientaSql;
                        command.ExecuteNonQuery();

                        sqlTran.Commit();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            sqlTran.Rollback();
                        }
                        catch { }
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool ZmodyfikowanoKlienta(clsKlient cKlient)
        {
            try
            {
                polaczenie_z_baza polacz = new polaczenie_z_baza();
                polacz.polacz();

                using (SqlConnection conn = polacz.polaczenie)
                {
                    SqlTransaction sqlTran = null;
                    try
                    {
                        sqlTran = conn.BeginTransaction();
                        SqlCommand command = conn.CreateCommand();
                        command.Transaction = sqlTran;

                        command.CommandText = cKlient.ModyfikujKlientaSql;
                        command.ExecuteNonQuery();

                        sqlTran.Commit();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            sqlTran.Rollback();
                        }
                        catch { }
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool SkasowanoKlienta(clsKlient cKlient)
        {
            try
            {
                polaczenie_z_baza polacz = new polaczenie_z_baza();
                polacz.polacz();
                using (SqlConnection conn = polacz.polaczenie)
                {
                    SqlTransaction sqlTran = null;
                    try
                    {
                        sqlTran = conn.BeginTransaction();
                        SqlCommand command = conn.CreateCommand();
                        command.Transaction = sqlTran;

                        command.CommandText = cKlient.KasujKlientaSql;
                        command.ExecuteNonQuery();

                        sqlTran.Commit();

                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            sqlTran.Rollback();
                        }
                        catch { }
                        return false;
                    }
                }
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool PdfDeleted()
        {
            try
            {
                DateTime datevalue = DateTime.Parse(tDataSprzedazy.Value.ToString());
                String mn = datevalue.Month.ToString();
                String yy = datevalue.Year.ToString();
                string folders = @"\Faktury\" + @"\" + yy + @"\" + @"\" + mn + @"\";

                string name = @"\F_VAT" + (tNrFaktury.Text).Replace(@"/", @"_") + "_" + NazwaSkr.Text + ".pdf";
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + folders;

                if (File.Exists(path + name))
                {
                    File.Delete(path + name);
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
        public bool PdfCreated(bool isPreview = false)
        {
            try
            {
                Document doc = new Document(PageSize.A4);
                doc.SetMargins(10f, 10f, 30f, 30f);

                BaseFont myFont = BaseFont.CreateFont(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "Calibri.ttf"), BaseFont.CP1257, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font f_8_bold = new iTextSharp.text.Font(myFont, 8, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font f_8_normal = new iTextSharp.text.Font(myFont, 8, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font f_10_bold = new iTextSharp.text.Font(myFont, 10, iTextSharp.text.Font.BOLD);

                DateTime datevalue = DateTime.Parse(tDataSprzedazy.Value.ToString());
                String mn = datevalue.Month.ToString();
                String yy = datevalue.Year.ToString();
                string folders = @"\Faktury\" + @"\" + yy + @"\" + @"\" + mn + @"\";

                string name = @"\F_VAT" + (tNrFaktury.Text).Replace(@"/", @"_") + "_" + NazwaSkr.Text + ".pdf";
                string path = "";
                if (!isPreview)
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + folders;
                }
                else
                {
                    path = System.IO.Path.GetTempPath() + folders;
                    //path = AppDomain.CurrentDomain.BaseDirectory.ToString() + folders;
                }
                

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }


                FileStream os = new FileStream(path + name, FileMode.Create);
                using (os)
                {
                    PdfWriter.GetInstance(doc, os);
                    doc.Open();

                    //Preview
                    {
                        if (isPreview)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase("PODGLĄD WYDRUKU", f_10_bold));
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            PdfPTable t = new PdfPTable(1);
                            t.AddCell(cell);
                            t.HorizontalAlignment = Element.ALIGN_CENTER;
                            t.WidthPercentage = 100;

                            doc.Add(t);
                        }
                    }

                    //Zjd
                    {
                        var projectPath = AppDomain.CurrentDomain.BaseDirectory.ToString();
                        string filePath = Path.Combine(projectPath, "Resources\\images.png");
                        iTextSharp.text.Image myImage = iTextSharp.text.Image.GetInstance(filePath);
                        PdfPCell cell = new PdfPCell(myImage, true);
                        cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        PdfPTable t = new PdfPTable(1);
                        t.AddCell(cell);
                        t.HorizontalAlignment = Element.ALIGN_LEFT;
                        t.WidthPercentage = 10;

                        doc.Add(t);

                    }

                    //Dane Faktury
                    {
                        PdfPTable table1 = new PdfPTable(2);
                        float[] width = new float[] { 40f, 60f };

                        PdfPCell cel0 = new PdfPCell(new Phrase("FAKTURA VAT:", f_8_bold));
                        PdfPCell cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        PdfPCell cel2 = new PdfPCell(new Phrase("Nr:", f_8_bold));
                        PdfPCell cel3 = new PdfPCell(new Phrase(tNrFaktury.Text, f_8_normal));
                        PdfPCell cel4 = new PdfPCell(new Phrase("ORYGINAŁ/KOPIA", f_8_normal));
                        PdfPCell cel5 = new PdfPCell(new Phrase("", f_8_normal));
                        PdfPCell cel6 = new PdfPCell(new Phrase("Data Sprzedaży:", f_8_bold));
                        PdfPCell cel7 = new PdfPCell(new Phrase(tDataSprzedazy.Text, f_8_normal));
                        PdfPCell cel8 = new PdfPCell(new Phrase("Data Wystawienia:", f_8_bold));
                        PdfPCell cel9 = new PdfPCell(new Phrase(tDataSprzedazy.Text, f_8_normal));

                        cel0.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        cel1.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        cel2.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        cel3.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        cel4.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        cel5.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        cel6.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        cel7.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        cel8.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        cel9.Border = iTextSharp.text.Rectangle.NO_BORDER;

                        cel0.HorizontalAlignment = Element.ALIGN_LEFT;
                        cel1.HorizontalAlignment = Element.ALIGN_LEFT;
                        cel2.HorizontalAlignment = Element.ALIGN_LEFT;
                        cel3.HorizontalAlignment = Element.ALIGN_LEFT;
                        cel4.HorizontalAlignment = Element.ALIGN_LEFT;
                        cel5.HorizontalAlignment = Element.ALIGN_LEFT;
                        cel6.HorizontalAlignment = Element.ALIGN_LEFT;
                        cel7.HorizontalAlignment = Element.ALIGN_LEFT;
                        cel8.HorizontalAlignment = Element.ALIGN_LEFT;
                        cel9.HorizontalAlignment = Element.ALIGN_LEFT;

                        table1.WidthPercentage = 40;
                        table1.HorizontalAlignment = Element.ALIGN_RIGHT;
                        table1.AddCell(cel0);
                        table1.AddCell(cel1);
                        table1.AddCell(cel2);
                        table1.AddCell(cel3);
                        table1.AddCell(cel4);
                        table1.AddCell(cel5);
                        table1.AddCell(cel6);
                        table1.AddCell(cel7);
                        table1.AddCell(cel8);
                        table1.AddCell(cel9);
                        table1.SpacingAfter = 0;
                        table1.SpacingBefore = 0;

                        doc.Add(table1);
                    }

                    //Dane Sprzedawcy/klienta
                    {
                        PdfPTable table1 = new PdfPTable(5);
                        float[] width = new float[] { 60f, 0f, 0f, 40f, 0f };

                        DataTable view = (DataTable)NazwaSkr.DataSource;
                        string nazwa1 = view.Rows[NazwaSkr.SelectedIndex]["Nazwa1"].ToString();
                        string nazwa2 = view.Rows[NazwaSkr.SelectedIndex]["Nazwa2"].ToString();
                        string nazwa3 = view.Rows[NazwaSkr.SelectedIndex]["Nazwa3"].ToString();
                        string ulica = view.Rows[NazwaSkr.SelectedIndex]["Ulica"].ToString();
                        string kod = view.Rows[NazwaSkr.SelectedIndex]["KodMiasto"].ToString();
                        string nip = new String(view.Rows[NazwaSkr.SelectedIndex]["NIP"].ToString().Where(Char.IsDigit).ToArray());
                        nip = "NIP: " + nip.Substring(0, 3) + "-" + nip.Substring(3, 3) + "-" + nip.Substring(6, 2) + "-" + nip.Substring(8, 2);

                        
         
                        PdfPCell cel0 = new PdfPCell(new Phrase("Sprzedawca:", f_8_bold));
                        PdfPCell cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        PdfPCell cel2 = new PdfPCell(new Phrase("", f_8_normal));
                        PdfPCell cel3 = new PdfPCell(new Phrase("Odbiorca:", f_8_bold));
                        PdfPCell cel4 = new PdfPCell(new Phrase("", f_8_normal));

                        PdfPCell[] rowToBe = { cel0, cel1, cel2, cel3, cel4 };
                        PdfPRow row1 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            cel.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }

                        cel0 = new PdfPCell(new Phrase("KT Studio Katarzyna Teper – Dziendziel", f_8_normal));
                        cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        cel2 = new PdfPCell(new Phrase("", f_8_normal));
                        cel3 = new PdfPCell(new Phrase(nazwa1, f_8_normal));
                        cel4 = new PdfPCell(new Phrase("", f_8_normal));

                        rowToBe = new PdfPCell[] { cel0, cel1, cel2, cel3, cel4 };
                        PdfPRow row2 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            cel.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }

                        cel0 = new PdfPCell(new Phrase("Ul. Urocza 4/1; 51-361 Wilczyce", f_8_normal));
                        cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        cel2 = new PdfPCell(new Phrase("", f_8_normal));
                        cel3 = new PdfPCell(new Phrase(nazwa2, f_8_normal));
                        cel4 = new PdfPCell(new Phrase("", f_8_normal));

                        rowToBe = new PdfPCell[] { cel0, cel1, cel2, cel3, cel4 };
                        PdfPRow row3 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            cel.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }

                        cel0 = new PdfPCell(new Phrase("NIP: 899-250-29-20", f_8_normal));
                        cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        cel2 = new PdfPCell(new Phrase("", f_8_normal));
                        cel3 = new PdfPCell(new Phrase(nazwa3, f_8_normal));
                        cel4 = new PdfPCell(new Phrase("", f_8_normal));

                        rowToBe = new PdfPCell[] { cel0, cel1, cel2, cel3, cel4 };
                        PdfPRow row4 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            cel.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }

                        cel0 = new PdfPCell(new Phrase("Dane do przelewu:", f_8_bold));
                        cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        cel2 = new PdfPCell(new Phrase("", f_8_normal));
                        cel3 = new PdfPCell(new Phrase(ulica, f_8_normal));
                        cel4 = new PdfPCell(new Phrase("", f_8_normal));

                        rowToBe = new PdfPCell[] { cel0, cel1, cel2, cel3, cel4 };
                        PdfPRow row5 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            cel.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }

                        cel0 = new PdfPCell(new Phrase("KT Kwiatowe Katarzyna Teper – Dziendziel", f_8_normal));
                        cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        cel2 = new PdfPCell(new Phrase("", f_8_normal));
                        cel3 = new PdfPCell(new Phrase(kod, f_8_normal));
                        cel4 = new PdfPCell(new Phrase("", f_8_normal));

                        rowToBe = new PdfPCell[] { cel0, cel1, cel2, cel3, cel4 };
                        PdfPRow row6 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            cel.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }

                        cel0 = new PdfPCell(new Phrase("Ul. Urocza 4/1; 51-361 Wilczyce", f_8_normal));
                        cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        cel2 = new PdfPCell(new Phrase("", f_8_normal));
                        cel3 = new PdfPCell(new Phrase(nip, f_8_normal));
                        cel4 = new PdfPCell(new Phrase("", f_8_normal));

                        rowToBe = new PdfPCell[] { cel0, cel1, cel2, cel3, cel4 };
                        PdfPRow row7 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            cel.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }

                        cel0 = new PdfPCell(new Phrase("Bank:", f_8_bold));
                        cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        cel2 = new PdfPCell(new Phrase("", f_8_normal));
                        cel3 = new PdfPCell(new Phrase("", f_8_normal));
                        cel4 = new PdfPCell(new Phrase("", f_8_normal));

                        rowToBe = new PdfPCell[] { cel0, cel1, cel2, cel3, cel4 };
                        PdfPRow row8 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            cel.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }


                        cel0 = new PdfPCell(new Phrase("ING BANK", f_8_normal));
                        cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        cel2 = new PdfPCell(new Phrase("", f_8_normal));
                        cel3 = new PdfPCell(new Phrase("", f_8_normal));
                        cel4 = new PdfPCell(new Phrase("", f_8_normal));

                        rowToBe = new PdfPCell[] { cel0, cel1, cel2, cel3, cel4 };
                        PdfPRow row9 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            cel.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }

                        cel0 = new PdfPCell(new Phrase("Nr Konta: 63 1050 1575 1000 0090 9042 1414", f_8_normal));
                        cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        cel2 = new PdfPCell(new Phrase("", f_8_normal));
                        cel3 = new PdfPCell(new Phrase("", f_8_normal));
                        cel4 = new PdfPCell(new Phrase("", f_8_normal));

                        rowToBe = new PdfPCell[] { cel0, cel1, cel2, cel3, cel4 };
                        PdfPRow row10 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            cel.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }

                        table1.WidthPercentage = 100f;
                        table1.HorizontalAlignment = Element.ALIGN_LEFT;

                        table1.Rows.Add(row1);
                        table1.Rows.Add(row2);
                        table1.Rows.Add(row3);
                        table1.Rows.Add(row4);
                        table1.Rows.Add(row5);
                        table1.Rows.Add(row6);
                        table1.Rows.Add(row7);
                        table1.Rows.Add(row8);
                        table1.Rows.Add(row9);
                        table1.Rows.Add(row10);

                        table1.SetWidths(width);

                        table1.SpacingAfter = 50;
                        table1.SpacingBefore = 20;

                        doc.Add(table1);
                    }

                    ////Pozycje
                    {
                        PdfPTable table1 = new PdfPTable(9);
                        float[] width = new float[] { 3f, 30f, 3f, 4f, 7f, 12f, 13f, 12f, 14f };

                        PdfPCell cel0 = new PdfPCell(new Phrase("Lp", f_8_bold));
                        PdfPCell cel1 = new PdfPCell(new Phrase("Nazwa", f_8_bold));
                        PdfPCell cel2 = new PdfPCell(new Phrase("J.m", f_8_bold));
                        PdfPCell cel3 = new PdfPCell(new Phrase("Ilość", f_8_bold));
                        PdfPCell cel4 = new PdfPCell(new Phrase("St. VAT", f_8_bold));
                        PdfPCell cel5 = new PdfPCell(new Phrase("Cena Netto", f_8_bold));
                        PdfPCell cel6 = new PdfPCell(new Phrase("Wartość Netto", f_8_bold));
                        PdfPCell cel7 = new PdfPCell(new Phrase("Wartość VAT", f_8_bold));
                        PdfPCell cel8 = new PdfPCell(new Phrase("Wartość Brutto", f_8_bold));


                        PdfPCell[] rowToBe = { cel0, cel1, cel2, cel3, cel4, cel5, cel6, cel7, cel8 };
                        PdfPRow row1 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            cel.BackgroundColor = new iTextSharp.text.BaseColor(220, 220, 220);
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }

                        table1.WidthPercentage = 100f;
                        table1.HorizontalAlignment = Element.ALIGN_LEFT;

                        table1.Rows.Add(row1);
                        table1.SetWidths(width);
                        doc.Add(table1);

                        int totalRows = 0;

                        table1 = new PdfPTable(9);
                        width = new float[] { 3f, 30f, 3f, 4f, 7f, 12f, 13f, 12f, 14f };
                        foreach (DataGridViewRow row in dgvPozycje.Rows)
                        {
                            totalRows += int.Parse((row.Cells["Nazwa"].Value.ToString().Length / 50).ToString()) + 1;

                            cel0 = new PdfPCell(new Phrase(row.Cells["Lp"].Value.ToString(), f_8_normal));
                            cel1 = new PdfPCell(new Phrase(row.Cells["Nazwa"].Value.ToString(), f_8_normal));
                            cel2 = new PdfPCell(new Phrase(row.Cells["J.m"].Value.ToString(), f_8_normal));
                            cel3 = new PdfPCell(new Phrase(row.Cells["Ilość"].Value.ToString(), f_8_normal));
                            cel4 = new PdfPCell(new Phrase(row.Cells["VAT"].Value.ToString(), f_8_normal));
                            cel5 = new PdfPCell(new Phrase(row.Cells["Cena Netto"].Value.ToString(), f_8_normal));
                            cel6 = new PdfPCell(new Phrase(row.Cells["Wartość Netto"].Value.ToString(), f_8_normal));
                            cel7 = new PdfPCell(new Phrase(row.Cells["Wartość VAT"].Value.ToString(), f_8_normal));
                            cel8 = new PdfPCell(new Phrase(row.Cells["Wartość Brutto"].Value.ToString(), f_8_normal));

                            rowToBe = new PdfPCell[] { cel0, cel1, cel2, cel3, cel4, cel5, cel6, cel7, cel8 };
                            row1 = new PdfPRow(rowToBe);

                            foreach (PdfPCell cel in rowToBe)
                            {
                                cel.HorizontalAlignment = Element.ALIGN_LEFT;
                            }
                            table1.Rows.Add(row1);
                        }

                        table1.WidthPercentage = 100f;
                        table1.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.SetWidths(width);
                        table1.SpacingAfter = (20 - totalRows) * 10;
                        doc.Add(table1);
                    }

                    //Podsumowanie
                    {
                        PdfPTable table1 = new PdfPTable(1);
                        PdfPCell cel0 = new PdfPCell(new Phrase("Podsumowanie:", f_8_bold));
                        cel0.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        table1.AddCell(cel0);
                        table1.HorizontalAlignment = Element.ALIGN_LEFT;
                        table1.WidthPercentage = 100f;
                        table1.SpacingAfter = 10;
                        doc.Add(table1);

                        //Razem
                        {
                            table1 = new PdfPTable(4);
                            float[] width = new float[] { 61f, 13f, 12f, 14f };

                            cel0 = new PdfPCell(new Phrase("Razem do zapłaty:", f_8_bold));
                            PdfPCell cel1 = new PdfPCell(new Phrase(tRazemNetto.Text, f_8_bold));
                            PdfPCell cel2 = new PdfPCell(new Phrase(tRazemVat.Text, f_8_bold));
                            PdfPCell cel3 = new PdfPCell(new Phrase(tRazemBrutto.Text, f_8_bold));

                            PdfPCell[] rowToBe = new PdfPCell[] { cel0, cel1, cel2, cel3 };
                            PdfPRow row1 = new PdfPRow(rowToBe);

                            foreach (PdfPCell cel in rowToBe)
                            {
                                cel.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                                cel.BorderWidth = 1.1f;
                                cel.BorderColor = new BaseColor(128, 128, 128);
                                cel.HorizontalAlignment = Element.ALIGN_LEFT;
                            }

                            table1.WidthPercentage = 100f;
                            table1.HorizontalAlignment = Element.ALIGN_LEFT;

                            table1.Rows.Add(row1);
                            table1.SetWidths(width);
                            table1.SpacingAfter = 10;
                            doc.Add(table1);
                        }

                        //Słownie
                        {
                            table1 = new PdfPTable(2);
                            float[] width = new float[] { 33f, 67f };

                            cel0 = new PdfPCell(new Phrase("Słownie:", f_8_bold));
                            PdfPCell cel1 = new PdfPCell(new Phrase(tSlownie.Text, f_8_normal));
                            cel1.BackgroundColor = new iTextSharp.text.BaseColor(220, 220, 220);

                            PdfPCell[] rowToBe = new PdfPCell[] { cel0, cel1 };
                            PdfPRow row1 = new PdfPRow(rowToBe);

                            foreach (PdfPCell cel in rowToBe)
                            {
                                cel.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                                cel.BorderColor = new BaseColor(255, 255, 255);
                                cel.HorizontalAlignment = Element.ALIGN_LEFT;
                            }
                            table1.Rows.Add(row1);
                            //
                            cel0 = new PdfPCell(new Phrase("Forma płatności:", f_8_bold));

                            cel1 = new PdfPCell(new Phrase("Przelew", f_8_normal));
                            if (rbGotowka.Checked)
                            {
                                cel1 = new PdfPCell(new Phrase("Gotówka", f_8_normal));
                            }

                            cel1.BackgroundColor = new iTextSharp.text.BaseColor(220, 220, 220);

                            rowToBe = new PdfPCell[] { cel0, cel1 };
                            row1 = new PdfPRow(rowToBe);

                            foreach (PdfPCell cel in rowToBe)
                            {
                                cel.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                                cel.BorderColor = new BaseColor(255, 255, 255);
                                cel.HorizontalAlignment = Element.ALIGN_LEFT;
                            }
                            table1.Rows.Add(row1);
                            //
                            cel0 = new PdfPCell(new Phrase("Termin:", f_8_bold));
                            cel1 = new PdfPCell(new Phrase(cTermin.Text, f_8_normal));
                            cel1.BackgroundColor = new iTextSharp.text.BaseColor(220, 220, 220);

                            rowToBe = new PdfPCell[] { cel0, cel1 };
                            row1 = new PdfPRow(rowToBe);

                            foreach (PdfPCell cel in rowToBe)
                            {
                                cel.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                                cel.BorderColor = new BaseColor(255, 255, 255);
                                cel.HorizontalAlignment = Element.ALIGN_LEFT;
                            }
                            table1.Rows.Add(row1);

                            table1.WidthPercentage = 100f;
                            table1.HorizontalAlignment = Element.ALIGN_LEFT;
                            table1.SetWidths(width);
                            doc.Add(table1);
                        }
                    }

                    //Podpis
                    {
                        PdfPTable table1 = new PdfPTable(3);
                        float[] width = new float[] { 33f, 34f, 33f };

                        PdfPCell cel0 = new PdfPCell(new Phrase("Podpis osoby upoważnionej do wystawienia faktury:", f_8_bold));
                        PdfPCell cel1 = new PdfPCell(new Phrase("", f_8_normal));
                        PdfPCell cel2 = new PdfPCell(new Phrase("Podpis osoby upoważnionej do odbioru faktury:", f_8_bold));

                        PdfPCell[] rowToBe = new PdfPCell[] { cel0, cel1, cel2 };
                        PdfPRow row1 = new PdfPRow(rowToBe);

                        foreach (PdfPCell cel in rowToBe)
                        {
                            if (cel != cel1)
                            {
                                cel.Border = iTextSharp.text.Rectangle.TOP_BORDER;
                            }
                            else
                            {
                                cel.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            }

                            cel.BorderWidth = 1.1f;
                            cel.BorderColor = new BaseColor(128, 128, 128);
                            cel.HorizontalAlignment = Element.ALIGN_LEFT;
                        }

                        table1.WidthPercentage = 100f;
                        table1.HorizontalAlignment = Element.ALIGN_LEFT;

                        table1.Rows.Add(row1);
                        table1.SetWidths(width);
                        table1.SpacingBefore = 120;
                        doc.Add(table1);

                    }

                    doc.Close();

                    if (isPreview)
                    {
                        System.Diagnostics.Process.Start(path + name);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                return false;
            }
        }

        private void WypelnijTabeleDanymi()
        {
            //Faktury
            PolaczFaktury(SQL_FAKTURY_INIT);
            PolaczNazwaSkr(SQL_NAZWA_SKR_INIT, NazwaSkr);
            PolaczNazwaSkr(SQL_NAZWA_SKR_INIT, NazwaSkr1);
            PolaczPozycje(SQL_POZYCJE_INIT);
            PolaczTermin(SQL_TERMIN_INIT);
            PolaczTowar(SQL_TOWAR_INIT);

            //Klienci
            PolaczNazwaSkrList(SQL_KLIENCI1_INIT, dgvKlienci);
        }
        private void PokazPozycje(int offset = 0)
        {
            try
            {
                int selectedrowindex = dgvPozycje.SelectedCells[0].RowIndex + offset;
                DataGridViewRow selectedRow = dgvPozycje.Rows[selectedrowindex];


                var cPozycja = new clsPozycja
                {
                    Lp = Convert.ToInt32(selectedRow.Cells["Lp"].Value),
                    Nazwa = Convert.ToString(selectedRow.Cells["Nazwa"].Value),

                    Jm = Convert.ToString(selectedRow.Cells["J.m"].Value),
                    Ilosc = My.Convert.toDecimal(selectedRow.Cells["Ilość"].Value.ToString()),
                    StVat=My.Convert.toDecimal(selectedRow.Cells["VAT"].Value.ToString()),
                    Vat = My.Convert.toDecimal(selectedRow.Cells["VAT"].Value.ToString()),
                    CenaNetto = My.Convert.toDecimal(selectedRow.Cells["Cena Netto"].Value.ToString()),
                };


                this.Lp.Text = cPozycja.Lp.ToString();
                this.Towar.Text = cPozycja.Nazwa.ToString();
                this.Ilosc.Text = cPozycja.Ilosc.ToString();

                if (cPozycja.Jm == "szt.") { this.rSzt.Checked = true; };
                if (cPozycja.Jm == "kpl.") { this.rKpl.Checked = true; };
                if (cPozycja.Jm == "godz.") { this.rGodz.Checked = true; };

                if (cPozycja.Vat == decimal.Parse( 0.08.ToString())) { this.rVat8.Checked = true; };
                if (cPozycja.Vat == decimal.Parse(0.23.ToString())) { this.rVat23.Checked = true; };

                this.Netto.Text = Math.Round( cPozycja.CenaNetto,2).ToString();
                this.WartNetto.Text = Math.Round(cPozycja.WartoscNetto,2).ToString();
                this.Vat.Text = Math.Round((cPozycja.WartoscVat / cPozycja.Ilosc),2).ToString();
                this.WartVat.Text = cPozycja.WartoscVat.ToString();
                this.Brutto.Text = Math.Round((cPozycja.WartoscBrutto / cPozycja.Ilosc),2).ToString();
                this.WartBrutto.Text = Math.Round(cPozycja.WartoscBrutto,2).ToString();
            }
            catch { }

        }
        public void PokazFakture(int offset = 0)
        {
            try
            {
                int selectedrowindex = dgvFaktury.SelectedCells[0].RowIndex + offset;
                DataGridViewRow selectedRow = dgvFaktury.Rows[selectedrowindex];

                var cFaktura = new clsFaktura();

                cFaktura.Id = My.Convert.toInt(selectedRow.Cells["Id"].Value.ToString());
                cFaktura.NrFaktury = selectedRow.Cells["NrFaktury"].Value.ToString();
                cFaktura.DataSprzedazy = My.Convert.toDate(selectedRow.Cells["Data Sprzedaży"].Value.ToString());
                var cKlient = new clsKlient();
                cKlient.Id = My.Convert.toInt(selectedRow.Cells["KlientId"].Value.ToString());
                cFaktura.Klient = cKlient;
                cFaktura.Netto8 = My.Convert.toDecimal(selectedRow.Cells["Netto8"].Value.ToString());
                cFaktura.Netto23 = My.Convert.toDecimal(selectedRow.Cells["Netto23"].Value.ToString());
                cFaktura.Slownie = selectedRow.Cells["Slownie"].Value.ToString();
                cFaktura.Termin = selectedRow.Cells["Termin"].Value.ToString();
                cFaktura.Platnosc = selectedRow.Cells["FormaPlatnosci"].Value.ToString();

                this.tId.Text = cFaktura.Id.ToString();
                this.tNrFaktury.Text = cFaktura.NrFaktury.ToString();

                this.tDataSprzedazy.Value = cFaktura.DataSprzedazy;
                this.NazwaSkr.SelectedValue = cFaktura.Klient.Id;
                this.tNetto8.Text = cFaktura.Netto8.ToString();
                this.tVat8.Text = cFaktura.Vat8.ToString();
                this.tBrutto8.Text = cFaktura.Brutto8.ToString();
                this.tNetto23.Text = cFaktura.Netto23.ToString();
                this.tVat23.Text = cFaktura.Vat23.ToString();
                this.tBrutto23.Text = cFaktura.Brutto23.ToString();
                this.tRazemNetto.Text = cFaktura.RazemNetto.ToString();
                this.tRazemVat.Text = cFaktura.RazemVat.ToString();
                this.tRazemBrutto.Text = cFaktura.RazemBrutto.ToString();
                this.tSlownie.Text = cFaktura.Slownie;
                this.cTermin.SelectedValue = cFaktura.Termin;

                if (cFaktura.Platnosc == "Gotówka")
                {
                    this.rbGotowka.Checked = true;
                    this.rbPrzelew.Checked = false;
                }
                else
                {
                    this.rbGotowka.Checked = false;
                    this.rbPrzelew.Checked = true;
                }

                polaczenie_z_baza polacz = new polaczenie_z_baza();
                polacz.polacz();

                dtsPozycje = polacz.select("select * from dbo.V_Pozycje1 where NrFaktury='" + cFaktura.NrFaktury + "' order by Lp");
                this.dgvPozycje.DataSource = dtsPozycje.Tables[0];
                this.dgvPozycje.Refresh();
                polacz.rozlacz();
                this.dgvPozycje.Columns["Id"].Visible = false;
                this.dgvPozycje.Columns["NrFaktury"].Visible = false;
                this.dgvPozycje.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            }
            catch (ArgumentOutOfRangeException) { }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }


        }
        public void PokazKlienta(int offset = 0)
        {
            try
            {
                int selectedrowindex = dgvKlienci.SelectedCells[0].RowIndex + offset;
                DataGridViewRow selectedRow = dgvKlienci.Rows[selectedrowindex];

                var cKlient = new clsKlient();

                cKlient.Id = My.Convert.toInt(selectedRow.Cells["KlientId"].Value.ToString());
                cKlient.Nazwa1 = selectedRow.Cells["Nazwa1"].Value.ToString();
                cKlient.Nazwa2 = selectedRow.Cells["Nazwa2"].Value.ToString();
                cKlient.Nazwa3 = selectedRow.Cells["Nazwa3"].Value.ToString();
                cKlient.NazwaSkr = selectedRow.Cells["NazwaSkr"].Value.ToString();
                cKlient.Ulica = selectedRow.Cells["Ulica"].Value.ToString();
                cKlient.KodMiasto = selectedRow.Cells["KodMiasto"].Value.ToString();
                cKlient.Email = selectedRow.Cells["E-mail"].Value.ToString();
                cKlient.Telefon = selectedRow.Cells["Telefon"].Value.ToString();
                cKlient.Nip = selectedRow.Cells["NIP"].Value.ToString();
                cKlient.Aktywny = bool.Parse(selectedRow.Cells["Aktywny"].Value.ToString());

                this.tKlientId.Text = cKlient.Id.ToString();
                this.tNazwa1.Text = cKlient.Nazwa1.ToString();
                this.tNazwa2.Text = cKlient.Nazwa2.ToString();
                this.tNazwa3.Text = cKlient.Nazwa3.ToString();
                this.tNazwaSkr.Text = cKlient.NazwaSkr.ToString();

                this.tAdres.Text = cKlient.Ulica.ToString();
                this.tKodMiasto.Text = cKlient.KodMiasto.ToString();
                this.tEmail.Text = cKlient.Email.ToString();
                this.tTelefon.Text = cKlient.Telefon.ToString();
                this.tNip.Text = cKlient.Nip.ToString();
                this.chAktywny.Checked = cKlient.Aktywny;
            }
            catch (ArgumentOutOfRangeException) { }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
        }
        public void WyczyscFormularz()
        {
            CzyscFakture();
            CzyscPozycje();
            PolaczPozycje(SQL_POZYCJE_INIT);
        }
        private void CzyscPozycje()
        {
            this.Lp.Text = "";
            this.Towar.Text = "";
            this.Ilosc.Text = "";
            this.rSzt.Checked = false;
            this.rKpl.Checked = false;
            this.rGodz.Checked = false;
            this.rVat8.Checked = false;
            this.rVat23.Checked = false;

            this.Netto.Text = "";
            this.WartNetto.Text = "";
            this.Vat.Text = "";
            this.WartVat.Text = "";
            this.Brutto.Text = "";
            this.WartBrutto.Text = "";

            
        }
        private void CzyscFakture()
        {
            this.tId.Text = "";
            this.tNrFaktury.Text = "";
            this.tDataSprzedazy.Value= DateTime.Today;
            this.NazwaSkr.SelectedValue = -1;
            this.NazwaSkr.Text="";
            this.tKlient.Text = "";

            this.tNetto8.Text = "";
            this.tVat8.Text = "";
            this.tBrutto8.Text = "";

            this.tNetto23.Text = "";
            this.tVat23.Text = "";
            this.tBrutto23.Text = "";

            this.tRazemNetto.Text = "";
            this.tRazemVat.Text = "";
            this.tRazemBrutto.Text = "";

            this.tSlownie.Text = "";

            this.rbGotowka.Checked = false;
            this.rbPrzelew.Checked = false;

            this.cTermin.SelectedValue = -1;
            this.cTermin.Text = "";
        }
        private void CzyscKlienta()
        {
            this.tKlientId.Text = "";
            this.tNazwa1.Text = "";
            this.tNazwa2.Text = "";
            this.tNazwa3.Text = "";
            this.tNazwaSkr.Text = "";

            this.tAdres.Text = "";
            this.tKodMiasto.Text = "";
            this.tEmail.Text = "";
            this.tTelefon.Text = "";
            this.tNip.Text = "";
            this.chAktywny.Checked = true;

            
        }
        public void PolaczFaktury(string sqlStr)
        {
            polaczenie_z_baza polacz = new polaczenie_z_baza();
            //polacz = new polaczenie_z_baza();
            polacz.polacz();
            dtsFaktury = polacz.select(sqlStr);
            dgvFaktury.DataSource = dtsFaktury.Tables[0];
            dgvFaktury.Refresh();
            FormatujTabele("Faktury");
            polacz.rozlacz();
        }
        public void PolaczNazwaSkr(string sqlStr, ComboBox ctrl)
        {
            polaczenie_z_baza polacz = new polaczenie_z_baza();
            polacz.polacz();
            dtsNazwaSkr = polacz.select(sqlStr);
            ctrl.DataSource = dtsNazwaSkr.Tables[0];
            ctrl.DisplayMember = "NazwaSkr";
            ctrl.ValueMember = "KlientId";
            ctrl.SelectedIndex = -1;
            ctrl.Refresh();
            polacz.rozlacz();
        }
        public void PolaczNazwaSkrList(string sqlStr, DataGridView dgv)
        {
            polaczenie_z_baza polacz = new polaczenie_z_baza();
            //polacz = new polaczenie_z_baza();
            polacz.polacz();
            dtsNazwaSkr = polacz.select(sqlStr);
            dgv.DataSource = dtsNazwaSkr.Tables[0];
            dgv.Refresh();
            FormatujTabele("Klienci");
            polacz.rozlacz();
        }
        public void PolaczPozycje(string sqlStr)
        {
            polaczenie_z_baza polacz = new polaczenie_z_baza();
            polacz.polacz();
            dtsPozycje = polacz.select(sqlStr);
            dgvPozycje.DataSource = dtsPozycje.Tables[0];
            this.dgvPozycje.Columns["Id"].Visible = false;
            this.dgvPozycje.Columns["NrFaktury"].Visible = false;
            dgvPozycje.Refresh();
            polacz.rozlacz();
        }
        public void PolaczTermin(string sqlStr)
        {
            polaczenie_z_baza polacz = new polaczenie_z_baza();
            polacz.polacz();
            dtsTermin = polacz.select(sqlStr);
            cTermin.DataSource = dtsTermin.Tables[0];
            cTermin.DisplayMember = "Termin";
            cTermin.ValueMember = "Termin";
            cTermin.SelectedIndex = -1;
            cTermin.Refresh();
            polacz.rozlacz();
        }
        public void PolaczTowar(string sqlStr)
        {
            polaczenie_z_baza polacz = new polaczenie_z_baza();
            polacz.polacz();
            dtsTowary = polacz.select(sqlStr);
            Towar.DataSource = dtsTowary.Tables[0];
            Towar.DisplayMember = "NazwaTowaru";
            Towar.ValueMember = "NazwaTowaru";
            Towar.SelectedIndex = -1;
            Towar.Refresh();
            polacz.rozlacz();
        }

        private void Formatuj()
        {

            System.Drawing.Font listFont = new System.Drawing.Font(dgvPozycje.Font, FontStyle.Regular);

            this.dgvFaktury.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvFaktury.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvFaktury.ColumnHeadersDefaultCellStyle.Font = listFont;
            dgvFaktury.DefaultCellStyle.Font = listFont;

            this.dgvPozycje.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvPozycje.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvPozycje.ColumnHeadersDefaultCellStyle.Font = listFont;
            dgvPozycje.DefaultCellStyle.Font = listFont;

            this.dgvKlienci.AutoResizeColumns();
            //this.dgvKlienci.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            //this.dgvKlienci.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvKlienci.ColumnHeadersDefaultCellStyle.Font = listFont;
            dgvKlienci.DefaultCellStyle.Font = listFont;
            
            tDataSprzedazy.Format = DateTimePickerFormat.Custom;
            tDataSprzedazy.CustomFormat = "ddd dd MMM yyyy";
        }
        public void FormatujTabele(string tabela)
        {
            switch (tabela)
            {
                case "Faktury":

                    this.dgvFaktury.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    this.dgvFaktury.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                    for (int i = 0; i < this.dgvFaktury.Columns.Count; i++)
                    {
                        this.dgvFaktury.Columns[i].Visible = false;
                        this.dgvFaktury.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    }

                    this.dgvFaktury.Columns["NrFaktury"].Visible = true;
                    this.dgvFaktury.Columns["Data Sprzedaży"].Visible = true;
                    this.dgvFaktury.Columns["NazwaSkr"].Visible = true;
                    this.dgvFaktury.Columns["Razem Brutto"].Visible = true;

                    this.dgvFaktury.AutoResizeColumns();

                    break;
                case "Klienci":

                    //this.dgvKlienci.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    this.dgvKlienci.AutoResizeColumns();
                    dgvKlienci.AllowUserToResizeColumns = true;
                    //this.dgvKlienci.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                    for (int i = 0; i < this.dgvKlienci.Columns.Count; i++)
                    {
                        this.dgvKlienci.Columns[i].Visible = true;
                        this.dgvKlienci.Columns[i].SortMode = DataGridViewColumnSortMode.Automatic;
                    }

                    this.dgvKlienci.Columns["KlientId"].Visible = false;
                    this.dgvKlienci.Columns["Nazwa1"].Visible = false;
                    this.dgvKlienci.Columns["Nazwa2"].Visible = false;
                    this.dgvKlienci.Columns["Nazwa3"].Visible = false;
                    this.dgvKlienci.Columns["Aktywny"].Visible = false;
                    this.dgvKlienci.Columns["Telefon"].Visible = false;
                    this.dgvKlienci.Columns["E-mail"].Visible = false;
                    

                    

                    break;
                default:
                    break;
            }
        }
        public void PodsumujPozycje()
        {
            try
            {
                const decimal stVat8 = 0.08M;
                const decimal stVat23 = 0.23M;
                //decimal stVat8 = decimal.Parse(0.08.ToString());
                //decimal stVat23 = decimal.Parse(0.23.ToString());

                decimal netto8 = 0;
                decimal vat8 = 0;
                decimal brutto8 = 0;
                decimal netto23 = 0;
                decimal vat23 = 0;
                decimal brutto23 = 0;
                decimal razemNetto = 0;
                decimal razemVat = 0;
                decimal razemBrutto = 0;

                foreach (DataGridViewRow row in dgvPozycje.Rows)
                {
                    switch (My.Convert.toDecimal( row.Cells["VAT"].Value.ToString()))
                    {
                        case stVat8:
                            netto8+= decimal.Parse(row.Cells["Wartość Netto"].Value.ToString());
                            vat8 += decimal.Parse(row.Cells["Wartość Vat"].Value.ToString());
                            brutto8 += decimal.Parse(row.Cells["Wartość Brutto"].Value.ToString());

                            break;
                        case stVat23:
                            netto23 += decimal.Parse(row.Cells["Wartość Netto"].Value.ToString());
                            vat23 += decimal.Parse(row.Cells["Wartość Vat"].Value.ToString());
                            brutto23 += decimal.Parse(row.Cells["Wartość Brutto"].Value.ToString());

                            break;
                        default:
                            break;
                    }
                }
                razemNetto = netto8 + netto23;
                razemVat = vat8 + vat23;
                razemBrutto = brutto8 + brutto23;

                this.tNetto8.Text = Math.Round( netto8,2).ToString();
                this.tVat8.Text=vat8.ToString();
                this.tBrutto8.Text = brutto8.ToString();

                this.tNetto23.Text = Math.Round(netto23, 2).ToString();
                this.tVat23.Text = vat23.ToString();
                this.tBrutto23.Text = brutto23.ToString();

                this.tRazemNetto.Text = razemNetto.ToString();
                this.tRazemVat.Text = razemVat.ToString();
                this.tRazemBrutto.Text = razemBrutto.ToString();

                this.tSlownie.Text = NumberToText.Convert(decimal.Parse(razemBrutto.ToString()), Currency.PLN,true);
            }
            catch 
            {
                this.tNetto8.Text = "0";
                this.tVat8.Text = "0";
                this.tBrutto8.Text = "0";

                this.tNetto23.Text = "0";
                this.tVat23.Text = "0";
                this.tBrutto23.Text = "0";

                this.tRazemNetto.Text = "0";
                this.tVat23.Text = "0";
                this.tBrutto23.Text = "0";

                this.tSlownie.Text = NumberToText.Convert(decimal.Parse("0"), Currency.PLN, true);
            }
        }
        public decimal sumaBrutto()
        {
            try
            {
                decimal tempSumaBrutto = 0;
                foreach (DataGridViewRow row in dgvPozycje.Rows)
                {
                    tempSumaBrutto += decimal.Parse(row.Cells["Wartość Brutto"].Value.ToString());
                }
                return tempSumaBrutto;
            }
            catch (Exception)
            {
                return 0;
            }

        }
        private int NextId(string fieldName, string tableName)
        {
            DataSet dtsNextId = new DataSet();
            try
            {
                polaczenie_z_baza polacz = new polaczenie_z_baza();
                polacz = new polaczenie_z_baza();
                polacz.polacz();
                dtsNextId = polacz.select("Select max(" + fieldName + ") +1 From " + tableName);
                polacz.rozlacz();

                return int.Parse(dtsNextId.Tables[0].Rows[0][0].ToString());
            }
            catch { return 0; }
        }
        public int NextLp()
        {
            try
            {
                return dgvPozycje.Rows.Cast<DataGridViewRow>().Max(r => Convert.ToInt32(r.Cells["Lp"].Value)) + 1;
            }
            catch { return 1; }
        }
        public double NumericValue(string textToConvert)
        {
            try
            {
                return double.Parse(textToConvert);
            }
            catch
            {
                return 0;
            }

        }
        public string SprawdzJednostke()
        {
            try
            {
                if (this.rSzt.Checked) { return "szt."; };
                if (this.rKpl.Checked) { return "kpl."; };
                if (this.rGodz.Checked) { return "godz."; };

                return "";
            }
            catch { return ""; }
        }
        public decimal SprawdzVat()
        {
            try
            {
                if (this.rVat8.Checked) { return 0.08M; };
                if (this.rVat23.Checked) { return 0.23M; };

                return 0;
            }
            catch { return 0; }
        }
        private void ObliczNaPodstawie(string podstawa)
        {
            try
            {
                string orgText = "";
                int ilosc = My.Convert.toInt(this.Ilosc.Text.ToString());
                decimal vat = decimal.Parse(SprawdzVat().ToString());
                decimal numPodstawa = 0;
                decimal netto = 0;
                var ctrl = new Control();
                switch (podstawa)
                {
                    case "Netto":
                        orgText = this.Netto.Text;
                       
                        numPodstawa =decimal.Parse(orgText);
                        netto = My.Convert.toDecimal(numPodstawa.ToString());

                        break;
                    case "WartNetto":
                        orgText = this.WartNetto.Text;
                        
                        numPodstawa = decimal.Parse(this.WartNetto.Text);
                        netto = My.Convert.toDecimal((numPodstawa / ilosc).ToString());

                        break;
                    case "Vat":
                        orgText = this.Vat.Text;

                        numPodstawa = decimal.Parse(this.Vat.Text);
                        netto = My.Convert.toDecimal((numPodstawa / vat).ToString());

                        break;
                    case "WartVat":
                        orgText = this.WartVat.Text;
                        
                        numPodstawa = decimal.Parse(this.WartVat.Text);
                        netto = My.Convert.toDecimal(((numPodstawa / vat) / ilosc).ToString());

                        break;
                    case "Brutto":
                        orgText = this.Brutto.Text;
                        
                        numPodstawa = decimal.Parse(this.Brutto.Text);
                        netto = My.Convert.toDecimal((numPodstawa / (1 + vat)).ToString());

                        break;
                    case "WartBrutto":
                        orgText = this.WartBrutto.Text;
                        
                        numPodstawa = decimal.Parse(this.WartBrutto.Text);
                        netto = My.Convert.toDecimal(((numPodstawa / (1 + vat)) / ilosc).ToString());

                        break;
                    case "Ilosc":
                        orgText = this.Netto.Text;
                 
                        numPodstawa = decimal.Parse(orgText);
                        netto = My.Convert.toDecimal(numPodstawa.ToString());

                        break;
                    case "StVat":
                        orgText = this.Netto.Text;

                        numPodstawa = decimal.Parse(orgText);
                        netto = My.Convert.toDecimal(numPodstawa.ToString());

                        break;
                    default:
                        netto = 0;
                        break;
                }

                this.Netto.Text = My.Convert.roundToTwo(netto.ToString()).ToString();
                this.WartNetto.Text = My.Convert.roundToTwo((ilosc * netto).ToString()).ToString();

                this.Vat.Text = My.Convert.roundToTwo((netto * vat).ToString()).ToString();
                this.WartVat.Text = My.Convert.roundToTwo(((netto * vat) * ilosc).ToString()).ToString();

                this.Brutto.Text = My.Convert.roundToTwo((netto + (netto * vat)).ToString()).ToString();
                this.WartBrutto.Text = My.Convert.roundToTwo((ilosc * (netto + (netto * vat))).ToString()).ToString();

                
            }
            catch (DivideByZeroException)
            {
                MessageBox.Show("Nie można podzielić przez 0");
            }
            catch (Exception)
            {
            }
        }
        public List<Control> KontrolkiPozycji()
        {
            var kontrolki = new List<Control>();
            kontrolki.Add(this.Netto);
            kontrolki.Add(this.WartNetto);
            kontrolki.Add(this.Vat);
            kontrolki.Add(this.WartVat);
            kontrolki.Add(this.Brutto);
            kontrolki.Add(this.WartBrutto);

            return kontrolki;
        }
        public void WyczyscKontrolki([Optional]Control nieCzysc)
        {


            var kontrolki = KontrolkiPozycji();
            foreach (var kontrolka in kontrolki)
            {
                if (nieCzysc==null)
                {
                    kontrolka.Text = "";
                }
                else
                {
                    if (kontrolka != nieCzysc)
                    {
                        kontrolka.Text = "";
                    }
                }
               
                
            }
        }

        private void tNip_TextChanged(object sender, EventArgs e)
        {
            //string newNip = new String(tNip.Text.Where(Char.IsDigit).ToArray());
            //if (newNip.Length == 10)
            //{
            //    tNip.Text = newNip.Substring(0, 3) + "-" + newNip.Substring(3, 3) + "-" + newNip.Substring(6, 2) + "-" + newNip.Substring(8, 2);
            //}
        }

        private void NazwaSkr_SelectedValueChanged(object sender, EventArgs e)
        {
            int index = NazwaSkr.SelectedIndex;

            if (index>-1 & !formLoading)
            {
                DataTable view = (DataTable)NazwaSkr.DataSource;
                string nazwa1 = view.Rows[NazwaSkr.SelectedIndex]["Nazwa1"].ToString();
                string nazwa2 = view.Rows[NazwaSkr.SelectedIndex]["Nazwa2"].ToString();
                string nazwa3 = view.Rows[NazwaSkr.SelectedIndex]["Nazwa3"].ToString();
                string ulica = view.Rows[NazwaSkr.SelectedIndex]["Ulica"].ToString();
                string kod = view.Rows[NazwaSkr.SelectedIndex]["KodMiasto"].ToString();
                string nip = view.Rows[NazwaSkr.SelectedIndex]["NIP"].ToString();

                tKlient.Text = nazwa1 + " " + nazwa2 + " " + nazwa3 + " " + ulica + " " + kod + " " + nip;
            }
           
        }
    }
    public static class ExtensionMethods
    {
        public static bool IsNull(this System.Object o)
        {
            return (o == null);
        }
    }



}
