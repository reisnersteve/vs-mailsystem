using System;

namespace mailsystem.src
{
    public class Mail
    {
        public string Sender;
        public string Receiver;
        public string Message;
        public bool IsRead;
        public DateTime SentDate;

        public Mail(string sender, string receiver, string message, DateTime sentDate, bool read = false)
        {
            Sender = sender;
            Receiver = receiver;
            Message = message;
            IsRead = read;
            SentDate = sentDate;
            
        }
    }
}
