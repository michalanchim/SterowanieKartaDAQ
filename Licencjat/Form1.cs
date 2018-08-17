using Jace;
using NationalInstruments;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Timers;
using System.Diagnostics;

namespace Licencjat
{
    public partial class Form1 : Form
    {
        private Task myTask, myTaskOUT, myTaskOUT2;
        private int nr;
        private int[] indexTablicyCiaglych;
        private CheckBox[] checkBoxy = new CheckBox[16];
        private CheckBox[] checkBoxyZapis = new CheckBox[16];
        private string[] nazwaZapisu = new string[16];
        Label label12;
        double[,] TabWartosci;
        List<double>[] ListaWartosci = new List<double>[16];
        public int ilTablic = 0;
        NumericUpDown wartoscOUT;
        Label labelsinus;
        NumericUpDown amplituda;
        Label labelsinus2;
        NumericUpDown amplituda2;
        Label l;
        NumericUpDown wartoscOUT2;
        private AnalogMultiChannelReader reader;
        private AnalogWaveform<double>[] data;
        public bool zapisywanie = false;
        private RichTextBox wlasny;
        private RichTextBox wlasny2;
        private BackgroundWorker work = null;
        private BackgroundWorker workerOut = null;
        private BackgroundWorker workC;
        private double[] wartosciDoStatystyki;
        private Label[] nrkanLab;
        private Label[] sredniaLab;
        private Label[] odchylenieLab;
        private double[] srednia;
        private double[] odchylenie;
        private Boolean zatrzymajPetle = false;
        private string path;
        Form popup;
        Label popupStaraNazwaLabel;
        Label popupNowaNazwaLabel;
        TextBox popuoNowaNazwa;
        Button popupTak;
        Button popupNie;
        string nazwaZmienianego = "";
        string[] nazwayPoczatkowe = new string[16];
        private Label labelInfo;
        private System.Timers.Timer aTimer;
        private bool moznaLiczyc;
        private int infoZapis = 0;
        List<double> pomocnicza;
        private List<double>[] wartWykresu;
        private List<double>[] wartWykresuOut;
        private int odswiezanieWykresu;
        private int odswiezanieWykresuOut;

        public Form1()
        {
            InitializeComponent();
            path = Path.GetDirectoryName(Application.ExecutablePath);
            progressBar1.Visible = false;
            kanOUT2.Enabled = false;
            maxOutput2Value.Enabled = false;
            minOutput2Value.Enabled = false;
            maxOutput2Value.Visible = false;
            minOutput2Value.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            StworzJedenSkonczony();
            checkBoxy[0] = ai0;
            checkBoxy[1] = ai1;
            checkBoxy[2] = ai2;
            checkBoxy[3] = ai3;
            checkBoxy[4] = ai4;
            checkBoxy[5] = ai5;
            checkBoxy[6] = ai6;
            checkBoxy[7] = ai7;
            checkBoxy[8] = ai8;
            checkBoxy[9] = ai9;
            checkBoxy[10] = ai10;
            checkBoxy[11] = ai11;
            checkBoxy[12] = ai12;
            checkBoxy[13] = ai13;
            checkBoxy[14] = ai14;
            checkBoxy[15] = ai15;
            checkBoxyZapis[0] = checkBox1;
            checkBoxyZapis[1] = checkBox2;
            checkBoxyZapis[2] = checkBox3;
            checkBoxyZapis[3] = checkBox4;
            checkBoxyZapis[4] = checkBox5;
            checkBoxyZapis[5] = checkBox6;
            checkBoxyZapis[6] = checkBox7;
            checkBoxyZapis[7] = checkBox8;
            checkBoxyZapis[8] = checkBox9;
            checkBoxyZapis[9] = checkBox10;
            checkBoxyZapis[10] = checkBox11;
            checkBoxyZapis[11] = checkBox12;
            checkBoxyZapis[12] = checkBox13;
            checkBoxyZapis[13] = checkBox14;
            checkBoxyZapis[14] = checkBox15;
            checkBoxyZapis[15] = checkBox16;
            for (int i = 0; i < checkBoxyZapis.Length; i++)
            {
                checkBoxyZapis[i].Visible = false;
                checkBoxyZapis[i].Enabled = false;
                checkBoxyZapis[i].MouseDown += zmienNazwe;
                nazwayPoczatkowe[i] = checkBoxyZapis[i].Text;
                nazwaZapisu[i] = checkBoxyZapis[i].Text + " - zapisane";
            }
            for (int i = 0; i < checkBoxy.Length; i++)
            {
                checkBoxy[i].Click += odswiezListeDoZapisu;
            }
        }

        private void zmienNazwe(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //MessageBox.Show("Right click na : " + clicked.Text);
                if (popup==null)
                {
                    CheckBox clicked = new CheckBox();
                    clicked = (CheckBox)sender;
                    nazwaZmienianego = clicked.Text;
                    popup = new Form();
                    popup.Text = "Zmiana nazwy pliku";
                    popup.FormClosed += usunpopup;
                    popupStaraNazwaLabel = new Label();
                    popupNowaNazwaLabel = new Label();
                    popuoNowaNazwa = new TextBox();
                    popupTak = new Button();
                    popupNie = new Button();
                    popupStaraNazwaLabel.Location = new Point(5, 5);
                    popupStaraNazwaLabel.Text = "Stara nazwa: " + clicked.Text;
                    popupNowaNazwaLabel.Location = new Point(5, popupStaraNazwaLabel.Location.Y + popupStaraNazwaLabel.Height + 5);
                    popupNowaNazwaLabel.Text = "Nowa nazwa: ";
                    popupNowaNazwaLabel.AutoSize = true;
                    popuoNowaNazwa.Location = new Point(popupNowaNazwaLabel.Location.X + popupNowaNazwaLabel.Width - 20, popupNowaNazwaLabel.Location.Y);
                    popupTak.Location = new Point(5, popuoNowaNazwa.Location.Y + popuoNowaNazwa.Height + 5);
                    popupTak.Text = "Zapisz";
                    popupNie.Location = new Point(popupTak.Location.X + popupTak.Width + 5, popupTak.Location.Y);
                    popupNie.Text = "Anuluj";

                    popupTak.Click += zmienNazweKanalu;

                    popup.Controls.Add(popupStaraNazwaLabel);
                    popup.Controls.Add(popupNowaNazwaLabel);
                    popup.Controls.Add(popuoNowaNazwa);
                    popup.Controls.Add(popupTak);
                    popup.Controls.Add(popupNie);
                    clicked = null;
                    popup.Show();
                }
                else
                {
                    MessageBox.Show("Prosze najpierw zakończyć zmiane nazwy.");
                    popup.Focus();
                }
            }
        }

        private void usunpopup(object sender, FormClosedEventArgs e)
        {
            popup = null;
            popupStaraNazwaLabel = null;
            popupNowaNazwaLabel = null;
            popuoNowaNazwa = null;
            popupTak = null;
            popupNie = null;
        }

        private void zmienNazweKanalu(object sender, EventArgs e)
        {
            for (int i = 0; i < checkBoxyZapis.Length; i++)
            {
                if (nazwaZmienianego == checkBoxyZapis[i].Text)
                {
                    if (popuoNowaNazwa.Text == "")
                    {
                        MessageBox.Show("Nowa nazwa pliku nie może być pusta");
                    }
                    else
                    {
                        nazwaZapisu[i] = popuoNowaNazwa.Text;
                        for (int k = 0; k < nazwaZapisu.Length; k++)
                        {
                            if (i!=k)
                            {
                                if (nazwaZapisu[i]==nazwaZapisu[k])
                                {
                                    MessageBox.Show("Prosze podać unikalną nazwe");
                                    return;
                                }
                            }
                        }
                        if (checkBoxyZapis[i].Text.Length > 3)
                        {
                            if(checkBoxyZapis[i].Text[3] == '(')
                                checkBoxyZapis[i].Text = nazwayPoczatkowe[i] + "(" + popuoNowaNazwa.Text + ")";
                        }
                        else
                        {
                            checkBoxyZapis[i].Text = nazwayPoczatkowe[i] + "(" + popuoNowaNazwa.Text + ")";
                        }
                        if (checkBoxyZapis[i].Text.Length > 4)
                        {
                            if (checkBoxyZapis[i].Text[4] == '(')
                                checkBoxyZapis[i].Text = nazwayPoczatkowe[i] + "(" + popuoNowaNazwa.Text + ")";
                        }
                        else
                        {
                            checkBoxyZapis[i].Text = nazwayPoczatkowe[i] + "(" + popuoNowaNazwa.Text + ")";
                        }
                        popup.Close();
                        break;
                    }
                }
            }
        }

