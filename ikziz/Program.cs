using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace ikziz
{
    class Program
    {

        static HashSet<String> UrlsParsed = new HashSet<String>();

        static void Main(string[] args)
        {
            string source = "";
            string destination = "./";

            Console.Write("Url : ");
            while(String.IsNullOrEmpty(source))
            {
                source = Console.ReadLine();
            }
            Regex reghttp = new Regex("^http", RegexOptions.IgnoreCase);
            if(! reghttp.IsMatch(source))
            {
                source = "http://" + source;
            }

            Console.Write("Repertoire de destination : ");
            if(String.IsNullOrEmpty(destination))
            {
                destination = Console.ReadLine();
            }
            while (!Directory.Exists(destination))
            {
                Console.Write("Le repertoire de destination " + destination + " n'existe pas : ");
                destination = Console.ReadLine();
            }

            ParseSite(source, destination);
            
            Console.WriteLine("\n\n....:::::::::::::Terminé:::::::::::::....");
            Console.Read();
        }

        static string GetFromUrl(string url)
        {
            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            try
            {
                return client.DownloadString(url);
            }catch(Exception e)
            {
                Console.WriteLine("Unable to open " + url + " : " + e.Message);
                return "";
            }
        }

        static void ParseSite(String url, String destination)
        {

            if(UrlsParsed.Contains(url))
            {
                return;
            }
            Console.WriteLine("\n-----------------\nReading " + url);

            UrlsParsed.Add(url);
            List<string> urls = new List<string>();
            List<string[]> images = new List<string[]>();

            string content = GetFromUrl(url);

            string[] directory = url.Split('/');
            string title = directory[directory.Length - 1].Split('?')[0].Split('#')[0];
            string author = directory[directory.Length - 2];
            string baseUrl = directory[0] + "//" + directory[2];

            // Get all other link
            Regex regUrls = new Regex(@" href=""([^""]{1,})""", RegexOptions.IgnoreCase);
            Regex regLarges = new Regex(@"/large/", RegexOptions.IgnoreCase);
            Regex regUrl = new Regex(@"^/.{1,}", RegexOptions.IgnoreCase);
            Regex regHttp = new Regex(@"^/http", RegexOptions.IgnoreCase);

            Match m = regUrls.Match(content);
            int countImg = 1;
            while (m.Success)
            {
                for (int i = 1; i <= 2; i++)
                {
                    Group g = m.Groups[i];
                    CaptureCollection cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        //Capture c = cc[j];
                        String link = cc[j].ToString();
                        if (regLarges.IsMatch(link))
                        {
                            if (!Directory.Exists(destination + "/" + author))
                            {
                                Directory.CreateDirectory(destination + "/" + author);
                            }
                            if (!Directory.Exists(destination + "/" + author + "/" + title))
                            {
                                Directory.CreateDirectory(destination + "/" + author + "/" + title);
                            }

                            if (!File.Exists(destination + "/" + author + "/" + title + "/img_" + countImg + ".jpg"))
                            {
                                images.Add(new string[] { link, destination + "/" + author + "/" + title + "/img_" + countImg + ".jpg" });
                                countImg++;
                            }
                        }
                        else if(regUrl.IsMatch(link))
                        {
                            if (regUrl.IsMatch(link))
                            {
                                urls.Add(baseUrl + link);
                            } else if(regHttp.IsMatch(link))
                            {
                                urls.Add(link);
                            } else
                            {
                                urls.Add(url + link);
                            }
                        }
                    }
                }
                m = m.NextMatch();
            }

            // Save image
            Console.WriteLine("\tSaving images " + images.Count);
            countImg = 1;
            images.ForEach(delegate (String[] img)
            {
                Console.WriteLine("\t\t"+ countImg  + "/" + images.Count);
                try
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(img[0], img[1]);
                }catch(Exception e)
                {
                    Console.WriteLine("\t\tError " + img[1] + " : " + e.Message);
                }
                countImg++;
            });

            // Parse all link
            urls.Sort();
            urls.ForEach(delegate (String link)
            {
                ParseSite(link, destination);
            });
        }

    }
}
