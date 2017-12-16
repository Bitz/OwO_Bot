using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using SysTimer = System.Timers;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using OwO_Bot.Functions.DAL;
using OwO_Bot.Models;
using static OwO_Bot.Constants;

namespace OwO_Bot.Functions
{
    class Mail
    {

        public bool Send(string subject, string messageContent)
        {
            C.WriteNoTime("Requesting title via email...");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("OwO Bot", Config.mail.username));
            message.To.Add(new MailboxAddress(Config.mail.to));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = messageContent
            };


            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(Config.mail.outgoing_server, Config.mail.outgoing_port, true);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(Config.mail.username, Config.mail.password);
                client.Send(message);
                client.Disconnect(true);
            }
            return true;
        }

        public string Recieve(string search, long postId)
        {
            string result = "";
            using (var client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(Config.mail.incoming_server, Config.mail.incoming_port, true);

                // Note: since we don't have an OAuth2 token, disable
                client.Authenticate(Config.mail.username, Config.mail.password);
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                // The Inbox folder is always available on all IMAP servers...
                IMailFolder inbox = client.Inbox;

                inbox.Open(FolderAccess.ReadWrite);

                UniqueId nextUid = new UniqueId();
                if (inbox.UidNext != null)
                {
                    nextUid = inbox.UidNext.Value;
                }
                else
                {
                    C.WriteLine("Could not connect to email.");
                    Environment.Exit(0);
                }

                var query = SearchQuery.NotSeen
                    .And(SearchQuery.SubjectContains(search))
                    .And(SearchQuery.FromContains(Config.mail.to.ToLower()));

                CancellationTokenSource done = new CancellationTokenSource();
                var thread = new Thread(IdleLoop);
                
                client.Inbox.MessagesArrived += (sender, e) =>
                {
                    C.WriteLineNoTime("You got mail!");
                    done.Cancel();
                };
                //1 Hour and 30 mins.
                var timer = new SysTimer.Timer {Interval = 5400000 };
                timer.Elapsed += (sender, e) =>
                {
                    C.WriteLine("Time passed with no reply...");
                    Send(search, "Request Cancelled");
                    C.WriteLine("We won't be proceeding with this post...");

                    Environment.Exit(0);
                    done.Cancel();
                };
                timer.Start();

                thread.Start(new IdleState(client, done.Token));
                thread.Join();
                done.Dispose();
                    
                var range = new UniqueIdRange(nextUid, UniqueId.MaxValue);

                foreach (UniqueId uid in inbox.Search(range,query))
                {
                    var message = inbox.GetMessage(uid);
                    result = message.TextBody.Split('\r', '\n').FirstOrDefault();
                    inbox.AddFlags(uid, MessageFlags.Deleted, true);

                    if (result != null && result.ToLower().ToLower() == "cancel")
                    {
                        C.WriteLine("We won't be proceeding with this post...");
                        DbBlackList dbBlackList = new DbBlackList();
                        Blacklist item = new Blacklist
                        {
                            Subreddit = WorkingSub,
                            PostId = postId,
                            CreatedDate = DateTime.Now
                        };

                        dbBlackList.AddToBlacklist(item);
                        Environment.Exit(0);
                    } else if (result != null && result.ToLower().ToLower() == "next")
                    {
                        C.WriteLine("We won't be proceeding with this post...");
                        DbBlackList dbBlackList = new DbBlackList();
                        Blacklist item = new Blacklist
                        {
                            Subreddit = WorkingSub,
                            PostId = postId,
                            CreatedDate = DateTime.Now
                        };

                        dbBlackList.AddToBlacklist(item);
                        Process.Start(Assembly.GetExecutingAssembly().Location, Args.FirstOrDefault());
                        Environment.Exit(0);
                    }
                    else
                    {
                        C.WriteLineNoTime("We got a title!");
                    }
                    break;
                }
                client.Disconnect(true);
            }
            return result;
        }
        

        //https://github.com/jstedfast/MailKit/blob/master/samples/ImapIdle/ImapIdle/Program.cs
        class IdleState
        {
            readonly object _mutex = new object();
            CancellationTokenSource _timeout;

            /// <summary>
            /// Get the cancellation token.
            /// </summary>
            /// <remarks>
            /// <para>The cancellation token is the brute-force approach to cancelling the IDLE and/or NOOP command.</para>
            /// <para>Using the cancellation token will typically drop the connection to the server and so should
            /// not be used unless the client is in the process of shutting down or otherwise needs to
            /// immediately abort communication with the server.</para>
            /// </remarks>
            /// <value>The cancellation token.</value>
            public CancellationToken CancellationToken { get; }

            /// <summary>
            /// Get the done token.
            /// </summary>
            /// <remarks>
            /// <para>The done token tells the <see cref="Program.IdleLoop"/> that the user has requested to end the loop.</para>
            /// <para>When the done token is cancelled, the <see cref="Program.IdleLoop"/> will gracefully come to an end by
            /// cancelling the timeout and then breaking out of the loop.</para>
            /// </remarks>
            /// <value>The done token.</value>
            public CancellationToken DoneToken { get; }

            /// <summary>
            /// Get the IMAP client.
            /// </summary>
            /// <value>The IMAP client.</value>
            public ImapClient Client { get; }

            /// <summary>
            /// Check whether or not either of the CancellationToken's have been cancelled.
            /// </summary>
            /// <value><c>true</c> if cancellation was requested; otherwise, <c>false</c>.</value>
            public bool IsCancellationRequested => CancellationToken.IsCancellationRequested || DoneToken.IsCancellationRequested;

            /// <summary>
            /// Initializes a new instance of the <see cref="IdleState"/> class.
            /// </summary>
            /// <param name="client">The IMAP client.</param>
            /// <param name="doneToken">The user-controlled 'done' token.</param>
            /// <param name="cancellationToken">The brute-force cancellation token.</param>
            public IdleState(ImapClient client, CancellationToken doneToken, CancellationToken cancellationToken = default(CancellationToken))
            {
                CancellationToken = cancellationToken;
                DoneToken = doneToken;
                Client = client;

                // When the user hits a key, end the current timeout as well
                doneToken.Register(CancelTimeout);
            }

            /// <summary>
            /// Cancel the timeout token source, forcing ImapClient.Idle() to gracefully exit.
            /// </summary>
            void CancelTimeout()
            {
                lock (_mutex)
                {
                    _timeout?.Cancel();
                }
            }

            /// <summary>
            /// Set the timeout source.
            /// </summary>
            /// <param name="source">The timeout source.</param>
            public void SetTimeoutSource(CancellationTokenSource source)
            {
                lock (_mutex)
                {
                    _timeout = source;

                    if (_timeout != null && IsCancellationRequested)
                        _timeout.Cancel();
                }
            }
        }

        static void IdleLoop(object state)
        {
            var idle = (IdleState)state;

            lock (idle.Client.SyncRoot)
            {
                // Note: since the IMAP server will drop the connection after 30 minutes, we must loop sending IDLE commands that
                // last ~29 minutes or until the user has requested that they do not want to IDLE anymore.
                // 
                // For GMail, we use a 9 minute interval because they do not seem to keep the connection alive for more than ~10 minutes.
                while (!idle.IsCancellationRequested)
                {
                    using (var timeout = new CancellationTokenSource(new TimeSpan(0, 9, 0)))
                    {
                        try
                        {
                            // We set the timeout source so that if the idle.DoneToken is cancelled, it can cancel the timeout
                            idle.SetTimeoutSource(timeout);

                            if (idle.Client.Capabilities.HasFlag(ImapCapabilities.Idle))
                            {
                                // The Idle() method will not return until the timeout has elapsed or idle.CancellationToken is cancelled
                                idle.Client.Idle(timeout.Token, idle.CancellationToken);
                            }
                            else
                            {
                                // The IMAP server does not support IDLE, so send a NOOP command instead
                                idle.Client.NoOp(idle.CancellationToken);

                                // Wait for the timeout to elapse or the cancellation token to be cancelled
                                WaitHandle.WaitAny(new[] { timeout.Token.WaitHandle, idle.CancellationToken.WaitHandle });
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // This means that idle.CancellationToken was cancelled, not the DoneToken nor the timeout.
                            break;
                        }
                        catch (ImapProtocolException)
                        {
                            // The IMAP server sent garbage in a response and the ImapClient was unable to deal with it.
                            // This should never happen in practice, but it's probably still a good idea to handle it.
                            // 
                            // Note: an ImapProtocolException almost always results in the ImapClient getting disconnected.
                            break;
                        }
                        catch (ImapCommandException)
                        {
                            // The IMAP server responded with "NO" or "BAD" to either the IDLE command or the NOOP command.
                            // This should never happen... but again, we're catching it for the sake of completeness.
                            break;
                        }
                        finally
                        {
                            // We're about to Dispose() the timeout source, so set it to null.
                            idle.SetTimeoutSource(null);
                        }
                    }
                }
            }
        }
    }
}
