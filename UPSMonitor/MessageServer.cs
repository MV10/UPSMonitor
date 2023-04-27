using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
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
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        var messageLines = message.Split(Program.SeparatorControlCode);
                        if (messageLines.Length > 0)
                        {
                            // store the raw message to history
                            var storedMessage = new Message()
                            {
                                Content = message
                            };
                            Program.MessageHistory.Enqueue(storedMessage);

                            // display the pop-up
                            var toast = new ToastContentBuilder();
                            foreach (var line in messageLines)
                            {
                                toast.AddText(line);
                            }
                            toast.Show();
                        }
                    }

                    // disconnect from the client
                    try
                    {
                        if (server.IsConnected)
                            server.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        //Output(LogLevel.Warning, $"{ex.GetType().Name} while trying to disconnect from switch pipe client");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // normal, disregard
            }
            catch (Exception ex)
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
                using (var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true))
                {
                    var size = reader.ReadInt32();
                    if (size > 0)
                    {
                        var buffer = reader.ReadBytes(size);
                        response = Encoding.ASCII.GetString(buffer);
                    }
                }
                await stream.FlushAsync();
            }
            catch (Exception ex)
            { }

            return response;
        }
    }
}