        private void odswiezListeZapisu()
        {
            if (zapisDoPliku.Checked)
            {
                zapisywanie = true;
                DokladnoscZapisuLabel.Visible = true;
                DokladnoscZapisuNumeric.Visible = true;
                mscZapisu.Visible = true;
                for (int i = 0; i < checkBoxy.Length; i++)
                {
                    if (checkBoxy[i].Checked)
                    {
                        checkBoxyZapis[i].Visible = true;
                        checkBoxyZapis[i].Enabled = true;
                    }
                    else
                    {
                        checkBoxyZapis[i].Visible = false;
                        checkBoxyZapis[i].Enabled = false;
                    }
                }
            }
            else
            {
                zapisywanie = false;
                DokladnoscZapisuLabel.Visible = false;
                DokladnoscZapisuNumeric.Visible = false;
                mscZapisu.Visible = false;
                for (int i = 0; i < checkBoxyZapis.Length; i++)
                {
                    checkBoxyZapis[i].Visible = false;
                    checkBoxyZapis[i].Enabled = false;
                    checkBoxyZapis[i].Checked = false;
                }
            }
        }

        private void odswiezListeDoZapisu(object sender, EventArgs e)
        {
            odswiezListeZapisu();
        }

        //Część INPUT
        //Część INPUT
        //Część INPUT
        //Część INPUT
        //Część INPUT

        private void MinValue_ValueChanged(object sender, EventArgs e)
        {
            decimal temp = MinValue.Value;
            if (MinValue.Value>=MaxValue.Value)
            {
                MessageBox.Show("Minimalna wartość musi być mniejsza od maksymalnej wartości.");
                MinValue.Value = MaxValue.Value - 1;
            }
        }

        private void MaxValue_ValueChanged(object sender, EventArgs e)
        {
            decimal temp = MaxValue.Value;
            if (MinValue.Value >= MaxValue.Value)
            {
                MessageBox.Show("Minimalna wartość musi być mniejsza od maksymalnej wartości.");
                MaxValue.Value = MinValue.Value + 1;
            }
        }

        private void start_Click(object sender, EventArgs e)
        {
            zatrzymajPetle = false;
            czyscWykres();
            if (aTimer != null)
            {
                aTimer.Dispose();
            }
            aTimer = new System.Timers.Timer();
            aTimer.Interval = Convert.ToDouble(czasOdswiezania.Value);
           
            aTimer.Enabled = true;
            aTimer.Start();
           
            aTimer.Elapsed += aktualizujStatystyki;

            if (wartWykresu!=null)
            {
                wartWykresu = null;
            }

            moznaLiczyc = false;

            wartosciDoStatystyki = null;
            wartosciDoStatystyki = new double[(int)(ilProb.Value)];
            //zatrzymaj = false;
            ilTablic = 0;
            TabWartosci = null;
            wykres.Series.Clear();
            int licz = 1;
            for (int i = 0; i < checkBoxyZapis.Length; i++)
            {
                if (checkBoxyZapis[i].Checked)
                {
                    licz++;
                }
            }
            nazwaUrz.Enabled = false;
            for (int i = 0; i < checkBoxy.Length; i++)
            {
                checkBoxy[i].Enabled = false;
                checkBoxyZapis[i].Enabled = false;
                if (checkBoxy[i].Checked)
                {
                    ListaWartosci[i] = new List<double>();
                    ilTablic++;

                    wykres.Series.Add(checkBoxy[i].Text);
                    wykres.Series[checkBoxy[i].Text].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                }
            }
            srednia = null;
            odchylenie = null;
            srednia = new double[checkBoxy.Length];
            odchylenie = new double[checkBoxy.Length];
            nrkanLab = null;
            sredniaLab = null;
            odchylenieLab = null;
            nrkanLab = new Label[ilTablic];
            sredniaLab = new Label[ilTablic];
            odchylenieLab = new Label[ilTablic];
            nrkanBox.Controls.Clear();
            sredniaBox.Controls.Clear();
            odchylenieBox.Controls.Clear();
            int indexLabela = 0;
            for (int i = 0; i < checkBoxy.Length; i++)
            {
                srednia[i] = 0;
                odchylenie[i] = 0;
                if (checkBoxy[i].Checked)
                {
                    nrkanLab[indexLabela] = new Label();
                    nrkanLab[indexLabela].Text = checkBoxy[i].Text;
                    nrkanLab[indexLabela].Location = new Point(2, 20 + indexLabela * nrkanLab[indexLabela].Height);
                    nrkanBox.Height = 50 + indexLabela * nrkanLab[indexLabela].Height;
                    nrkanLab[indexLabela].Width = nrkanBox.Width;
                    if (indexLabela%2==0)
                    {
                        nrkanLab[indexLabela].BackColor = Color.LightGray;
                    }
                    nrkanBox.Controls.Add(nrkanLab[indexLabela]);


                    sredniaLab[indexLabela] = new Label();
                    sredniaLab[indexLabela].Location = new Point(2, 20 + indexLabela * sredniaLab[indexLabela].Height);
                    sredniaBox.Height = 50 + indexLabela * sredniaLab[indexLabela].Height;
                    sredniaLab[indexLabela].Width = sredniaBox.Width;
                    if (indexLabela % 2 == 0)
                    {
                        sredniaLab[indexLabela].BackColor = Color.LightGray;
                    }
                    sredniaBox.Controls.Add(sredniaLab[indexLabela]);


                    odchylenieLab[indexLabela] = new Label();
                    odchylenieLab[indexLabela].Location = new Point(2, 20 + indexLabela * odchylenieLab[indexLabela].Height);
                    odchylenieBox.Height = 50 + indexLabela * odchylenieLab[indexLabela].Height;
                    odchylenieLab[indexLabela].Width = odchylenieBox.Width;
                    if (indexLabela % 2 == 0)
                    {
                        odchylenieLab[indexLabela].BackColor = Color.LightGray;
                    }
                    odchylenieBox.Controls.Add(odchylenieLab[indexLabela]);

                    indexLabela++;
                }
            }
            for (int i = 0; i < wykres.Series.Count; i++)
            {
                wykres.Series[i].Points.Clear();
            }
            wykres.ChartAreas[0].AxisX.Minimum = 0;
            wykres.ChartAreas[0].AxisX.Maximum = (double)ilProb.Value;
            indexTablicyCiaglych = new int[ilTablic];
            for (int i = 0; i < indexTablicyCiaglych.Length; i++)
            {
                indexTablicyCiaglych[i] = 0;
            }
            TabWartosci = new double[ilTablic, Convert.ToInt32(ilProb.Value)];
            zapisDoPliku.Enabled = false;
            MinValue.Enabled = false;
            MaxValue.Enabled = false;
            TypSczytywania.Enabled = false;
            czestotliwosc.Enabled = false;
            ilProb.Enabled = false;
            start.Enabled = false;
            stop.Enabled = true;
            usrednienie.Enabled = false;
            int czekaj = Convert.ToInt32(czestotliwosc.Value/1000);

            if (zapisDoPliku.Checked)
            {
                zapisywanie = true;
            }
            else
            {
                zapisywanie = false;
            }
            if (czestotliwosc.Value/usrednienie.Value<=30)
            {
                odswiezanieWykresu = 1;
            }
            else if (czestotliwosc.Value / usrednienie.Value <= 100)
            {
                odswiezanieWykresu = 8;
            }
            else if (czestotliwosc.Value / usrednienie.Value <= 500)
            {
                odswiezanieWykresu = 20;
            }
            else if (czestotliwosc.Value / usrednienie.Value <= 1000)
            {
                odswiezanieWykresu = 35;
            }
            else
            {
                odswiezanieWykresu = 55;
            }
            progressBar1.Maximum = Convert.ToInt32(ilProb.Value);
            if (TypSczytywania.Text == "Skończony" && ilTablic>0)
            {
                progressBar1.Visible = true;
                work = new BackgroundWorker();
                work.DoWork += new DoWorkEventHandler(work_DoWork);
                work.RunWorkerCompleted += new RunWorkerCompletedEventHandler(work_RunWorkerCompleted);
                work.ProgressChanged += new ProgressChangedEventHandler(work_progressChanged);
                work.WorkerReportsProgress = true;
                work.RunWorkerAsync();
                work.WorkerSupportsCancellation = true;
            }
            else if (TypSczytywania.Text == "Ciągły" && ilTablic>0)
            {
                progressBar1.Visible = false;
                start.Enabled = false;
                stop.Enabled = true;
                workC = new BackgroundWorker();
                workC.DoWork += new DoWorkEventHandler(workC_DoWork);
                workC.WorkerReportsProgress = true;
                workC.RunWorkerAsync();
                workC.WorkerSupportsCancellation = true;
            }
            else
            {
                MessageBox.Show("Prosze zaznaczyć conajlnniej jeden kanał do odczytu.");
                zapisDoPliku.Enabled = true;
                MinValue.Enabled = true;
                MaxValue.Enabled = true;
                TypSczytywania.Enabled = true;
                czestotliwosc.Enabled = true;
                ilProb.Enabled = true;
                start.Enabled = true;
                stop.Enabled = false;
                usrednienie.Enabled = true;
                for (int i = 0; i < checkBoxy.Length; i++)
                {
                    checkBoxy[i].Enabled = true;
                    checkBoxyZapis[i].Enabled = true;
                }
            }
        }

