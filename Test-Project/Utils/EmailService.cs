using System.Net;
using System.Net.Mail;

namespace TestProject.Utils
{
    public class EmailService
    {
        public void SendEmail(string reportPath)
        {
            MailMessage mail = new MailMessage();
            string UserEmail = "";
            string UserName = "apikey";
            string Password = "";
            string ToEmail = "";
            string Subject = "Execution Report✓";
            string contentBody = "<h3> Execution Summary from your last test execution </h3>";

            mail.From = new MailAddress(UserEmail);
            mail.To.Add(ToEmail);
            mail.Subject = Subject;
            mail.Body = contentBody;
            mail.IsBodyHtml = true;
            mail.Attachments.Add(new Attachment(reportPath));

            SmtpClient smtp = new SmtpClient("smtp.sendgrid.net", 587);
            smtp.Credentials = new NetworkCredential(UserName, Password);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }
    }
}