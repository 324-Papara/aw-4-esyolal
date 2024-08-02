namespace Para.Bussiness.Email
{
    public class EmailMessage
    {
        public string Subject { get; set; }
        public string To { get; set; }
        public string Content { get; set; }

        public EmailMessage(string subject, string to, string content)
        {
            Subject = subject;
            To = to;
            Content = content;
        }
    }
}
