using System.Net;
using System.Net.Mail;

namespace Banking_System.Communication.Email;

public static class EmailSender
{
    public static void SendEmail(string to, string? subject, string body)
    {
        const string senderEmail = "gsbank308@gmail.com";
        
        var client = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            EnableSsl = true,
            Credentials = new NetworkCredential(senderEmail, "onxe hqki aawe zjdv")
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        
        mailMessage.To.Add(to);
        client.Send(mailMessage);
    }
}