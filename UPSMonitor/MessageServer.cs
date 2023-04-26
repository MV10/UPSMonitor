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
                Debug.WriteLine("Message server starting");
                while(!cancellationToken.IsCancellationRequested)
                {
                    using var server = new NamedPipeServerStream(Program.PipeServerName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    // wait for a connection
                    Debug.WriteLine("Message server waiting for connection");
                    await server.WaitForConnectionAsync(cancellationToken);
                    Debug.WriteLine($"Message server cancelled? {cancellationToken.IsCancellationRequested}");
                    cancellationToken.ThrowIfCancellationRequested();

                    // client has connected, process the message
                    Debug.WriteLine("Message server reading data");
                    var message = await ReadString(server);
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        var messageLines = message.Split(Program.SeparatorControlCode);
                        Debug.WriteLine($"Message server received {messageLines.Length} lines of text");
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
                        Debug.WriteLine("Message server disconnecting\n\n");
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
                Debug.WriteLine("Message server OperationCanceledException (expected)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Message server exception:\n{ex.Message}\n{ex.InnerException?.Message}");
            }
            finally
            {
                // server terminated
                Debug.WriteLine("Message server terminated");

                // Remove any pop-up or Activity Center notifications, otherwise clicking one would re-start the program
                ToastNotificationManagerCompat.Uninstall();
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
            {
                //Output(LogLevel.Warning, $"{ex.GetType().Name} while reading stream");
            }

            return response;
        }
    }
}
