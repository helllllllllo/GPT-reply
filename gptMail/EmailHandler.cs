using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

public class EmailHandler
{
    private readonly string _emailAddress;
    private readonly string _emailPassword;
    private readonly GptHandler _gptHandler;

    public EmailHandler(string emailAddress, string emailPassword, GptHandler gptHandler)
    {
        _emailAddress = emailAddress;
        _emailPassword = emailPassword;
        _gptHandler = gptHandler;
    }

    public async Task ConnectAndIdleAsync()
    {
        while (true)
        {
            try
            {
                using (var emailClientIdel = new ImapClient())
                {
                    emailClientIdel.Connect("imap.gmail.com", 993, true);
                    emailClientIdel.Authenticate(_emailAddress, _emailPassword);
                    emailClientIdel.Inbox.Open(FolderAccess.ReadOnly);
                    emailClientIdel.Inbox.CountChanged += async (sender, e) => await HandleNewEmailAsync();

                    var cts = new CancellationTokenSource();
                    await emailClientIdel.IdleAsync(cts.Token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disconnected from the email server. Reason: {ex.Message}");
                Console.WriteLine("Reconnecting in 30 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }
    }

    private async Task HandleNewEmailAsync()
    {
        using (var emailClient = new ImapClient())
        {
            emailClient.Connect("imap.gmail.com", 993, true);
            emailClient.Authenticate(_emailAddress, _emailPassword);

            emailClient.Inbox.Open(FolderAccess.ReadWrite);

            // Get the latest email received
            var uids = emailClient.Inbox.Search(SearchQuery.All);
            var message = emailClient.Inbox.GetMessage(uids[uids.Count - 1]);

            // Process the message and generate a reply using GPT
            string reply = await _gptHandler.GenerateReply(message);

            // Send the reply
            await SendReplyAsync(message, reply);
        }
    }

    private async Task SendReplyAsync(MimeMessage originalMessage, string reply)
    {
        var replyMessage = new MimeMessage();
        replyMessage.From.Add(new MailboxAddress("GPT-3", _emailAddress));
        replyMessage.To.Add(originalMessage.From[0]);
        replyMessage.Subject = $"RE: {originalMessage.Subject}";
        replyMessage.Body = new TextPart("plain")
        {
            Text = reply
        };

        using (var client = new SmtpClient())
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate(_emailAddress, _emailPassword);

            await client.SendAsync(replyMessage);
            client.Disconnect(true);
        };
    }
}
