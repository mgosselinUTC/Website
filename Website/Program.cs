using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// google voice login:
// username: mikepreble@gmail.com
// password: sukirevomnrhqxxo


namespace Website
{
    class Program
    {
        static void Main(string[] args)
        {

            int port = 80;

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (true)
            {
                Console.WriteLine("Listening for connections on port " + port + "...");
                try
                {
                    TcpClient client = listener.AcceptTcpClient();

                    HTTPRequest request = new HTTPRequest(client);
                    request.respond();

                }
                catch (InvalidHTTPRequestException e)
                {
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

        public string ToString()
        {
            return line;
        }
    }

    public class HTTPRequest
    {


        //too lazy to create a bunch of getters, so i made everything public.
        //DONT SCREW WITH THESE VARIABLES
        private static volatile int requests = 0;
        public static string ROOT = "T:\\Website";//"C:\\Users\\mgosselin\\Desktop";

        public int ID = ++requests;

        public Stream stream;
        public TcpClient client;
        public bool broken = false;

        public string method = "GET"; // the method to be used.
        public string fileRequestPath = "/"; // the requested file or folder.
        public string protocolVersion = "unknown"; //protocol and version, like HTTP/1.1
        public string ip; //source ip
        public string port; // source port

        private StreamReader reader;

        Dictionary<string, string> headers = new Dictionary<string, string>();

        public HTTPRequest(TcpClient client)
        {

            this.client = client;
            stream = client.GetStream();

            ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            port = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();

            Console.WriteLine("Incoming connection from " + ip + " on port " + port + "...");
            Console.WriteLine();

            reader = new StreamReader(stream);
            // debug thing. output responses to the desktop.
            // FileStream outputFile = new FileStream(ROOT + "\\output\\" + DateTime.Now.Millisecond + ".txt", FileMode.Create);


            readProtocolInformation();

            readHeaderInformation();

            //dont respond. thats for the creator of this object to decide.
            //at the end of the day this really is just a request object,
            //not a repsonse.
        }

        public void respond()
        {
            if (protocolVersion.StartsWith("HTTP/")) HTTPConnectionHelper.respondTo(this);
            //if (protocolVersion.StartsWith("MNDP/")) MDNPConnectionHelper.respondTo();
        }

        private void readProtocolInformation()
        {
            string line = reader.ReadLine();
            if (line != null)
            {
                string[] parts;
                parts = line.Split(' ');
                if (parts.Length != 3)
                    throw new InvalidHTTPRequestException("Error parsing " + line);
                method = parts[0];
                fileRequestPath = parts[1];
                protocolVersion = parts[2];
            }
            else throw new InvalidHTTPRequestException("Expected HTTP request declaration, got null");
            Console.WriteLine("Method:            " + method);
            Console.WriteLine("File Requested:    " + fileRequestPath);
            Console.WriteLine("HTTP Version:      " + protocolVersion);
            Console.WriteLine();
        }

        private void readHeaderInformation()
        {
            Console.WriteLine("Reading Header Information...");

            string line = "";
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
        }
    }

    public static class HTTPConnectionHelper
    {



        public static void respondTo(HTTPRequest request)
        {
            request.fileRequestPath = HTTPRequest.ROOT + request.fileRequestPath.Replace("/", "\\");

            Console.WriteLine(request.ip + " is requesting " + request.fileRequestPath);

            if (!File.Exists(request.fileRequestPath))
            {

                Console.WriteLine("" + request.fileRequestPath + " does not exist, appending index.html.");
                request.fileRequestPath += "index.html";

            }

            if (File.Exists(request.fileRequestPath))
            {
                //it should be legit by now...
                StreamWriter writer = new StreamWriter(request.stream);
                Console.WriteLine("Sending " + request.fileRequestPath + " to " + request.ip);

                writer.WriteLine("HTTP/1.1 200 OK");

                writer.WriteLine("");
                byte[] fileBuffer = File.ReadAllBytes(request.fileRequestPath);
                writer.Write(Encoding.ASCII.GetString(fileBuffer));
                writer.Close();
            }
            else
            {
                //return the 404 page
            }

            //stream.Close();
        }

    }
}
