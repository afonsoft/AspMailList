using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenPop.Common.Logging;
using OpenPop.Mime;
using OpenPop.Mime.Decode;
using OpenPop.Mime.Header;
using OpenPop.Pop3;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace AspMailList.library
{
    public class Pop3
    {
        /// <summary>
        /// Example showing:
        ///  - how to fetch all messages from a POP3 server
        /// </summary>
        /// <param name="hostname">Hostname of the server. For example: pop3.live.com</param>
        /// <param name="port">Host port to connect to. Normally: 110 for plain POP3, 995 for SSL POP3</param>
        /// <param name="useSsl">Whether or not to use SSL to connect to server</param>
        /// <param name="username">Username of the user on the server</param>
        /// <param name="password">Password of the user on the server</param>
        /// <returns>All Messages on the POP3 server</returns>
        public List<Message> FetchAllMessages(string hostname, int port, bool useSsl, string username, string password)
        {
            // The client disconnects from the server when being disposed
            using (Pop3Client client = new Pop3Client())
            {
                // Connect to the server
                client.Connect(hostname, port, useSsl);

                // Authenticate ourselves towards the server
                client.Authenticate(username, password);

                // Get the number of messages in the inbox
                int messageCount = client.GetMessageCount();

                // We want to download all messages
                List<Message> allMessages = new List<Message>(messageCount);

                // Messages are numbered in the interval: [1, messageCount]
                // Ergo: message numbers are 1-based.
                // Most servers give the latest message the highest number
                for (int i = messageCount; i > 0; i--)
                {
                    allMessages.Add(client.GetMessage(i));
                }

                // Now return the fetched messages
                return allMessages;
            }
        }

        /// <summary>
        /// Example showing:
        ///     Connect 
        /// </summary>
        /// <param name="hostname">Hostname of the server. For example: pop3.live.com</param>
        /// <param name="port">Host port to connect to. Normally: 110 for plain POP3, 995 for SSL POP3</param>
        /// <param name="useSsl">Whether or not to use SSL to connect to server</param>
        /// <param name="username">Username of the user on the server</param>
        /// <param name="password">Password of the user on the server</param>
        /// <returns>Pop3Client object</returns>
        public Pop3Client pop3Client(string hostname, int port, bool useSsl, string username, string password)
        {
            Pop3Client client = new Pop3Client();
            // Connect to the server
            client.Connect(hostname, port, useSsl, 120000, 120000, null);
            // Authenticate ourselves towards the server
            client.Authenticate(username, password, AuthenticationMethod.Auto);
            return client;
        }

        public List<Message> FetchAllMessages(Pop3Client client)
        {
            // The client disconnects from the server when being disposed
           // Get the number of messages in the inbox
            int messageCount = client.GetMessageCount();

            // We want to download all messages
            List<Message> allMessages = new List<Message>(messageCount);

            // Messages are numbered in the interval: [1, messageCount]
            // Ergo: message numbers are 1-based.
            // Most servers give the latest message the highest number
            for (int i = messageCount; i > 0; i--)
            {
                allMessages.Add(client.GetMessage(i));
            }

            // Now return the fetched messages
            return allMessages;

        }

        /// <summary>
        /// Example showing:
        ///  - how to delete fetch an emails headers only
        ///  - how to delete a message from the server
        /// </summary>
        /// <param name="client">A connected and authenticated Pop3Client from which to delete a message</param>
        /// <param name="messageId">A message ID of a message on the POP3 server. Is located in <see cref="MessageHeader.MessageId"/></param>
        /// <returns><see langword="true"/> if message was deleted, <see langword="false"/> otherwise</returns>
        public bool DeleteMessageByMessageId(Pop3Client client, string messageId)
        {
            // Get the number of messages on the POP3 server
            int messageCount = client.GetMessageCount();

            // Run trough each of these messages and download the headers
            for (int messageItem = messageCount; messageItem > 0; messageItem--)
            {
                // If the Message ID of the current message is the same as the parameter given, delete that message
                if (client.GetMessageHeaders(messageItem).MessageId == messageId)
                {
                    // Delete
                    client.DeleteMessage(messageItem);
                    return true;
                }
            }

            // We did not find any message with the given messageId, report this back
            return false;
        }

        /// <summary>
        /// Example showing:
        ///  - how to a find plain text version in a Message
        ///  - how to save MessageParts to file
        /// </summary>
        /// <param name="message">The message to examine for plain text</param>
        public MessagePart FindPlainTextInMessage(Message message)
        {
            return message.FindFirstPlainTextVersion();
        }

        /// <summary>
        /// Example showing:
        ///  - how to find a html version in a Message
        ///  - how to save MessageParts to file
        /// </summary>
        /// <param name="message">The message to examine for html</param>
        public MessagePart FindHtmlInMessage(Message message)
        {
            return message.FindFirstHtmlVersion();
        }

        /// <summary>
        /// Example showing:
        ///  - how to use UID's (unique ID's) of messages from the POP3 server
        ///  - how to download messages not seen before
        ///    (notice that the POP3 protocol cannot see if a message has been read on the server
        ///     before. Therefore the client need to maintain this state for itself)
        /// </summary>
        /// <param name="hostname">Hostname of the server. For example: pop3.live.com</param>
        /// <param name="port">Host port to connect to. Normally: 110 for plain POP3, 995 for SSL POP3</param>
        /// <param name="useSsl">Whether or not to use SSL to connect to server</param>
        /// <param name="username">Username of the user on the server</param>
        /// <param name="password">Password of the user on the server</param>
        /// <param name="seenUids">
        /// List of UID's of all messages seen before.
        /// New message UID's will be added to the list.
        /// Consider using a HashSet if you are using >= 3.5 .NET
        /// </param>
        /// <returns>A List of new Messages on the server</returns>
        public List<Message> FetchUnseenMessages(string hostname, int port, bool useSsl, string username, string password, List<string> seenUids)
        {
            // The client disconnects from the server when being disposed
            using (Pop3Client client = new Pop3Client())
            {
                // Connect to the server
                client.Connect(hostname, port, useSsl);

                // Authenticate ourselves towards the server
                client.Authenticate(username, password);

                // Fetch all the current uids seen
                List<string> uids = client.GetMessageUids();

                // Create a list we can return with all new messages
                List<Message> newMessages = new List<Message>();

                // All the new messages not seen by the POP3 client
                for (int i = 0; i < uids.Count; i++)
                {
                    string currentUidOnServer = uids[i];
                    if (!seenUids.Contains(currentUidOnServer))
                    {
                        // We have not seen this message before.
                        // Download it and add this new uid to seen uids

                        // the uids list is in messageNumber order - meaning that the first
                        // uid in the list has messageNumber of 1, and the second has 
                        // messageNumber 2. Therefore we can fetch the message using
                        // i + 1 since messageNumber should be in range [1, messageCount]
                        Message unseenMessage = client.GetMessage(i + 1);

                        // Add the message to the new messages
                        newMessages.Add(unseenMessage);

                        // Add the uid to the seen uids, as it has now been seen
                        seenUids.Add(currentUidOnServer);
                    }
                }

                // Return our new found messages
                return newMessages;
            }
        }

        /// <summary>
        /// Example showing:
        ///  - how to use UID's (unique ID's) of messages from the POP3 server
        ///  - how to download messages not seen before
        ///    (notice that the POP3 protocol cannot see if a message has been read on the server
        ///     before. Therefore the client need to maintain this state for itself)
        /// </summary>
        /// <param name="client">A connected and authenticated Pop3Client from which to delete a message</param>
        /// <param name="seenUids">
        /// List of UID's of all messages seen before.
        /// New message UID's will be added to the list.
        /// Consider using a HashSet if you are using >= 3.5 .NET
        /// </param>
        /// <returns>A List of new Messages on the server</returns>
        public List<Message> FetchUnseenMessages(Pop3Client client, List<string> seenUids)
        {

            // Fetch all the current uids seen
            List<string> uids = client.GetMessageUids();

            // Create a list we can return with all new messages
            List<Message> newMessages = new List<Message>();

            // All the new messages not seen by the POP3 client
            for (int i = 0; i < uids.Count; i++)
            {
                string currentUidOnServer = uids[i];
                if (!seenUids.Contains(currentUidOnServer))
                {
                    // We have not seen this message before.
                    // Download it and add this new uid to seen uids

                    // the uids list is in messageNumber order - meaning that the first
                    // uid in the list has messageNumber of 1, and the second has 
                    // messageNumber 2. Therefore we can fetch the message using
                    // i + 1 since messageNumber should be in range [1, messageCount]
                    Message unseenMessage = client.GetMessage(i + 1);

                    // Add the message to the new messages
                    newMessages.Add(unseenMessage);

                    // Add the uid to the seen uids, as it has now been seen
                    seenUids.Add(currentUidOnServer);
                }
            }
            // Return our new found messages
            return newMessages;
        }

        /// <summary>
        /// Example showing:
        ///  - how to set timeouts
        ///  - how to override the SSL certificate checks with your own implementation
        /// </summary>
        /// <param name="hostname">Hostname of the server. For example: pop3.live.com</param>
        /// <param name="port">Host port to connect to. Normally: 110 for plain POP3, 995 for SSL POP3</param>
        /// <param name="timeouts">Read and write timeouts used by the Pop3Client</param>
        public void BypassSslCertificateCheck(string hostname, int port, int timeouts)
        {
            // The client disconnects from the server when being disposed
            using (Pop3Client client = new Pop3Client())
            {
                // Connect to the server using SSL with specified settings
                // true here denotes that we connect using SSL
                // The certificateValidator can validate the SSL certificate of the server.
                // This might be needed if the server is using a custom normally untrusted certificate
                client.Connect(hostname, port, true, timeouts, timeouts, certificateValidator);

                // Do something extra now that we are connected to the server
            }
        }
        private static bool certificateValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            // We should check if there are some SSLPolicyErrors, but here we simply say that
            // the certificate is okay - we trust it.
            return true;
        }
    }
}
