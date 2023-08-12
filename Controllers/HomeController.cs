using GmailOAuth.Models;
using GmailOAuth.Service;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GmailOAuth.Controllers
{
    public class HomeController : Controller
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/gmail-dotnet-quickstart.json
        private static readonly string[] Scopes = { GmailService.Scope.GmailReadonly };
        private static readonly string ApplicationName = "Gmail API .NET Quickstart";

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string username)
        {
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                 new ClientSecrets() { ClientId = "312582055255-9da1thomnpe5q11c2vcp8tjaqlhq4hnv.apps.googleusercontent.com", ClientSecret = "Ai3IVqqYnJNgVL4vbYKR5irB" },
                    Scopes,
                    "user",
                    CancellationToken.None, null).Result;
            // Create Gmail API service.
            GmailService service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            var inboxlistRequest = service.Users.Messages.List("me");
            inboxlistRequest.LabelIds = "INBOX";
            inboxlistRequest.IncludeSpamTrash = false;
            List<GmailMessages> modelList = new List<GmailMessages>();
            // List labels.
            var labels = inboxlistRequest.Execute().Messages;
            if (labels != null && labels.Count > 0)
            {
                foreach (var labelItem in labels)
                {
                    var emailInfoRequest = service.Users.Messages.Get("ritesh.digisoft@gmail.com", labelItem.Id);
                    var emailInfoResponse = emailInfoRequest.Execute();
                    if (emailInfoResponse != null)
                    {
                        string from = "";
                        string date = "";
                        string subject = "";
                        string body = "";
                        //loop through the headers to get from,date,subject, body  
                        try
                        {
                            foreach (var mParts in emailInfoResponse.Payload.Headers)
                            {
                                if (mParts.Name == "Date")
                                {
                                    date = mParts.Value;
                                }
                                else if (mParts.Name == "From")
                                {
                                    from = mParts.Value;
                                }
                                else if (mParts.Name == "Subject")
                                {
                                    subject = mParts.Value;
                                }
                                if (date != "" && from != "")
                                {
                                    foreach (MessagePart p in emailInfoResponse.Payload.Parts)
                                    {
                                        if (p.MimeType == "text/html")
                                        {
                                            byte[] data = FromBase64ForUrlString(p.Body.Data);
                                            body = Encoding.UTF8.GetString(data);
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }

                        modelList.Add(new GmailMessages { Subject = subject, From = from, Body = body });
                    }

                }
            }

            return View(modelList);
        }
        public static byte[] FromBase64ForUrlString(string base64ForUrlInput)
        {
            int padChars = (base64ForUrlInput.Length % 4) == 0 ? 0 : (4 - (base64ForUrlInput.Length % 4));
            StringBuilder result = new StringBuilder(base64ForUrlInput, base64ForUrlInput.Length + padChars);
            result.Append(String.Empty.PadRight(padChars, '='));
            result.Replace('-', '+');
            result.Replace('_', '/');
            return Convert.FromBase64String(result.ToString());
        }
        public ActionResult About()
        {

            return View();
        }
        public async Task<ActionResult> IndexAsync(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).
                AuthorizeAsync(cancellationToken);

            if (result.Credential != null)
            {
                var service = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "ASP.NET MVC Sample"
                });

                // YOUR CODE SHOULD BE HERE..
                // SAMPLE CODE:
                var list = await service.Users.Messages.List("me").ExecuteAsync();
                List<GmailMessages> modelList = new List<GmailMessages>();
                // List labels.
                var labels = list.Messages;
                if (labels != null && labels.Count > 0)
                {
                    foreach (var labelItem in labels.Take(50))
                    {
                        var emailInfoRequest = service.Users.Messages.Get("me", labelItem.Id);
                        var emailInfoResponse = emailInfoRequest.Execute();
                        if (emailInfoResponse != null)
                        {
                            string from = "";
                            string date = "";
                            string subject = "";
                            string body = "";
                            //loop through the headers to get from,date,subject, body  
                            try
                            {
                                foreach (var mParts in emailInfoResponse.Payload.Headers)
                                {
                                    if (mParts.Name == "Date")
                                    {
                                        date = mParts.Value;
                                    }
                                    else if (mParts.Name == "From")
                                    {
                                        from = mParts.Value;
                                    }
                                    else if (mParts.Name == "Subject")
                                    {
                                        subject = mParts.Value;
                                    }
                                    if (date != "" && from != "")
                                    {
                                        foreach (MessagePart p in emailInfoResponse.Payload.Parts)
                                        {
                                            if (p.MimeType == "text/html")
                                            {
                                                byte[] data = FromBase64ForUrlString(p.Body.Data);
                                                body = Encoding.UTF8.GetString(data);
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                            }

                            modelList.Add(new GmailMessages { Subject = subject, From = from, Body = body });
                        }

                    }
                }

                return View("About", modelList);
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
