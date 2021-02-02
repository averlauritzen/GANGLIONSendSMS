using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace GANGLIONSendSMS
{
    class Mailer
    {

        public static void SendMail(string mailAdresse, string Emne, string Besked)
        {
            System.Net.Mail.MailMessage email = new System.Net.Mail.MailMessage();
            email.From = new System.Net.Mail.MailAddress("noreply@ganglion.dk", "Fejl i SMS afsendelse ");
            //email.To.Add("debug_NetklinikProxy@caresolutions.dk");
            MailAddress m = new MailAddress(mailAdresse);

            email.To.Add(m);
            email.Subject = Emne;
            email.Body = Besked;

            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("post.ganglion.dk");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            System.Net.NetworkCredential cred = new System.Net.NetworkCredential("noreply@ganglion.dk", "c62S45q4U5");
            smtp.Credentials = cred;
            smtp.EnableSsl = true;
            smtp.Port = 587;
            smtp.Send(email);
        }

    }
}
