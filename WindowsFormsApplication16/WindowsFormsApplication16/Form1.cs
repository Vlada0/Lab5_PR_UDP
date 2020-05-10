using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApplication16
{
    public partial class Form1 : Form
    {
        private static Bitmap bmpScreenshot;
        private static Graphics gfxScreenshot;
        byte[] data;
        int count_array=0;
        bool flag;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Thread thread = new Thread(Run); //запуск потока 
            thread.Start();
        }


        public void Run() { 

        var ip = IPAddress.Parse("127.0.0.1");
        Socket sock1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        sock1.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, 1);
            IPEndPoint iep1 = new IPEndPoint(ip, 9060); 
            byte[] g = new byte[0];
            sock1.SendTo(g, iep1);
            flag = true;
            do
            {
                Thread.Sleep(15);
        int position = 0;

        
        //создание скриншота
        bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
        gfxScreenshot = Graphics.FromImage(bmpScreenshot);
        gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
        //вызов функции сжатия скриншота
        data = CompressBitmap(bmpScreenshot);

        List<byte[]> send_file = new List<byte[]>(); //лист массивов байтов
        
                if (data.Length % 65002 != 0) //определение количества массивов байтов
                {
                    count_array = data.Length / 65002 + 1; //если остаток от деления не равен 0
                }
                else
                    count_array = data.Length / 65002; //если остаток равен 0
                while (position < data.Length) //пока position меньше количества байтов в data
                {
                    byte[] bytes;
                    if ((data.Length - position) >= 65000)
                        bytes = new byte[65002];
                    else
                    {
                        bytes = new byte[data.Length - position + 2]; //объявление последнего массива байтов количеством оставшихся в потоке байт
                    
                    }
                 
                    Array.Copy(data, position, bytes, 0, bytes.Length-2); //заполнеям массив bytes с нужной позиции (кроме 2 последних байта)
                    position += bytes.Length - 2; //увеличиваем позицию
                    bytes[bytes.Length - 2] = (byte) (count_array); //заносим в 2 последних байта количество массивов байтов
                    
                    send_file.Add(bytes);//заносим в лист массив байтов
                  
                }
             
                
                for (int i=0; i<count_array; i++)
                {
                    
                    sock1.SendTo(send_file[i], iep1); //отправка листа массивов байтов
                    Thread.Sleep(5); 

                }

            } while (flag);
            sock1.Close(); //закрытие сокета
    }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            flag = false;
        }

        private byte[] CompressBitmap(Bitmap bmp)
        {
            using (var zipped = new MemoryStream())
            {
                using (var gzip = new GZipStream(zipped, CompressionMode.Compress)) 
                    bmp.Save(gzip, ImageFormat.Bmp);

                return zipped.ToArray();
                
            }
        }


       
    }
}
