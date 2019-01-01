using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace ImageLoader
{
    class Program
    {
        const string ALPHABET = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
        const string BASE_URL = "https://i.imgur.com/";
        const string DIR_NAME = "Scraped_Images";

        const int LINK_LENGTH = 5;

        static void Main(string[] args)
        {
            int threads = Environment.ProcessorCount;
            Console.WriteLine("Threads: " + threads.ToString());
            try
            {
                Directory.CreateDirectory(DIR_NAME);
            }
            catch
            {
                Console.WriteLine("Cant create folder");
                Console.WriteLine("Exiting...");
                return;

            }
            Console.WriteLine(DIR_NAME + " directory was created");
            Task[] tasks = new Task[threads];
            for (int j = 0; j < threads; j++)
            {
                tasks[j] = new Task(SaveScreenshoot);
                tasks[j].Start();
            }

            while (true)
            {
                int index = Task.WaitAny(tasks);
                tasks[index] = new Task(SaveScreenshoot);
                tasks[index].Start();

            }

        }
        private static int ByteHash(List<byte> data)
        {
            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;

                for (int i = 0; i < data.Count; i++)
                    hash = (hash ^ data[i]) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }
        private static Random r = new Random();

        private static void SaveScreenshoot()
        {
            StringBuilder sb = new StringBuilder();
            List<byte> bytes = new List<byte>();
            sb.Append(BASE_URL);

            for (int i = 0; i < LINK_LENGTH; i++)
            {
                char randChar = ALPHABET[r.Next(0, ALPHABET.Length)];
                sb.Append(randChar);
            }
            sb.Append(".png");
            string fileName = sb.ToString().Replace(BASE_URL, "");
            string url = sb.ToString();
            LogStatus(fileName, "Loading...");
            using (Stream ws = WebRequest.Create(url).GetResponse().GetResponseStream())
            {
                while (true)
                {
                    int b = ws.ReadByte();
                    if (b == -1)
                    {
                        LogStatus(fileName, "Load complete");
                        break;
                    }
                    bytes.Add((byte)b);
                }


                int hash = ByteHash(bytes);
                if (hash == -1796356933)//hash of invalid image
                {
                    LogStatus(fileName, "Invalid image");
                    return;
                }
                LogStatus(fileName, "Saving to file");
                using (FileStream fs = new FileStream(DIR_NAME + "/" + fileName, FileMode.Create))
                {
                    fs.Write(bytes.ToArray(), 0, bytes.Count);
                }

            }
        }
        private static void LogStatus(string name, string status)
        {
            Console.WriteLine(name + " : " + status);
        }
    }
}
