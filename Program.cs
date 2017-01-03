using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace ikziz
{
    class Program
    {
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



            /*
            string[] fileEntries = Directory.GetFiles(source);
            string fileDirectory, fileName, basename;
            Regex regFileName = new Regex("( |_){1,}", RegexOptions.Multiline);
            Regex regFileExtension = new Regex(@"\.[a-z0-9]{2,4}$", RegexOptions.IgnoreCase);
            foreach (string file in fileEntries)
            {
                basename = Path.GetFileName(file);
                basename = regFileName.Replace(basename, ".");
                basename = regFileExtension.Replace(basename, "");

                fileDirectory = source + @"\" + basename;
                fileName = fileDirectory + @"\" + Path.GetFileName(file);
                if (!Directory.Exists(basename))
                {
                    try
                    {
                        Directory.CreateDirectory(fileDirectory);
                        Console.WriteLine("\t(CD)\t" + basename);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\t(Error CD)\t" + basename + " (" + e.Message + ")");
                    }
                }
                if (!File.Exists(fileName))
                {
                    try
                    {
                        File.Move(file, fileName);
                        Console.WriteLine("\t(MF)\t" + fileName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\t(Error MF)\t" + fileName + " (" + e.Message + ")");
                    }
                }
            }

            // Rename directory .
            string[] directoryEntries = Directory.GetDirectories(source, "*");
            Regex regDirectory = new Regex(@"\.", RegexOptions.IgnoreCase);
            Regex regYearDirectory = new Regex(@" ([0-9]{4})$", RegexOptions.IgnoreCase);
            foreach (string dir in directoryEntries)
            {
                string newName = regDirectory.Replace(dir, " ");
                newName = newName.Trim();
                newName = regYearDirectory.Replace(newName, " ($1)");
                if (dir != newName)
                {
                    try
                    {
                        Directory.Move(dir, newName);
                        Console.WriteLine("\t(RD)\t" + newName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\t(Error RD)\t" + newName + " (" + e.Message + ")");
                    }
                }
            }
            */

            Console.WriteLine("Terminé...");
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

        static List<String> ParseSite(String url, String destination)
        {
            List<string> urls = new List<string>();
            List<string> images = new List<string>();

            string content = GetFromUrl(url);

            string[] directory = url.Split('/');
            string title = directory[directory.Length - 1];
            string author = directory[directory.Length - 2];

            // Get all other link
            Regex regUrls = new Regex(@"<a href=""([^""]{1,})""", RegexOptions.IgnoreCase);
            Regex regLarges = new Regex(@"/large/", RegexOptions.IgnoreCase);
            Regex regUrl = new Regex(@"^/.{1,}", RegexOptions.IgnoreCase);

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
                                Console.WriteLine(link);
                                Console.WriteLine("");

                                WebClient webClient = new WebClient();
                                webClient.DownloadFile(link, destination + "/" + author + "/" + title + "/img_" + countImg + ".jpg");
                                countImg++;
                            }
                        }
                        else if(regUrl.IsMatch(link))
                        {
                            if (!urls.Contains(link))
                            {
                                if (regUrl.IsMatch(link))
                                    urls.Add(url + link);
                            }
                        }
                    }
                }
                m = m.NextMatch();
            }
            urls.Sort();

            // Parse all link
            urls.ForEach(delegate (String link)
            {
                //ParseSite(link);
            });

            return urls;
        }

    }
}
