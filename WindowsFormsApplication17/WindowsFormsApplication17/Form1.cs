using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace WindowsFormsApplication17
{
   
    public partial class Form1 : Form
    {
        private static int portReceive = 9060;
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //объявление сокета
        private byte[] buffer = new byte[UInt16.MaxValue];
        public List<byte[]> array = new List<byte[]>();
        public int count_bytes = 0; //общее количество байтов
        private static IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, portReceive);
        private EndPoint endPoint = (EndPoint)ipEndPoint;
        public Form1()
        {
            InitializeComponent();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //связываем сокет с локальной конечной точкой
            socket.Bind(endPoint);
            //начинаем асинхронный прием данных
            socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(ReceiveCallback), socket);
        }


        public void ReceiveCallback(IAsyncResult ar)
        {
            
                while (true)
                {
                
                    int m = 0; //подсчет количества принятых массивов байтов
                    short value;//количество массивов байтов, которые мы должны принять
                    do
                    {

                        byte[] buffer_rez = new byte[65002];

                        int n = socket.ReceiveFrom(buffer_rez, ref endPoint); //получаем датаграмму в буфер данных
                   
                        count_bytes += n - 2; 
                        byte[] bytes_arr = new byte[n - 2];
                        byte[] two_bytes = new byte[2];
                        Array.Copy(buffer_rez, n - 2, two_bytes, 0, 2); //копирование из буфера в two_bytes 2 последних байта
                        Array.Copy(buffer_rez, 0, bytes_arr, 0, bytes_arr.Length);//копирование в bytes_arr всех байтов кроме последних двух
                        value = BitConverter.ToInt16(two_bytes, two_bytes.Length - 2); //получаем количество принимаемых байтов
                        array.Add(bytes_arr); //заполняем лист массивов
                        m++;

                    } while (m != value);


                    byte[] full_array = new byte[count_bytes];

                    for (int i = 0; i < array.Count; i++)
                    {
                        Array.Copy(array[i], 0, full_array, i * 65000, array[i].Length); //полученные массивы байтов объединяем в один массив

                    }

                    byte[] result = Dec(full_array); //декомпрессия 

                    MemoryStream memory_stream = new MemoryStream(result);
                    System.Drawing.Bitmap bmp =
                       (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memory_stream);
                    pictureBox1.Image = bmp; //вставка изображения
                //очищение для получения нового рисунка
                    array.Clear();
                    count_bytes = 0;

                }
          
        }

        static byte[] Dec(byte[] full_array)
        {
             
                if (full_array == null)
                    throw new ArgumentNullException("byteData", @"inputData must be non-null");

                using (var compressedMs = new MemoryStream(full_array))
                {
                    using (var decompressedMs = new MemoryStream())
                    {
                        using (var gzs = new BufferedStream(new GZipStream(compressedMs, CompressionMode.Decompress), 4096))
                        {
                            gzs.CopyTo(decompressedMs);
                        }
                        return decompressedMs.ToArray();
                    }
                }
            
        }


      

        
    }   
}
