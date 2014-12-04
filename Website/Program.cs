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

            /*
            TcpListener listener = new TcpListener(IPAddress.Any, 80);
            listener.Start();

            while (true)
            {

                var client = listener.AcceptTcpClient();
                Console.WriteLine("Client Connected!");

                var stream = client.GetStream();

                if (stream.CanRead)
                {
                    byte[] buffer = new byte[8192];
                    stream.Read(buffer, 0, 8192);
                    string request = Encoding.ASCII.GetString(buffer);

                    //because trimming doesn't trim \0 characters.
                    request = request.Substring(0, request.IndexOf('\0'));

                    string[] parts = request.Split('\n');

                    foreach (string str in parts)
                    {
                        Console.WriteLine(str);
                        //Console.WriteLine();
                    }


                    if (stream.CanWrite)
                    {
                        string path = "C:\\mgosselin\\Desktop" + parts[0].Split(' ')[1].Replace("/", "\\");
                        if (!File.Exists(path))
                            path += "index.html";
                        if (!File.Exists(path))
                        {
                            byte[] notfound = Encoding.ASCII.GetBytes("<html>404 not found</html>");
                            stream.Write(notfound, 0, notfound.Length);
                        }
                        else
                        {
                            byte[] response = File.ReadAllBytes(path);
                            stream.Write(response, 0, response.Length);
                        }
                    }

                }

            
            }
            */


            TcpClient client = new TcpClient("google.com.", 80);
            client.Connect("http://www.google.com/", 80);
            var stream = client.GetStream();
            byte[] send = Encoding.ASCII.GetBytes("GET / HTTP/1.1\nHost: google.com\nConnection: keep-alive\nCache-Control: max-age=0\nAccept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8\nUser-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, Like Gecko) Chrome/40.0.2214.28 Safari/537.36\nAccept-Encoding: gzip, deflate, sdcd\nAcceptLanguage: en-US,en;q=0.8\n");
            stream.Write(send, 0, send.Length);

            Thread.Sleep(500);



        }
    }
}