        private void workC_DoWork(object sender, DoWorkEventArgs e)
        {
            int nrPetli = 0;
            myTask = new Task();
            while (!workC.CancellationPending)
            {
                if (wartWykresu==null)
                {
                    wartWykresu = new List<double>[16];
                }

                Skonczony(nrPetli);
                
                nrPetli++;

                if (zatrzymajPetle)
                {
                    zatrzymajPetle = false;
                    break;
                }
            }
        }

        private void work_progressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (aTimer != null)
            {
                aTimer.Dispose();
            }

            MinValue.Enabled = true;
            MaxValue.Enabled = true;
            TypSczytywania.Enabled = true;
            czestotliwosc.Enabled = true;
            ilProb.Enabled = true;
            start.Enabled = true;
            zapisDoPliku.Enabled = true;
            stop.Enabled = false;
            usrednienie.Enabled = true;
            nazwaUrz.Enabled = true;
            for (int i = 0; i < checkBoxy.Length; i++)
            {
                checkBoxy[i].Enabled = true;
            }
            progressBar1.Visible = false;
            odswiezListeZapisu();
        }

        private void work_DoWork(object sender, DoWorkEventArgs e)
        {

            this.Invoke(new MethodInvoker(delegate {
                wykres.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(ilProb.Value);
            }));

            myTask = new Task();
            for (int i = 0; i < ilProb.Value; i++)
            {
                if (wartWykresu == null)
                {
                    wartWykresu = new List<double>[16];
                }

                Skonczony(i);
                
                
                work.ReportProgress(i);
                if (work.CancellationPending)
                {
                    break;
                }
                if (zatrzymajPetle)
                {
                    zatrzymajPetle = false;
                    break;
                }
            }
            moznaLiczyc = true;
            liczStatystyki();
            moznaLiczyc = false;

            work.ReportProgress(Convert.ToInt32(ilProb.Value));
        }

        private void stop_Click(object sender, EventArgs e)
        {
            zatrzymaj();
        }

        private void zatrzymaj()
        {
            aTimer.Enabled = false;
            aTimer.Stop();

            nazwaUrz.Enabled = true;
            for (int i = 0; i < checkBoxy.Length; i++)
            {
                checkBoxy[i].Enabled = true;
                checkBoxyZapis[i].Enabled = true;
            }
            zapisDoPliku.Enabled = true;
            MinValue.Enabled = true;
            usrednienie.Enabled = true;
            MaxValue.Enabled = true;
            TypSczytywania.Enabled = true;
            czestotliwosc.Enabled = true;
            ilProb.Enabled = true;
            start.Enabled = true;
            stop.Enabled = false;
            if (TypSczytywania.Text == "Ciągły")
            {
                workC.CancelAsync();
                myTask.Dispose();
            }
            if (TypSczytywania.Text == "Skończony")
            {
                work.CancelAsync();
            }
        }

        private void czysc_Click(object sender, EventArgs e)
        {
            czyscWykres();
        }

        private void czyscWykres()
        {
            TabWartosci = null;
            ilTablic = 0;
            wykres.Series.Clear();
            if (aTimer!=null)
            {
                aTimer.Stop();
            }
        }

        private void zapisywanieDoPliku(double wart, int nr, int nrPetli)
        {
            string wartosc = Math.Round(wart, (int)DokladnoscZapisuNumeric.Value) + "\n";
            int liczspr = 0;
            for (int i = 0; i < checkBoxy.Length; i++)
            {
                if (checkBoxyZapis[i].Checked)
                {
                    if (liczspr == nr)
                    {
                        File.AppendAllText(@path + "\\"+ nazwaZapisu[i] + ".txt",wartosc + Environment.NewLine);
                    }
                    liczspr++;
                }
            }
        }
       
        private void aktualizujStatystyki(object sender, ElapsedEventArgs e)
        {
            liczStatystyki();
        }

        private void liczStatystyki()
        {
            int od = Convert.ToInt32(wykres.ChartAreas[0].AxisX.Minimum);
            int ado = wykres.Series[0].Points.Count;
            for (int i = 0; i < wykres.Series.Count; i++)
            {
                if (ado >= wykres.Series[i].Points.Count)
                {
                    ado = wykres.Series[i].Points.Count;
                }
            }
            pomocnicza = null;
            pomocnicza = new List<double>();


            this.Invoke(new MethodInvoker(delegate
            {
                if (moznaLiczyc)
                {
                    for (int index = 0; index < wykres.Series.Count; index++)
                    {
                        pomocnicza.Clear();
                        srednia[index] = 0;
                        for (int j = od; j < ado - 1; j++)
                        {
                            pomocnicza.Add(wykres.Series[index].Points[j].YValues[0]);
                        }

                        srednia[index] = calculateAverage(pomocnicza);

                        sredniaLab[index].Text = Math.Round(srednia[index], 6) + "";

                        odchylenie[index] = calculateSTD(pomocnicza);

                        odchylenieLab[index].Text = Math.Round(odchylenie[index], 6) + "";

                        for (int i = 0; i < Convert.ToInt32(wykres.ChartAreas[0].AxisX.Minimum) - 1; i++)
                        {
                            wykres.Series[index].Points[i].YValues[0] = srednia[index];
                        }
                    }
                    wykres.ChartAreas[0].RecalculateAxesScale();
                }
            }));
        }

        private double calculateSTD(List<double> pomocnicza)
        {
            double average = calculateAverage(pomocnicza);
            double ximinusxsr = 0;
            for (int k = 0; k < pomocnicza.Count; k++)
            {
                ximinusxsr += (pomocnicza[k] - average) * (pomocnicza[k] - average);
            }
            return Math.Sqrt((ximinusxsr / pomocnicza.Count));
        }

        private double calculateAverage(List<double> pomocnicza)
        {
            double suma = 0;
            for (int i = 0; i < pomocnicza.Count; i++)
            {
                suma += pomocnicza[i];
            }
            return suma / pomocnicza.Count;
        }


        public void Skonczony(int nrPetli)
        {
            try
            {

                double sampleRate = Convert.ToDouble(czestotliwosc.Value);
                double rangeMinimum = Convert.ToDouble(MinValue.Value);
                double rangeMaximum = Convert.ToDouble(MaxValue.Value);
                int samplesPerChannel = Convert.ToInt32(usrednienie.Value);
                double suma = 0;

                if (nrPetli == 0)
                {
                    for (int i = 0; i < checkBoxy.Length; i++)
                    {
                        if (checkBoxy[i].Checked)
                        {
                            myTask.AIChannels.CreateVoltageChannel(checkBoxy[i].Text, "",
                                (AITerminalConfiguration)(-1), rangeMinimum, rangeMaximum, AIVoltageUnits.Volts);
                        }
                    }
                    
                    myTask.Timing.ConfigureSampleClock("", sampleRate, SampleClockActiveEdge.Rising,
                        SampleQuantityMode.FiniteSamples, samplesPerChannel);
                    
                    myTask.Control(TaskAction.Verify);
                    
                    reader = new AnalogMultiChannelReader(myTask.Stream);

                }
                data = reader.ReadWaveform(samplesPerChannel);

                int index = 0;
                double[] maxKanalu = new double[data.Length];

                int zwiekszZakresX = 0;
                foreach (AnalogWaveform<double> waveform in data)
                {
                    int l = 0;
                    for (int sample = 0; sample < usrednienie.Value; sample ++)
                    {
                        suma += waveform.Samples[sample].Value;
                    }
                    suma = suma / Convert.ToInt32(usrednienie.Value);
                    this.Invoke(new MethodInvoker(delegate {
                        if (wykres.Series[index].Points.Count <= ilProb.Value)
                        {

                            if (wartWykresu == null)
                            {
                                wartWykresu = new List<double>[16];
                            }
                            if (wartWykresu[index]==null)
                            {
                                wartWykresu[index] = new List<double>();
                            }

                            if (nrPetli + 1 == ilProb.Value)
                            {
                                for (int i = 0; i < wartWykresu[index].Count; i++)
                                {
                                    wykres.Series[index].Points.AddY(wartWykresu[index][i]);

                                    if (zapisywanie)
                                    {
                                        zapisywanieDoPliku(wartWykresu[index][i], index, nrPetli);
                                    }
                                }
                                wartWykresu[index].Clear();
                                moznaLiczyc = true;
                            }

                            wartWykresu[index].Add(suma);

                            if (wartWykresu[index].Count == odswiezanieWykresu)
                            {
                                for (int i = 0; i < wartWykresu[index].Count; i++)
                                {
                                   wykres.Series[index].Points.AddY(wartWykresu[index][i]);

                                    if (zapisywanie)
                                    {
                                        zapisywanieDoPliku(wartWykresu[index][i], index, nrPetli);
                                    }
                                }
                                wartWykresu[index].Clear();
                            }
                        }
                        else
                        {

                            if (wartWykresu == null)
                            {
                                wartWykresu = new List<double>[16];
                            }
                            if (wartWykresu[index] == null)
                            {
                                wartWykresu[index] = new List<double>();
                            }

                            wartWykresu[index].Add(suma);

                            if (wartWykresu[index].Count == odswiezanieWykresu)
                            {
                                for (int i = 0; i < wartWykresu[index].Count; i++)
                                {
                                    wykres.Series[index].Points.AddY(wartWykresu[index][i]);
                                    if (zwiekszZakresX == 0)
                                    {
                                        wykres.ChartAreas[0].AxisX.Minimum++;
                                        wykres.ChartAreas[0].AxisX.Maximum++;
                                    }

                                    if (zapisywanie)
                                    {
                                        zapisywanieDoPliku(wartWykresu[index][i], index, nrPetli);
                                    }
                                }
                                wartWykresu[index].Clear();
                            }
                        }
                    }));
                    nr = l;
                    l++;
                    suma = 0;
                    index++;
                    zwiekszZakresX++;
                    for (int w = 0; w < wykres.Series.Count; w++)
                    {
                        if (wykres.Series[w].Points.Count >= ilProb.Value)
                        {
                            moznaLiczyc = true;
                        }
                        else
                        {
                            moznaLiczyc = false;
                            break;
                        }
                    }
                }
            }
            catch (DaqException exception)
            {
                zatrzymajPetle = true;
                MessageBox.Show(exception.Message);
                zatrzymaj();
                if (myTask!=null)
                {
                    myTask.Dispose();
                }
            }
        }

        private void minOUT_ValueChanged(object sender, EventArgs e)
        {
            decimal temp = minOUT.Value;
            if (minOUT.Value >= maxOUT.Value)
            {
                MessageBox.Show("Minimalna wartość musi być mniejsza od maksymalnej wartości.");
                minOUT.Value = maxOUT.Value - 1;
            }
            wartoscOUT.Minimum = minOUT.Value;

            if (amplituda != null)
            {
                    amplituda.Maximum = (maxOUT.Value - minOUT.Value) / 2;
            }
        }

        private void maxOUT_ValueChanged(object sender, EventArgs e)
        {
            decimal temp = maxOUT.Value;
            if (minOUT.Value >= maxOUT.Value)
            {
                MessageBox.Show("Minimalna wartość musi być mniejsza od maksymalnej wartości.");
                maxOUT.Value = minOUT.Value + 1;
            }
            wartoscOUT.Maximum = maxOUT.Value;
            if (typOUT.Text != "Skończony")
            {
                amplituda.Maximum = (maxOUT.Value - minOUT.Value) / 2;
            }
        }

        private void ilKanOUT_TextChanged(object sender, EventArgs e)
        {
            if (ilKanOUT.Text == "1")
            {
                kanOUT2.Enabled = false;
                maxOutput2Value.Enabled = false;
                minOutput2Value.Enabled = false;
                maxOutput2Value.Visible = false;
                minOutput2Value.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
                if (typOUT.Text == "Skończony")
                {
                    StworzJedenSkonczony();
                }
                else if (typOUT.Text == "Sinusoidalny" || typOUT.Text == "Trójkątny" || typOUT.Text == "Kwadratowy" || typOUT.Text == "ZębyPiły")
                {
                    StworzJedenCiagly();
                }
                else if (typOUT.Text == "Własny")
                {
                    StworzJedenWlasny();
                }
                else
                {
                    MessageBox.Show("Prosze wybrać funkcje z listy");
                }
            }
            else
            {
                kanOUT1.Enabled = true;
                kanOUT2.Enabled = true;
                minOutput2Value.Enabled = true;
                maxOutput2Value.Enabled = true;
                maxOutput2Value.Visible = true;
                minOutput2Value.Visible = true;
                label2.Visible = true;
                label3.Visible = true;
                if (typOUT.Text == "Skończony")
                {
                    StworzDwaSkonczone();
                }
                else if (typOUT.Text == "Sinusoidalny" || typOUT.Text == "Trójkątny" || typOUT.Text == "Kwadratowy" || typOUT.Text == "ZębyPiły")
                {
                    StworzDwaCiagle();
                }
                else if (typOUT.Text == "Własny")
                {
                    StworzDwaWlasny();
                }
                else
                {
                    MessageBox.Show("Prosze wybrać funkcje z listy");
                }
            }
        }

        private void StworzJedenCiagly()
        {
            groupBox5.Controls.Clear();
            labelsinus = new Label();
            amplituda = new NumericUpDown();
            amplituda.DecimalPlaces = 4;
            labelsinus.Text = "Amplituda sygnału (V):";
            labelsinus.AutoSize = true;
            labelsinus.Location = new Point(6, 16);
            amplituda.AutoSize = true;
            amplituda.Location = new Point(label12.Location.X + 2, 30);
            amplituda.Minimum = 0;
            amplituda.Maximum = (maxOUT.Value - minOUT.Value) / 2;
            amplituda.Value= (maxOUT.Value - minOUT.Value) / 4;
            groupBox5.Controls.Add(labelsinus);
            groupBox5.Controls.Add(amplituda);
        }

        private void StworzDwaCiagle()
        {
            groupBox5.Controls.Clear();
            StworzJedenCiagly();
            labelsinus2 = new Label();
            amplituda2 = new NumericUpDown();
            amplituda2.DecimalPlaces = 4;
            labelsinus2.Text = "Amplituda sygnału 2 (V):";
            labelsinus2.AutoSize = true;
            labelsinus2.Location = new Point(groupBox5.Width / 2, 16);
            amplituda2.AutoSize = true;
            amplituda2.Location = new Point(labelsinus2.Location.X + 2, 30);
            amplituda2.Minimum = 0;
            amplituda2.Maximum = (maxOUT.Value - minOUT.Value) / 2;
            groupBox5.Controls.Add(labelsinus2);
            groupBox5.Controls.Add(amplituda2);
        }

        private void StworzJedenSkonczony()
        {
            groupBox5.Controls.Clear();
            label12 = new Label();
            wartoscOUT = new NumericUpDown();
            wartoscOUT.DecimalPlaces = 4;
            wartoscOUT.Minimum = minOUT.Value;
            wartoscOUT.Maximum = maxOUT.Value;
            label12.Text = "Wartość 1 kanału (V):";
            label12.AutoSize = true;
            label12.Location = new Point(6, 16);
            wartoscOUT.AutoSize = true;
            wartoscOUT.Location = new Point(label12.Location.X + 2, 30);
            groupBox5.Controls.Add(label12);
            groupBox5.Controls.Add(wartoscOUT);
        }

        private void StworzDwaSkonczone()
        {
            StworzJedenSkonczony();
            l = new Label();
            wartoscOUT2 = new NumericUpDown();
            wartoscOUT2.DecimalPlaces = 4;
            wartoscOUT2.Minimum = minOUT.Value;
            wartoscOUT2.Maximum = maxOUT.Value;
            l.Text = "Wartość 2 kanału (V):";
            wartoscOUT2.Minimum = minOUT.Value;
            wartoscOUT2.Maximum = maxOUT.Value;
            l.AutoSize = true;
            l.Location = new Point(groupBox5.Width / 2, 16);
            wartoscOUT2.AutoSize = true;
            wartoscOUT2.Location = new Point(l.Location.X + 2, wartoscOUT.Location.Y);
            groupBox5.Controls.Add(l);
            groupBox5.Controls.Add(wartoscOUT2);
        }

        private void stopuot_Click(object sender, EventArgs e)
        {
            startout.Enabled = true;
            stopuot.Enabled = false;
            ilKanOUT.Enabled = true;
            kanOUT1.Enabled = true;
            if (ilKanOUT.Text=="1")
            {
                kanOUT2.Enabled = false;
            }
            else
            {
                kanOUT2.Enabled = true;
            }
            minOUT.Enabled = true;
            maxOUT.Enabled = true;
            minOutput2Value.Enabled = true;
            maxOutput2Value.Enabled = true;
            typOUT.Enabled = true;
            czestOUT.Enabled = true;
            workerOut.CancelAsync();
            if (myTaskOUT != null)
            {
                myTaskOUT.Dispose();
            }
            if (myTaskOUT2 != null)
            {
                myTaskOUT2.Dispose();
            }
        }

        private void startout_Click(object sender, EventArgs e)
        {
            wykresOut.Series.Clear();
           
                wykresOut.ChartAreas[0].AxisX.Minimum = 0;
                wykresOut.ChartAreas[0].AxisX.Maximum = 100;

                    odswiezanieWykresuOut = 8;

                    if (wartWykresuOut!=null)
                    {
                    wartWykresuOut = null;
                    }

            startout.Enabled = false;
            stopuot.Enabled = true;
            ilKanOUT.Enabled = false;
            kanOUT1.Enabled = false;
            kanOUT2.Enabled = false;
            minOUT.Enabled = false;
            maxOUT.Enabled = false;
            minOutput2Value.Enabled = false;
            maxOutput2Value.Enabled = false;
            typOUT.Enabled = false;
            czestOUT.Enabled = false;
            wykresOut.Series.Add(kanOUT1.Text);
            wykresOut.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            if (ilKanOUT.Text=="2")
            {
                wykresOut.Series.Add(kanOUT2.Text);
                wykresOut.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            }
            if (typOUT.Text== "Skończony")
            {
                SkonczonyOUT();
                myTaskOUT.Dispose();
                if (ilKanOUT.Text=="2")
                {
                    myTaskOUT2.Dispose();
                }
                startout.Enabled = true;
                stopuot.Enabled = false;
                ilKanOUT.Enabled = true;
                kanOUT1.Enabled = true;
                kanOUT2.Enabled = true;
                minOUT.Enabled = true;
                maxOUT.Enabled = true;
                typOUT.Enabled = true;
                czestOUT.Enabled = true;
            }
            else if(typOUT.Text == "Sinusoidalny")
            {
                workerOut = new BackgroundWorker();
                workerOut.DoWork += new DoWorkEventHandler(workerOut_DoWorkSinusoidalnie);
                workerOut.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerOut_RunWorkerCompleted);
                workerOut.RunWorkerAsync();
                workerOut.WorkerSupportsCancellation = true;
            }
            else if (typOUT.Text == "Trójkątny")
            {
                workerOut = new BackgroundWorker();
                workerOut.DoWork += new DoWorkEventHandler(workerOut_DoWorkTr);
                workerOut.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerOut_RunWorkerCompleted);
                workerOut.RunWorkerAsync();
                workerOut.WorkerSupportsCancellation = true;
            }
            else if (typOUT.Text == "Kwadratowy")
            {
                workerOut = new BackgroundWorker();
                workerOut.DoWork += new DoWorkEventHandler(workerOut_DoWorkKw);
                workerOut.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerOut_RunWorkerCompleted);
                workerOut.RunWorkerAsync();
                workerOut.WorkerSupportsCancellation = true;
            }
            else if (typOUT.Text == "ZębyPiły")
            {
                workerOut = new BackgroundWorker();
                workerOut.DoWork += new DoWorkEventHandler(workerOut_DoZP);
                workerOut.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerOut_RunWorkerCompleted);
                workerOut.RunWorkerAsync();
                workerOut.WorkerSupportsCancellation = true;
            }
            else if (typOUT.Text == "Własny")
            {
                workerOut = new BackgroundWorker();
                workerOut.DoWork += new DoWorkEventHandler(workerOut_DoWorkWl);
                workerOut.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerOut_RunWorkerCompleted);
                workerOut.RunWorkerAsync();
                workerOut.WorkerSupportsCancellation = true;
            }
        }

        private void workerOut_DoWorkWl(object sender, DoWorkEventArgs e)
        {
            string nazwa1 = "", nazwa2 = "", ilkan = "", funkcja1 = "", funkcja2 = "";
            double xmin = 0, xmax = 0;
            double xmin2 = 0, xmax2 = 0;
            int czest = 1000;
            this.Invoke(new MethodInvoker(delegate {
                ilkan = ilKanOUT.Text;
                nazwa1 = kanOUT1.Text;
                funkcja1 = wlasny.Text;
                xmin = (double)minOUT.Value;
                xmax = (double)maxOUT.Value;
                czest = Convert.ToInt32(czestOUT.Value);
                if (ilKanOUT.Text == "2")
                {
                    xmin2 = (double)minOutput2Value.Value;
                    xmax2 = (double)maxOutput2Value.Value;
                    funkcja2 = wlasny2.Text;
                    nazwa2 = kanOUT2.Text;
                }
            }));
            double wart = 0;
            double wart2 = 0;
            double x = 0.1;
            double x2 = 0.1;
            int kierunek = 1, kierunek2 = 1;
            int nrPetli = 0;
            myTaskOUT = new Task();
            if (ilkan == "2")
            {
                myTaskOUT2 = new Task();
            }

            while (!workerOut.CancellationPending)
            {
                try
                {
                    Dictionary<string, double> variables = new Dictionary<string, double>();
                    variables.Add("x", x);

                    CalculationEngine engine = new CalculationEngine();
                    wart = engine.Calculate(funkcja1, variables);
                    variables = null;
                }
                catch (Exception eM)
                {
                    MessageBox.Show(eM.ToString());
                    workerOut.CancelAsync();
                    break;
                }
                if (ilkan == "2")
                {
                    try
                    {
                        Dictionary<string, double> variables2 = new Dictionary<string, double>();
                        variables2.Add("x", x2);

                        CalculationEngine engine = new CalculationEngine();
                        wart2 = engine.Calculate(funkcja2, variables2);
                        variables2 = null;
                    }
                    catch (Exception eM)
                    {
                        MessageBox.Show(eM.ToString());
                        workerOut.CancelAsync();
                        break;
                    }
                }
                if (wart <= xmax && wart >= xmin && wart2 <= xmax2 && wart2 >= xmin2)
                {
                    this.Invoke(new MethodInvoker(delegate {
                        if (wartWykresuOut == null)
                        {
                            wartWykresuOut = new List<double>[2];
                        }
                        if (wartWykresuOut[0]==null)
                        {
                            wartWykresuOut[0] = new List<double>();
                        }
                        wartWykresuOut[0].Add(wart);
                        if (wartWykresuOut[0].Count==odswiezanieWykresuOut)
                        {
                            for (int i = 0; i < wartWykresuOut[0].Count; i++)
                            {
                                wykresOut.Series[0].Points.AddY(wartWykresuOut[0][i]);
                                if (wykresOut.Series[0].Points.Count > 100)
                                {
                                    wykresOut.ChartAreas[0].AxisX.Minimum++;
                                    wykresOut.ChartAreas[0].AxisX.Maximum++;
                                }
                            }
                            wartWykresuOut[0].Clear();
                        }
                        if (ilKanOUT.Text == "2")
                        {
                            if (wartWykresuOut[1] == null)
                            {
                                wartWykresuOut[1] = new List<double>();
                            }
                            wartWykresuOut[1].Add(wart2);
                            if (wartWykresuOut[1].Count ==odswiezanieWykresuOut)
                            {
                                for (int i = 0; i < wartWykresuOut[1].Count; i++)
                                {
                                    wykresOut.Series[1].Points.AddY(wartWykresuOut[1][i]);
                                }
                                wartWykresuOut[1].Clear();
                            }
                        }
                    }));
                    CiaglyOUT(wart, wart2, nazwa1, nazwa2, ilkan, nrPetli);
                }
                x += 0.01 * kierunek;
                if (wart >= xmax)
                {
                    kierunek = -1;
                    x -= 0.01;
                }
                else if (wart <= xmin)
                {
                    kierunek = 1;
                    x += 0.01;
                }
                if (ilkan == "2")
                {
                    x2 += 0.01 * kierunek2;
                    if (wart2 >= xmax2)
                    {
                        kierunek2 = -1;
                        x2 -= 0.01;
                    }
                    else if (wart2 <= xmin2)
                    {
                        kierunek2 = 1;
                        x2 += 0.01;
                    }
                }
                Thread.Sleep(1000 / czest);
                nrPetli++;
            }
            workerOut.DoWork -= workerOut_DoWorkWl;
            string funkcja = "";
            this.Invoke(new MethodInvoker(delegate {
                funkcja = wlasny.Text;
            }));
           
        }

        private void workerOut_DoZP(object sender, DoWorkEventArgs e)
        {
            string nazwa1 = "", nazwa2 = "", ilkan = "";
            double srodek = 0, srodek2 = 0;
            double ampl1 = 0, ampl2 = 0;
            int czest = 1000;
            this.Invoke(new MethodInvoker(delegate {
                ilkan = ilKanOUT.Text;
                nazwa1 = kanOUT1.Text;
                srodek = Convert.ToDouble(maxOUT.Value + minOUT.Value) / 2;
                ampl1 = Convert.ToDouble(amplituda.Value);
                czest = Convert.ToInt32(czestOUT.Value);
                if (ilKanOUT.Text == "2")
                {
                    ampl2 = Convert.ToDouble(amplituda2.Value);
                    srodek2 = Convert.ToDouble(maxOutput2Value.Value + minOutput2Value.Value) / 2;
                    nazwa2 = kanOUT2.Text;
                }
            }));
            double wart = 0;
            double wart2 = 0;
            double x1 = 0;
            double x2 = 0;

            myTaskOUT = new Task();
            if (ilkan == "2")
            {
                myTaskOUT2 = new Task();
            }
            int nrPetli = 0;
            while (!workerOut.CancellationPending)
            {
                wart = x1 * ampl1 + srodek;
                wart2 = x2 * ampl2 + srodek2; 
                this.Invoke(new MethodInvoker(delegate
                {
                    if (wartWykresuOut == null)
                    {
                        wartWykresuOut = new List<double>[2];
                    }
                    if (wartWykresuOut[0] == null)
                    {
                        wartWykresuOut[0] = new List<double>();
                    }
                    wartWykresuOut[0].Add(wart);
                    if (wartWykresuOut[0].Count ==odswiezanieWykresuOut)
                    {
                        for (int i = 0; i < wartWykresuOut[0].Count; i++)
                        {
                            wykresOut.Series[0].Points.AddY(wartWykresuOut[0][i]);
                            if (wykresOut.Series[0].Points.Count > 100)
                            {
                                wykresOut.ChartAreas[0].AxisX.Minimum++;
                                wykresOut.ChartAreas[0].AxisX.Maximum++;
                            }
                        }
                        wartWykresuOut[0].Clear();
                    }
                    if (ilKanOUT.Text == "2")
                    {
                        if (wartWykresuOut[1] == null)
                        {
                            wartWykresuOut[1] = new List<double>();
                        }
                        wartWykresuOut[1].Add(wart2);
                        if (wartWykresuOut[1].Count ==odswiezanieWykresuOut)
                        {
                            for (int i = 0; i < wartWykresuOut[1].Count; i++)
                            {
                                wykresOut.Series[1].Points.AddY(wartWykresuOut[1][i]);
                            }
                            wartWykresuOut[1].Clear();
                        }
                    }
                }));
                CiaglyOUT(wart, wart2, nazwa1, nazwa2, ilkan, nrPetli);
                x1 += 0.05;
                if (x1>=1)
                {
                    x1 = -1;
                }
                x2 += 0.05;
                if (x2 >= 1)
                {
                    x2 = -1;
                }
                Thread.Sleep(1000 / czest);
                nrPetli++;
            }
            workerOut.DoWork -= workerOut_DoZP;
        }

        private void workerOut_DoWorkKw(object sender, DoWorkEventArgs e)
        {
            int kierunek = 1;
            string nazwa1 = "", nazwa2 = "", ilkan = "";
            double srodek = 0, srodek2 = 0;
            double ampl1 = 0, ampl2 = 0;
            int czest = 1000;
            this.Invoke(new MethodInvoker(delegate {
                ilkan = ilKanOUT.Text;
                nazwa1 = kanOUT1.Text;
                srodek = Convert.ToDouble(maxOUT.Value + minOUT.Value) / 2;
                ampl1 = Convert.ToDouble(amplituda.Value);
                czest = Convert.ToInt32(czestOUT.Value);
                if (ilKanOUT.Text == "2")
                {
                    ampl2 = Convert.ToDouble(amplituda2.Value);
                    srodek2 = Convert.ToDouble(maxOutput2Value.Value + minOutput2Value.Value) / 2;
                    nazwa2 = kanOUT2.Text;
                }
            }));
            double wart = 0;
            double wart2 = 0;
            double x = 0;
            myTaskOUT = new Task();
            if (ilkan == "2")
            {
                myTaskOUT2 = new Task();
            }
            int nrPetli = 0;
            while (!workerOut.CancellationPending)
            {
                wart = x * ampl1 + srodek;
                wart2 = x * ampl2 + srodek2;
                CiaglyOUT(wart, wart2, nazwa1, nazwa2, ilkan,nrPetli);
                x = kierunek;
                kierunek *= -1;
                Thread.Sleep(1000 / czest);
                nrPetli++;
                this.Invoke(new MethodInvoker(delegate
                {
                    if (wartWykresuOut == null)
                    {
                        wartWykresuOut = new List<double>[2];
                    }
                    if (wartWykresuOut[0] == null)
                    {
                        wartWykresuOut[0] = new List<double>();
                    }
                    wartWykresuOut[0].Add(wart);
                    if (wartWykresuOut[0].Count ==odswiezanieWykresuOut)
                    {
                        for (int i = 0; i < wartWykresuOut[0].Count; i++)
                        {
                            wykresOut.Series[0].Points.AddY(wartWykresuOut[0][i]);
                            if (wykresOut.Series[0].Points.Count > 100)
                            {
                                wykresOut.ChartAreas[0].AxisX.Minimum++;
                                wykresOut.ChartAreas[0].AxisX.Maximum++;
                            }
                        }
                        wartWykresuOut[0].Clear();
                    }
                    if (ilKanOUT.Text == "2")
                    {
                        if (wartWykresuOut[1] == null)
                        {
                            wartWykresuOut[1] = new List<double>();
                        }
                        wartWykresuOut[1].Add(wart2);
                        if (wartWykresuOut[1].Count ==odswiezanieWykresuOut)
                        {
                            for (int i = 0; i < wartWykresuOut[1].Count; i++)
                            {
                                wykresOut.Series[1].Points.AddY(wartWykresuOut[1][i]);
                            }
                            wartWykresuOut[1].Clear();
                        }
                    }
                }));
            }
            workerOut.DoWork -= workerOut_DoWorkKw;
        }

        private void workerOut_DoWorkTr(object sender, DoWorkEventArgs e)
        {
            int kierunek = 1;
            string nazwa1 = "", nazwa2 = "", ilkan = "";
            double srodek = 0, srodek2 = 0;
            double ampl1 = 0, ampl2 = 0;
            int czest = 1000;
            this.Invoke(new MethodInvoker(delegate {
                ilkan = ilKanOUT.Text;
                nazwa1 = kanOUT1.Text;
                srodek = Convert.ToDouble(maxOUT.Value + minOUT.Value) / 2;
                ampl1 = Convert.ToDouble(amplituda.Value);
                czest = Convert.ToInt32(czestOUT.Value);
                if (ilKanOUT.Text == "2")
                {
                    ampl2 = Convert.ToDouble(amplituda2.Value);
                    srodek2 = Convert.ToDouble(maxOutput2Value.Value + minOutput2Value.Value) / 2;
                    nazwa2 = kanOUT2.Text;
                }
            }));
            double wart = 0;
            double wart2 = 0;
            double x = 0;
            myTaskOUT = new Task();
            if (ilkan == "2")
            {
                myTaskOUT2 = new Task();
            }
            int nrPetli = 0;
            while (!workerOut.CancellationPending)
            {
                wart = x * ampl1 + srodek;
                wart2 = x * ampl2 + srodek2;
                this.Invoke(new MethodInvoker(delegate
                {
                    if (wartWykresuOut == null)
                    {
                        wartWykresuOut = new List<double>[2];
                    }
                    if (wartWykresuOut[0] == null)
                    {
                        wartWykresuOut[0] = new List<double>();
                    }
                    wartWykresuOut[0].Add(wart);
                    if (wartWykresuOut[0].Count ==odswiezanieWykresuOut)
                    {
                        for (int i = 0; i < wartWykresuOut[0].Count; i++)
                        {
                            wykresOut.Series[0].Points.AddY(wartWykresuOut[0][i]);
                            if (wykresOut.Series[0].Points.Count > 100)
                            {
                                wykresOut.ChartAreas[0].AxisX.Minimum++;
                                wykresOut.ChartAreas[0].AxisX.Maximum++;
                            }
                        }
                        wartWykresuOut[0].Clear();
                    }
                    if (ilKanOUT.Text == "2")
                    {
                        if (wartWykresuOut[1] == null)
                        {
                            wartWykresuOut[1] = new List<double>();
                        }
                        wartWykresuOut[1].Add(wart2);
                        if (wartWykresuOut[1].Count ==odswiezanieWykresuOut)
                        {
                            for (int i = 0; i < wartWykresuOut[1].Count; i++)
                            {
                                wykresOut.Series[1].Points.AddY(wartWykresuOut[1][i]);
                            }
                            wartWykresuOut[1].Clear();
                        }
                    }
                }));
                CiaglyOUT(wart, wart2, nazwa1, nazwa2, ilkan, nrPetli);

                x += 0.01 * kierunek;
                if (x>=1)
                {
                    kierunek = -1;
                    x -= 0.01;
                }
                else if (x<=-1)
                {
                    kierunek = 1;
                    x += 0.01;
                }
                Thread.Sleep(1000 / czest);
                nrPetli++;
            }
            workerOut.DoWork -= workerOut_DoWorkTr;
        }

        private void workerOut_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            startout.Enabled = true;
            stopuot.Enabled = false;
            ilKanOUT.Enabled = true;
            kanOUT1.Enabled = true;
            kanOUT2.Enabled = true;
            minOUT.Enabled = true;
            maxOUT.Enabled = true;
            typOUT.Enabled = true;
            czestOUT.Enabled = true;
        }

        private void workerOut_DoWorkSinusoidalnie(object sender, DoWorkEventArgs e)
        {
            string nazwa1 = "", nazwa2 = "", ilkan="";
            double srodek = 0, srodek2 = 0;
            double ampl1 = 0, ampl2 = 0;
            int czest=1000;
            this.Invoke(new MethodInvoker(delegate {
                ilkan = ilKanOUT.Text;
                nazwa1 = kanOUT1.Text;
                srodek = Convert.ToDouble(maxOUT.Value + minOUT.Value) / 2;
                ampl1 = Convert.ToDouble(amplituda.Value);
                czest = Convert.ToInt32(czestOUT.Value);
                if (ilKanOUT.Text == "2")
                {
                    ampl2 = Convert.ToDouble(amplituda2.Value);
                    srodek2 = Convert.ToDouble(maxOutput2Value.Value + minOutput2Value.Value) / 2;
                    nazwa2 = kanOUT2.Text;
                }
            }));
            double wart = 0;
            double wart2 = 0;
            double x = 0;
            int nrPetli = 0;
            myTaskOUT = new Task();
            if (ilkan == "2")
            {
                myTaskOUT2 = new Task();
            }
            while (!workerOut.CancellationPending)
            {
                wart = Math.Sin(x) * ampl1 + srodek;
                wart2 = Math.Sin(x) * ampl2 + srodek2;
                this.Invoke(new MethodInvoker(delegate
                {
                    if (wartWykresuOut == null)
                    {
                        wartWykresuOut = new List<double>[2];
                    }
                    if (wartWykresuOut[0] == null)
                    {
                        wartWykresuOut[0] = new List<double>();
                    }
                    wartWykresuOut[0].Add(wart);
                    if (wartWykresuOut[0].Count ==odswiezanieWykresuOut)
                    {
                        for (int i = 0; i < wartWykresuOut[0].Count; i++)
                        {
                            wykresOut.Series[0].Points.AddY(wartWykresuOut[0][i]);
                            if (wykresOut.Series[0].Points.Count > 100)
                            {
                                wykresOut.ChartAreas[0].AxisX.Minimum++;
                                wykresOut.ChartAreas[0].AxisX.Maximum++;
                            }
                        }
                        wartWykresuOut[0].Clear();
                    }
                    if (ilKanOUT.Text == "2")
                    {
                        if (wartWykresuOut[1] == null)
                        {
                            wartWykresuOut[1] = new List<double>();
                        }
                        wartWykresuOut[1].Add(wart2);
                        if (wartWykresuOut[1].Count ==odswiezanieWykresuOut)
                        {
                            for (int i = 0; i < wartWykresuOut[1].Count; i++)
                            {
                                wykresOut.Series[1].Points.AddY(wartWykresuOut[1][i]);
                            }
                            wartWykresuOut[1].Clear();
                        }
                    }
                }));
                CiaglyOUT(wart, wart2, nazwa1, nazwa2, ilkan, nrPetli);
                x += 0.1;
                Thread.Sleep(1000/czest);
                nrPetli++;
            }
            workerOut.DoWork -= workerOut_DoWorkSinusoidalnie;
        }

        private void typOUT_TextChanged(object sender, EventArgs e)
        {
            if (ilKanOUT.Text == "1")
            {
                if (typOUT.Text == "Skończony")
                {
                    StworzJedenSkonczony();
                }
                else if (typOUT.Text == "Sinusoidalny" || typOUT.Text == "Trójkątny" || typOUT.Text == "Kwadratowy" || typOUT.Text == "ZębyPiły")
                {
                    StworzJedenCiagly();
                }
                else if (typOUT.Text == "Własny")
                {
                    StworzJedenWlasny();
                }
                else
                {
                    MessageBox.Show("Prosze wybrać funkcje z listy.");
                }
            }
            else
            {
                if (typOUT.Text == "Skończony")
                {
                    StworzDwaSkonczone();
                }
                else if (typOUT.Text == "Sinusoidalny" || typOUT.Text == "Trójkątny" || typOUT.Text == "Kwadratowy" || typOUT.Text == "ZębyPiły")
                {
                    StworzDwaCiagle();
                }
                else if (typOUT.Text == "Własny")
                {
                    StworzDwaWlasny();
                }
                else
                {
                    MessageBox.Show("Prosze wybrać funkcje z listy.");
                }
            }
        }


        private void StworzDwaWlasny()
        {
            StworzJedenWlasny();
            l = new Label();
            wlasny2 = new RichTextBox();
            l.Text = "Podaj funkcje kanału 2:";
            l.AutoSize = true;
            l.Location = new Point(groupBox5.Width / 2, 16);
            wlasny2.Width = groupBox5.Width / 2 - 10;
            wlasny2.Location = new Point(l.Location.X + 2, wartoscOUT.Location.Y);
            groupBox5.Controls.Add(l);
            groupBox5.Controls.Add(wlasny2);
        }


        private void StworzJedenWlasny()
        {
            groupBox5.Controls.Clear();
            labelInfo = new Label();
            labelInfo.Text = "Prosze używać tylko funkcji: sin, cos, loge oraz log10 \n w postaci np.: sin(x)+cos(x)+loge(x)+log10(x) \n konieczne jest użycie x jako zmiennej.";
            label12 = new Label();
            wlasny = new RichTextBox();
            label12.Text = "Podaj funkcje:";
            label12.AutoSize = true;
            label12.Location = new Point(6, 16);
            wlasny.Width = groupBox5.Width / 2 - 10;
            wlasny.Location = new Point(label12.Location.X + 2, 30);
            labelInfo.Location = new Point(label12.Location.X + 2, wlasny.Location.Y + wlasny.Height + 2);
            labelInfo.Height *= 2;
            labelInfo.Width = groupBox5.Width - 6;
            groupBox5.Controls.Add(label12);
            groupBox5.Controls.Add(wlasny);
            groupBox5.Controls.Add(labelInfo);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkBoxy.Length; i++)
            {
                checkBoxy[i].Text = nazwaUrz.Text + "/ai" + i;
            }
        }

        private void zapisDoPliku_Click(object sender, EventArgs e)
        {
            odswiezListeZapisu();
            if (infoZapis==0)
            {
                if (zapisDoPliku.Checked)
                {
                    MessageBox.Show("Aktualne miejsce zapisu: \n" + path + "\n Aby zmienić nazwe pliku kliknij prawym przyciskiem myszy na kanał ");
                }
                infoZapis++;
            }
        }

        private void stworzListe(int i, double wart, int licz)
        {
            TabWartosci[i, indexTablicyCiaglych[i]] = wart;
            if (indexTablicyCiaglych[i]==ilProb.Value-1)
            {
                indexTablicyCiaglych[i] = 0;
            }
            else
            {
                indexTablicyCiaglych[i] += 1;
            }
        }

        private void TypZczytywania_TextChanged(object sender, EventArgs e)
        {
            if (TypSczytywania.Text=="Ciągły")
            {
                label7.Visible = true;
                ilProb.Visible = true;
            }
            else
            {
                label7.Visible = true;
                ilProb.Visible = true;
            }
        }

        private void minOutput2Value_ValueChanged(object sender, EventArgs e)
        {
            decimal temp = minOutput2Value.Value;
            if (minOutput2Value.Value >= maxOutput2Value.Value)
            {
                MessageBox.Show("Minimalna wartość musi być mniejsza od maksymalnej wartości.");
                minOutput2Value.Value = maxOutput2Value.Value - 1;
            }
            if (wartoscOUT2!=null)
            {
                wartoscOUT2.Minimum = minOutput2Value.Value;
            }
           

            if (typOUT.Text != "Skończony")
            {
                amplituda2.Maximum = (maxOutput2Value.Value - minOutput2Value.Value) / 2;
            }
        }

        private void maxOutput2Value_ValueChanged(object sender, EventArgs e)
        {
            decimal temp = maxOutput2Value.Value;
            if (minOutput2Value.Value >= maxOutput2Value.Value)
            {
                MessageBox.Show("Minimalna wartość musi być mniejsza od maksymalnej wartości.");
                maxOutput2Value.Value = minOutput2Value.Value + 1;
            }
            if (wartoscOUT2!=null)
            {
                wartoscOUT2.Maximum = maxOutput2Value.Value;
            }
            if (typOUT.Text != "Skończony")
            {
                amplituda2.Maximum = (maxOutput2Value.Value - minOutput2Value.Value) / 2;
            }
        }

        private void kanOUT1_TextChanged(object sender, EventArgs e)
        {
            if (kanOUT1.Text== "Dev1/ao0")
            {
                kanOUT2.Text = "Dev1/ao1";
            }
            else
            {
                kanOUT2.Text = "Dev1/ao0";
            }
           
        }

        private void kanOUT2_TextChanged(object sender, EventArgs e)
        {
            if (kanOUT2.Text == "Dev1/ao0")
            {
                kanOUT1.Text = "Dev1/ao1";
            }
            else
            {
                kanOUT1.Text = "Dev1/ao0";
            }
        }

        private void mscZapisu_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    path = fbd.SelectedPath;

                    System.Windows.Forms.MessageBox.Show("Wybrana ścieżka: \n" + path, "Message");
                }
            }
        }

        private void SkonczonyOUT()
        {
            try
            {
                myTaskOUT = new Task();
                myTaskOUT.AOChannels.CreateVoltageChannel(kanOUT1.Text, "aoChannel",
                    Convert.ToDouble(minOUT.Text), Convert.ToDouble(maxOUT.Text),
                    AOVoltageUnits.Volts);
                AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(myTaskOUT.Stream);
                writer.WriteSingleSample(true, Convert.ToDouble(wartoscOUT.Text));
                if (ilKanOUT.Text=="2")
                {
                    myTaskOUT2 = new Task();
                    myTaskOUT2.AOChannels.CreateVoltageChannel(kanOUT2.Text, "aoChannel",
                    Convert.ToDouble(minOUT.Text), Convert.ToDouble(maxOUT.Text),
                    AOVoltageUnits.Volts);
                    AnalogSingleChannelWriter writer2 = new AnalogSingleChannelWriter(myTaskOUT2.Stream);
                    writer2.WriteSingleSample(true, Convert.ToDouble(wartoscOUT2.Text));
                }
            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);
                if (myTaskOUT!=null)
                {
                    myTaskOUT.Dispose();
                }
                if (myTaskOUT2!=null)
                {
                    myTaskOUT2.Dispose();
                }
            }
        }

        private void CiaglyOUT(double wart, double wart2, string nazwa1, string nazwa2, string ilkan, int nrPetli)
        {
            try
            {
                if (!workerOut.CancellationPending)
                {
                    if (nrPetli==0)
                    {
                        myTaskOUT.AOChannels.CreateVoltageChannel(nazwa1, "aoChannel",
                        Convert.ToDouble(minOUT.Text), Convert.ToDouble(maxOUT.Text),
                        AOVoltageUnits.Volts);
                    }
                   
                    AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(myTaskOUT.Stream);
                    writer.WriteSingleSample(true, wart);

                    if (ilkan == "2")
                    {
                        if (nrPetli==0)
                        {
                            myTaskOUT2.AOChannels.CreateVoltageChannel(nazwa2, "aoChannel",
                        Convert.ToDouble(minOUT.Text), Convert.ToDouble(maxOUT.Text),
                        AOVoltageUnits.Volts);
                        }
                       
                        AnalogSingleChannelWriter writer2 = new AnalogSingleChannelWriter(myTaskOUT2.Stream);
                        writer2.WriteSingleSample(true, wart2);
                    }
                }
            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);
                if (myTaskOUT != null)
                {
                    myTaskOUT.Dispose();
                }
                if (myTaskOUT2 != null)
                {
                    myTaskOUT2.Dispose();
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }
    }
}