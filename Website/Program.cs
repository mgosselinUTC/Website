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
                    TcpClient client = listener.AcceptTcpClient();
                    string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    string port = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
                    Stream stream = client.GetStream();

                    Console.WriteLine("Incoming connection from " + ip + " on port " + port + "...");

                    StreamReader reader = new StreamReader(stream);

                    string line;
                    string method = "GET";
                    string fileRequestPath = "/";
                    string protocol = "unknown";

                    //split it up, if there's anything to split.
                    //this will glitch even if the header is there but on another line.
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        string[] parts;
                        parts = line.Split(' ');
                        if (parts.Length != 3)
                            throw new InvalidHTTPRequestException("Error parsing " + line);
                        method = parts[0];
                        fileRequestPath = parts[1];
                        protocol = parts[2];
                    }
                    else
                    {
                        throw new InvalidHTTPRequestException("Expected HTTP request declaration, got null");
                    }

                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    
                    //time to oop through our lines and look for headers
                    while ((line = reader.ReadLine()) != null)
                    {
                            try
                            {
                                // on the last line before the data body, this happens, the line is "" 
                                if (line != "")
                                {
                                    int colonIndex = line.IndexOf(':');
                                    string key = line.Substring(0, colonIndex);
                                    key = key.Trim();
                                    string value = line.Substring(colonIndex + 1, line.Length - colonIndex - 1);
                                    value = value.Trim();
                                    headers.Add(key, value);
                                }
                                    //so then we exit the lopo.
                                else break;
                            }
                            catch (Exception e)
                            {
                                throw new InvalidHTTPRequestException("Error parsing " + line);
                            }
                    }
                    //so if we haven't escaped yet, well, lets give them that file.
                    string ROOT = "C:\\Users\\mgosselin\\Desktop";
                    fileRequestPath = ROOT + fileRequestPath.Replace("/", "\\");

                    Console.WriteLine(ip + " is requesting " + fileRequestPath);

                    if (!File.Exists(fileRequestPath))
                    {

                        Console.WriteLine("" + fileRequestPath + " does not exist, appending index.html.");
                        fileRequestPath += "index.html";

                    }

                    if (File.Exists(fileRequestPath))
                    {
                        //it should be legit by now...
                        // FileStream outputFile = new FileStream(ROOT + "\\output\\" + DateTime.Now.Millisecond + ".txt", FileMode.Create);
                        StreamWriter writer = new StreamWriter(stream);
                        Console.WriteLine("Sending " + fileRequestPath + " to " + ip);

                        writer.WriteLine("HTTP/1.1 200 OK");
                        
                        writer.WriteLine("");
                        byte[] fileBuffer = File.ReadAllBytes(fileRequestPath);
                        writer.Write(Encoding.ASCII.GetString(fileBuffer));
                        writer.Close();
                    }
                    else
                    {
                        //return the 404 page
                    }

                    //stream.Close();

                }catch(InvalidHTTPRequestException e){
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }

    class InvalidHTTPRequestException : Exception
    {
        private string line;
        public InvalidHTTPRequestException(string line)
        {
            this.line = line;
        }

        public string ToString() {
            return line;
        }
    }
}
