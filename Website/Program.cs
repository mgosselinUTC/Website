using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Website
{
    class Program
    {
        static void Main(string[] args)
        {

            
            TcpListener listener = new TcpListener(IPAddress.Any, 80);
            listener.Start();

            while (true)
            {
                Console.WriteLine("Listening for connectionsa on port 80...");
                try{
                    var client = listener.AcceptTcpClient();
                    Console.WriteLine("Incoming connection from " + ((IPEndPoint)client.Client.RemoteEndPoint).Address);
                    Stream stream = client.GetStream();

                    StreamReader reader = new StreamReader(stream);

                    string line;
                    int lineCount = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts;
                        if (lineCount == 0) {
                            parts = line.Split(' ');
                            if (parts.Length != 3)
                            {
                                throw new InvalidCastException();
                            }
                        }
                        lineCount ++;
                    }


                }catch(Exception e){
                    
                }
            }
        }
    }

    class InvalidHTTPRequestException : Exception
    {

    }
}
