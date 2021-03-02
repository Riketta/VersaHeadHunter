using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VersaHeadHunter
{
    class Downloader
    {
        private static readonly Logger logger = Logger.GetLogger();

        public static string DownloadURL(string url)
        {
            logger.Debug($"Downloading URL \"{url}\"");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

            string data = readStream.ReadToEnd();

            receiveStream.Close();
            response.Close();

            return data;
        }
    }
}
