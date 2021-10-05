using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace MailClient
{
    public partial class Main : Form
    {
        string serverName { get; set; }
        int serverPort { get; set; }
        string account { get; set; }
        string password { get; set; }
        bool useSSL { get; set; }
        public Main()
        {
            serverName = ConfigurationManager.AppSettings["SMTPName"].ToString();
            serverPort = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
            account = ConfigurationManager.AppSettings["SMTPAccount"].ToString();
            password = ConfigurationManager.AppSettings["SMTPPassword"].ToString();
            useSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["SMTPUseSSL"]);
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            txFrom.Text = account;
            txFrom.Enabled = false;
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            lblLoader.Visible = true;
            try
            {
                string response = string.Empty;
                string to = txTo.Text.Trim();
                string subject = txSubject.Text.Trim();
                string body = txBody.Text;
                await Task.Factory.StartNew(async() =>
                {
                    response = await SendMessage(to, subject, body);
                }).Unwrap();

                if (response.Contains("OK"))
                    ClearInputs();
                MessageBox.Show(response);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                lblLoader.Visible = false;
            }
        }

        private async Task<string> SendMessage(string to, string subject, string body)
        {
            string response = string.Empty;
            try
            {
                MailMessage message = new MailMessage();

                string[] accounts = to.Split(';');
                message.Headers.Add("X-SES-CONFIGURATION-SET", "ConfigSet");
                message.IsBodyHtml = true;
                foreach (string item in accounts)
                    message.To.Add(new MailAddress(item));

                message.From = new MailAddress(account);
                message.Subject = subject;
                message.Body = 
                    "<h1>" + subject + "</h1>" +
                    "<p> " + body + "</p>";
                SmtpClient server = new SmtpClient(serverName, serverPort);
                server.Credentials = new NetworkCredential(account, password);
                server.EnableSsl = useSSL;

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
                server.Send(message);
                response = "OK-Mensaje Enviado";
            }
            catch (System.Net.Mail.SmtpException x) { response = String.Format("sendCustomMail > SmtpException [{0}] \n\n ExceptionDescription -> {1}", x.Message, x.StackTrace); }
            catch (Exception ex) { response = String.Format("sendCustomMail > SystemException [{0}] \n\n ExceptionDescription -> {1}", ex.Message, ex.StackTrace); }
            return response;
        }

        private void ClearInputs()
        {
            try
            {
                txTo.Text = string.Empty;
                txSubject.Text = string.Empty;
                txBody.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
