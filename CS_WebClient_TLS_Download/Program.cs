using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

//REF: https://blog.darkthread.net/blog/webclient-and-tls12/

namespace CS_WebClient_TLS_Download
{
    class Program
    {
        static void pause()
        {
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        public static bool ValidateServerCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        static void Main(string[] args)
        {

            try
            {
                WebClient wc = new WebClient();
                //REF: https://stackoverflow.com/a/39534068/288936
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                //REF: https://docs.microsoft.com/zh-tw/dotnet/api/system.net.servicepointmanager.expect100continue?view=netframework-4.7.2
                //REF: https://www.cnblogs.com/dudu/p/the_remote_certificate_is_invalid_according_to_the_validation_procedure.html
                ServicePointManager.UseNagleAlgorithm = true;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.CheckCertificateRevocationList = false;//The remote certificate is invalid according to the validation procedure
                ServicePointManager.DefaultConnectionLimit = ServicePointManager.DefaultPersistentConnectionLimit;

                //REF: http://libraclark.blogspot.com/2007/06/ssl.html
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

                //REF: https://docs.microsoft.com/en-us/dotnet/api/system.net.webclient.downloadfile?view=netframework-4.7.2
                wc.DownloadFile("https://www.moi.gov.tw/", "download00.html");

                //REF: https://www.qiufengblog.com/articles/csharp-request-https.html
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.moi.gov.tw/");
                HttpWebResponse response = null;
                try
                {
                    response = request.GetResponse() as HttpWebResponse;
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            string content = reader.ReadToEnd();
                            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download01.html");
                            File.WriteAllText(file, content, Encoding.UTF8);
                        }
                    }
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;
                    Console.WriteLine("statuscode ={0}", response.StatusCode);
                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }
                Console.WriteLine("download finish...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR=" + ex.ToString());
            }

            pause();
        }
    }
}
