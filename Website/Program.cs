using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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


        }
    }
}
