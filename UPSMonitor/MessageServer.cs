﻿using Microsoft.Toolkit.Uwp.Notifications;
using System.IO.Pipes;
using System.Text;

namespace UPSMonitor
{
    internal static class MessageServer
    {
        public static async Task RunServer(CancellationToken cancellationToken)
        {
            try
            {
                while(!cancellationToken.IsCancellationRequested)
                {
                    using var server = new NamedPipeServerStream(Program.PipeServerName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    // wait for a connection
                    await server.WaitForConnectionAsync(cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    // client has connected, process the message
                    var message = await ReadString(server);
                    var received = false;
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        received = true;

                        // look for and strip the no-popup / log-only control code
                        var noPopUp = message.StartsWith(Program.NoPopupPrefix);
                        if (noPopUp) message = message.Substring(1);

                        // store the raw message to history
                        var storedMessage = new Message()
                        {
                            Content = message
                        };
                        Program.MessageHistory.Enqueue(storedMessage);

                        // display the pop-up
                        if(!noPopUp)
                        {
                            var sep = message.IndexOf(Program.TitleSeparator);
                            if(sep == -1)
                            {
                                new ToastContentBuilder()
                                    .AddText("Message")
                                    .AddText(message)
                                    .Show();
                            }
                            else
                            {
                                var title = message.Substring(0, sep);
                                var detail = message.Substring(sep + 1);
                                new ToastContentBuilder()
                                    .AddText(title)
                                    .AddText(detail)
                                    .Show();
                            }

                        }
                    }

                    // disconnect from the client
                    try
                    {
                        if (server.IsConnected)
                            server.Disconnect();
                    }
                    catch
                    { }

                    // persist message history
                    if (received) MessageStorage.WriteHistory();
                        
                }
            }
            catch (OperationCanceledException) // normal, disregard
            { }
            catch
            { }
            finally
            {
                // Remove any pop-up or Activity Center notifications, otherwise clicking one would re-start the program
                ToastNotificationManagerCompat.Uninstall();

                // server terminated
            }
        }

        private static async Task<string> ReadString(PipeStream stream)
        {
            string response = string.Empty;

            try
            {
                using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

                var size = reader.ReadInt32();
                if (size > 0)
                {
                    var buffer = reader.ReadBytes(size);
                    response = Encoding.ASCII.GetString(buffer);
                }

                await stream.FlushAsync();
            }
            catch
            { }

            return response;
        }
    }
}
