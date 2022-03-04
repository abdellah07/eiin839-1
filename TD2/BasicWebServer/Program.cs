using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

/**
     * exercice 1

    http://localhost:8080/api/MyMethod?
    http://localhost:8080/api/stringMethode?param1=ceci%20est%20un%20parametre
    http://localhost:8080/api/somme?param1=2&param2=3

    exercice 2

    http://localhost:8080/api/externalCall?param1=2
**/


namespace BasicServerHTTPlistener
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            //if HttpListener is not supported by the Framework
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("A more recent Windows version is required to use the HttpListener class.");
                return;
            }
 
 
            // Create a listener.
            HttpListener listener = new HttpListener();

            // Add the prefixes.
            if (args.Length != 0)
            {
                foreach (string s in args)
                {
                    listener.Prefixes.Add(s);
                    // don't forget to authorize access to the TCP/IP addresses localhost:xxxx and localhost:yyyy 
                    // with netsh http add urlacl url=http://localhost:xxxx/ user="Tout le monde"
                    // and netsh http add urlacl url=http://localhost:yyyy/ user="Tout le monde"
                    // user="Tout le monde" is language dependent, use user=Everyone in english 

                }
            }
            else
            {
                Console.WriteLine("Syntax error: the call must contain at least one web server url as argument");
            }
            listener.Start();

            // get args 
            foreach (string s in args)
            {
                Console.WriteLine("Listening for connections on " + s);
            }

            // Trap Ctrl-C on console to exit 
            Console.CancelKeyPress += delegate {
                // call methods to close socket and exit
                listener.Stop();
                listener.Close();
                Environment.Exit(0);
            };


            while (true)
            {
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                string documentContents;
                using (Stream receiveStream = request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        documentContents = readStream.ReadToEnd();
                    }
                }
                
                // get url 
                Console.WriteLine($"Received request for {request.Url}");

                //get url protocol
                Console.WriteLine(request.Url.Scheme);
                //get user in url
                Console.WriteLine(request.Url.UserInfo);
                //get host in url
                Console.WriteLine(request.Url.Host);
                //get port in url
                Console.WriteLine(request.Url.Port);
                //get path in url 
                Console.WriteLine(request.Url.LocalPath);

                string methodeName = request.Url.Segments[request.Url.Segments.Length - 1];

                // parse path in url 
                foreach (string str in request.Url.Segments)
                {
                    Console.WriteLine(str);
                }

                //get params un url. After ? and between &

                Console.WriteLine(request.Url.Query);

                //parse params in url
                Console.WriteLine("param1 = " + HttpUtility.ParseQueryString(request.Url.Query).Get("param1"));
                Console.WriteLine("param2 = " + HttpUtility.ParseQueryString(request.Url.Query).Get("param2"));

                //
                Console.WriteLine(documentContents);

                // Obtain a response object.
                HttpListenerResponse response = context.Response;

                string result = "NONE WAS RETURNED";

                int i = 1;

                ArrayList arrayList= new ArrayList();

                while (HttpUtility.ParseQueryString(request.Url.Query).Get("param"+i) != null)
                {
                    
                    arrayList.Add(HttpUtility.ParseQueryString(request.Url.Query).Get("param" + i));

                    i++;
                }

                

                if (methodeName != null && methodeName != "favicon.ico")
                {
                    Type type = typeof(MyReflectionClass);
                    MethodInfo method = type.GetMethod(methodeName);
                    MyReflectionClass c = new MyReflectionClass();
                    result = (string)method.Invoke(c, arrayList.ToArray());
                    Console.WriteLine(result);
                   
                }

                // Construct a response.
                string responseString = "<HTML><BODY> "+ result + "</BODY></HTML>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }
            // Httplistener neither stop ... But Ctrl-C do that ...
             listener.Stop();
        }
    }

    public class MyReflectionClass
    {
        public string MyMethod()
        {
            Console.WriteLine("Call MyMethod 1");
            return "Call MyMethod 2";
        }

        public string stringMethode(String message)
        {
            return "call methode param reader -> " + message;
        }

        public String somme(String a, String b)
        {
            return "cela est la somme de parametre -> " + (Int32.Parse(a) * Int32.Parse(b));
        }

        public String externalCall(String args)
        {
            Console.WriteLine("external call");
            //
            // Set up the process with the ProcessStartInfo class.
            // https://www.dotnetperls.com/process
            //
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"..\..\..\ExecTest\bin\Debug\ExecTest.exe"; // Specify exe name.

            start.Arguments = args;// Specify arguments.
            
           
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            //
            // Start the process.
            //
            string result = "NONE";
            using (Process process = Process.Start(start))
            {
                //
                // Read in all the text from the process with the StreamReader.
                //
                
                using (StreamReader reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd();
                    Console.WriteLine(result);
                }
            }
            return result;
        }
    }
}