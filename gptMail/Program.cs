using System;
using System.Net.Http.Headers;
using System.Net.Http;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using OpenAI_API;
using System.Net.Http.Json;
using OpenAI_API.Models;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using MailKit.Net.Smtp;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace EmailServer
{
    public struct MailChain
    {
        public string name;
        public string title;
        public MailChain(string name, string title)
        {
            this.name = name;
            this.title = title;
        }
    }

    class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        static public string PrePrompt = "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly.\n\nHuman: Hello, who are you?\nAI: I am an AI created by OpenAI. How can I help you today?\nHuman: ";

        static async Task Main(string[] args)
        {
            var openaiApiKey = Configuration["OpenAI:ApiKey"];
            var emailAddress = Configuration["Email:Address"];
            var emailPassword = Configuration["Email:Password"];

            var gptHandler = new GptHandler(openaiApiKey);
            var emailHandler = new EmailHandler(emailAddress, emailPassword, gptHandler);

            await emailHandler.ConnectAndIdleAsync();
        }
    }





}