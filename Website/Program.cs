using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static string ROOT = "T:\\Website-Data";//"C:\\Users\\mgosselin\\Desktop";

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

        public Dictionary<string, string> headers = new Dictionary<string, string>();

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
            if (broken) return;
            else if (protocolVersion.StartsWith("HTTP/") && method == "GET") HTTPConnectionHelper.respondToGet(this);
            else if (protocolVersion.StartsWith("HTTP/") && method == "SEND") HTTPConnectionHelper.respondToSend(this);
            else if (protocolVersion.StartsWith("HTTP/") && method == "SAVE") HTTPConnectionHelper.respondToSave(this);
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
            else
            {

                reader.Close();
                stream.Close();
                broken = true;
                throw new InvalidHTTPRequestException("Expected HTTP request declaration, got null");

            }
            Console.WriteLine("|");
            Console.WriteLine("|     Request From " + ip + ":" + port);
            Console.WriteLine("|");
            Console.WriteLine("|   Method:            " + method);
            Console.WriteLine("|   File Requested:    " + fileRequestPath);
            Console.WriteLine("|   HTTP Version:      " + protocolVersion);
            Console.WriteLine("|");
        }

        private void readHeaderInformation()
        {

            Console.WriteLine("|");
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
                        value = formatForWidth(value, 50);
                        value = value.Replace("\n", "\n|      ");
                        Console.WriteLine("|   " + key + "\n|      " + value + "\n|");
                    }
                    //so then we exit the lopo.
                    else break;
                }
                catch (Exception e)
                {
                    throw new InvalidHTTPRequestException("Error parsing " + line);
                }
            }
            if (headers.Count == 0)
            {

                Console.WriteLine("|   No Header Information");
                Console.WriteLine("|");
            }
        }

        private string formatForWidth(string value, int maxWidth)
        {
            StringBuilder builder = new StringBuilder();
            int lastSpace = -1;
            int currentWidth = 0;
            for(int i = 0; i < value.Length; i ++)
            {

                builder.Append(value[i]);
                currentWidth++;

                if(value[i] == ' ') {
                    lastSpace = i;
                }

                if (currentWidth >= maxWidth)
                {
                    if (lastSpace == -1)
                    {
                        builder.Append("\n");
                    }
                    else
                    {
                        builder.Insert(lastSpace, "\n");
                        lastSpace = -1;
                    }
                    currentWidth = 0;
                }

            }
            return builder.ToString();
        }
    }

    public static class HTTPConnectionHelper
    {



        public static void respondToGet(HTTPRequest request)
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
                //FileStream fileStream = new FileStream("T:\\Website\\HTTPOUTPUT\\" + DateTime.Now.Ticks + ".txt", FileMode.CreateNew);
                BinaryWriter writer = new BinaryWriter(request.stream, Encoding.ASCII);
                Console.WriteLine("Sending " + request.fileRequestPath + " to " + request.ip);

                write("HTTP/1.1 200 OK\r\n", writer);
                
                write("\r\n", writer);
                byte[] fileBuffer = File.ReadAllBytes(request.fileRequestPath);
                writer.Write((fileBuffer));
                writer.Close();
            }
            else
            {
                //it should be legit by now...
                //FileStream fileStream = new FileStream("T:\\Website\\HTTPOUTPUT\\" + DateTime.Now.Ticks + ".txt", FileMode.CreateNew);
                BinaryWriter writer = new BinaryWriter(request.stream, Encoding.ASCII);
                Console.WriteLine("Sending " + request.fileRequestPath + " to " + request.ip);

                write("HTTP/1.1 200 OK\r\n", writer);

                write("\r\n", writer);
                byte[] fileBuffer = File.ReadAllBytes(HTTPRequest.ROOT + "\\404.html");
                writer.Write((fileBuffer));
                writer.Close();
            }

            //stream.Close();
        }

        //because binary writers literally SU U U U CK K K K.
        //they append strings by prefixing them with the number of
        //character in the string. BAD BECAUSE HTTP PARSERS NO LIKE
        private static void write(string str, BinaryWriter writer) {
            //so instead we convert the string using a utf8 decoding
            //to a byte array, then send it along its way.
            writer.Write(Encoding.UTF8.GetBytes(str));
        }

        public static void respondToSend(HTTPRequest request) {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.WorkingDirectory = HTTPRequest.ROOT;
            startInfo.Arguments = "/C " + HTTPRequest.ROOT + "\\PythonBin\\python.exe " + HTTPRequest.ROOT + "\\PythonBin\\send.py \"" + request.headers["message"] + "\"";
            

            int pid = new Random().Next(1000, 9999);

            StringBuilder builder = new StringBuilder();

            startInfo.RedirectStandardOutput = true;
            process.ErrorDataReceived += delegate(object o, DataReceivedEventArgs e) {
                Console.WriteLine("[" + pid + " Python err] " + e.Data);
            };


            startInfo.RedirectStandardError = true;
            process.OutputDataReceived += delegate(object o, DataReceivedEventArgs e) {
                
                Console.WriteLine("[" + pid + " Python out] " + e.Data);
                builder.Append(e.Data + "\n");
                if (e.Data == "Done!")
                {
                    BinaryWriter writer = new BinaryWriter(request.stream);
                    write("HTTP/1.1 200 OK\r\n\n", writer);
                    write(builder.ToString(), writer);
                    writer.Close();
                }

            };

            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

        }

        public static void respondToSave(HTTPRequest request)
        {
            string toWrite = request.headers["numbers"].Replace(" ", "\n");
            FileStream file = new FileStream(HTTPRequest.ROOT + "\\" + request.fileRequestPath, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            write(toWrite, writer);
            writer.Close();

        }
    }
}
