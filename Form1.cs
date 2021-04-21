using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;

namespace GANGLIONSendSMS
{
    public partial class Form1 : Form
    {
        FbConnection dataBaseConnection;

        System.Timers.Timer _timer;

        private int _ErrorCount;

        // Hide window from alt+tab
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr window, int index, int value);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr window, int index);
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_APPWINDOW = 0x00040000;
        const string A = "";


        public Form1()
        {

            logger.writeToLogAlways("Programstartet ");
            _ErrorCount = 0;
            try
            {
                InitializeComponent();
                if (dataBaseConnection == null)
                {
                    initConnectionToDb();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Fejl i forbindelse til database(r). \n\n" + e.ToString());
                lukProgrammet.PerformClick();
            }

            autoSender();
        }

        private void initConnectionToDb()
        {

            FbConnectionStringBuilder cs = new FbConnectionStringBuilder();
            cs.DataSource = "5.179.93.89"; // Gammel: "5.179.93.89"
            cs.Port = 3050;
            cs.Database = @"C:\MASTERLOG\SMS.IB";
            cs.UserID = "SYSDBA";
            cs.Role = "";
            cs.Password = "J6%wN6wg";
            cs.Dialect = 3;
            cs.Charset = "";
            dataBaseConnection = new FbConnection(cs.ToString());

        }

        public void autoSender()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = 60000;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            sendAllSMS();
        }

        public void balloonTipSending(string s)
        {
            notifyIcon1.ShowBalloonTip(2000, "", "Sender " + s + " SMS'er. med Callback", ToolTipIcon.Info);
        }

        public void balloonTipDBError(string s)
        {
            notifyIcon1.ShowBalloonTip(2000, "Kan ikke forbinde til serveren", "Det er ikke muligt at forbinde til 5.179.93.84: " + s, ToolTipIcon.Error);
        }

        public void sendAllSMS()
        {
            _timer.Stop();

            // program må kun sende sms mellem 07:00 og 23:00
            // Calculate what day of the week is 36 days from this instant.
            System.DateTime today = System.DateTime.Today;
            //             System.TimeSpan duration = new System.TimeSpan(0, 8, 0, 0);
            System.TimeSpan duration = new System.TimeSpan(0, 7, 0, 0);
            System.DateTime answer1 = today.Add(duration);

            System.DateTime today2 = System.DateTime.Today;
            //             System.TimeSpan duration2 = new System.TimeSpan(0, 23, 0, 0);
            System.TimeSpan duration2 = new System.TimeSpan(0, 23, 59, 59);
            System.DateTime answer2 = today2.Add(duration2);
            int result = DateTime.Compare(answer1, DateTime.Now);

            if (DateTime.Compare(DateTime.Now, answer1) < 0)
                //   relationship = "is earlier than 08:00" gør ingenting
                notifyIcon1.ShowBalloonTip(2000, "", "Det er for tidligt at sende SMSer", ToolTipIcon.Info);

            else if (DateTime.Compare(DateTime.Now, answer2) < 0)
            //   relationship = "is earlier than 23:00";
            {
                try
                {
                    dataBaseConnection.Open();
                    _ErrorCount = 0;
                }
                catch (Exception ex)
                {
                    balloonTipDBError(ex.ToString());

                    _ErrorCount = _ErrorCount + 1;
                    LogException(ex);
                    return;
                }

                try
                {
                    string query = "SELECT * FROM SMS WHERE (SMS_SENT_TIMESTAMP is null) AND (MESSAGE>'" + A + "') AND (SMS_TO_BE_SENT_TIMESTAMP='" + String.Format("{0:yyyy/M/d}", DateTime.Now) + "')"; //Parametiseres


                    DataTable dataTableSmsContent = null;
                    using (FbCommand command = new FbCommand())
                    {
                        command.CommandText = "SELECT * FROM SMS WHERE (SMS_SENT_TIMESTAMP is null) AND (MESSAGE>'@A') AND (SMS_TO_BE_SENT_TIMESTAMP='" + String.Format("{0:yyyy/M/d}", DateTime.Now) + "')";
                        command.Parameters.AddWithValue("@A", A);
                        dataTableSmsContent = GetSmsContentFromDb(command);
                    }

                    int NumberOfSmsToSend = dataTableSmsContent.Rows.Count;

                    balloonTipSending(NumberOfSmsToSend.ToString());

                    if (NumberOfSmsToSend > 0)
                    {
                        List<SmsContent> smsListe = new List<SmsContent>();
                        foreach (DataRow rowSmsContent in dataTableSmsContent.Rows)
                        {
                            SmsContent smsTilListe = new SmsContent
                            {
                                AfsenderNavn = "Klinikken",
                                Besked = rowSmsContent["MESSAGE"].ToString(),
                                Landekode = rowSmsContent["LANDEKODE"].ToString(),
                                Telefonnummer = rowSmsContent["SMS_CAPABLE_PHONENUMBER"].ToString(),
                                ID = rowSmsContent["ID"].ToString()
                            };
                            smsListe.Add(smsTilListe);
                        }

                        foreach (SmsContent newSms in smsListe)
                        {
                            SendAndLogSms(newSms);
                        }
                    }
                }
                catch (Exception ex)
                {
                    balloonTipDBError(ex.ToString());
                }
                finally
                {
                    dataBaseConnection.Close();
                }
            }

            else
            {
                notifyIcon1.ShowBalloonTip(2000, "", "Det er for sent at sende SMSer", ToolTipIcon.Info);
            }
            _timer.Start();
        }

        private void LogException(Exception ex)
        {
            if (_ErrorCount >= 5)
            {
                try
                {
                    Mailer.SendMail("kj@averlauritzen.dk", "Fejl i SMS program", ex.ToString());
                    logger.writeToLogAlways(ex.ToString());
                }
                catch
                {
                    logger.writeToLogAlways("Fejl i afsendelse af mail");
                    logger.writeToLogAlways(ex.ToString());
                }
            }
        }

        private DataTable GetSmsContentFromDb(FbCommand query)
        {
            query.Connection = dataBaseConnection;
            DataTable dt = new DataTable();
            using (FbDataAdapter myAdapter = new FbDataAdapter(query))
            {
                myAdapter.Fill(dt);
            }
            return dt;
        }

        private void SendAndLogSms(SmsContent nysms)
        {
            using (GatewayV2 smsGate = new GatewayV2())
            {
                string modtager = "";
                if (nysms.Landekode.Length > 0)
                {
                    modtager = nysms.Landekode + nysms.Telefonnummer;
                }
                else
                {
                    modtager = "45" + nysms.Telefonnummer;
                }



                SmsReciept reciept = smsGate.SendThisSms(nysms.AfsenderNavn, nysms.Besked, modtager);
                string gatewayid = reciept.SmsIDNumbers[0] == null ? "" : reciept.SmsIDNumbers[0];
                logger.writeToLogAlways("Besked sendt til " + modtager + " GatewayID " + gatewayid);

                string input = DateTime.Today.Date.ToString();
                string date = input.Substring(0, input.IndexOf(" "));

                using (FbCommand updateCommand = new FbCommand())
                {

                    updateCommand.CommandText = "UPDATE SMS SET SMS_SENT_TIMESTAMP=@TIMESTAMP, GATEWAYID= @GATEWAYID, VALUTA = @VALUTA, PRIS = @PRIS WHERE SMS_CAPABLE_PHONENUMBER=@NUMBER AND SMS_TO_BE_SENT_TIMESTAMP=@DATE AND SMS_SENT_TIMESTAMP is NULL AND ID = @ID;"; //

                    updateCommand.Parameters.AddWithValue("@TIMESTAMP", DateTime.Now);
                    updateCommand.Parameters.AddWithValue("@GATEWAYID", gatewayid);
                    updateCommand.Parameters.AddWithValue("@VALUTA", reciept.Valuta);
                    updateCommand.Parameters.AddWithValue("@PRIS", reciept.Pris.Replace(',', '.'));
                    updateCommand.Parameters.AddWithValue("@NUMBER", nysms.Telefonnummer);
                    updateCommand.Parameters.AddWithValue("@DATE", date);
                    updateCommand.Parameters.AddWithValue("@ID", nysms.ID);
                    updateCommand.Connection = dataBaseConnection;

                    updateCommand.ExecuteNonQuery();
                }
            }
        }



        private void Form1_Resize(object sender, System.EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                Hide();
            }
        }

        private void lukProgrammet_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void sendNuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sendAllSMS();
        }
    }
}
