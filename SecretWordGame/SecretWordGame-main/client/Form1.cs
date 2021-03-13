using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testconnection
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream nStream;
        BinaryWriter sw;
        BinaryReader sr;
        IPAddress local_address;
        Task startRecieveTask;
        Task readmsgfromserver;
        Button[] btns;
        string[] letters;
        //Label label1;
        int wordLength;

        public Form1()
        {
            InitializeComponent();
            btns = new Button[26] { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z };
            letters = new string[26] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        
        }

        private void RecieveMessage()
        {
            string msg;
            while (true)
            {

                if ((msg = sr.ReadString()) != null)
                {

                    if (msg == "no")
                    {
                        MessageBox.Show("refused");
                        sr.Close();
                        sw.Close();
                        nStream.Close();
                        break;
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show(msg, "info", MessageBoxButtons.OKCancel);
                        if (result == DialogResult.OK)
                        {
                            sw.Write("yes");
                            panel2.Visible = false;
                            readmsgfromserver.Start();
                            break;
                        }
                        else
                        {
                            sw.Write("no");
                            sr.Close();
                            sw.Close();
                            nStream.Close();
                            break;
                        }
                    }
                }
            }
        }

        void readMsg()
        {
            string msg;
            while (true)
            {
                if ((msg = sr.ReadString()) != null)//nStream.DataAvailable)
                {
                    //msg = sr.ReadString();
                    var commaIndicator = msg.IndexOf(',');
                    var underscoreIndicator = msg.IndexOf('_');

                    if (msg == "0")
                    {
                        MessageBox.Show("server is out! You Win");
                        closeConnection();
                    }
                    else if (msg.Contains("length:"))
                    {
                        wordLength = int.Parse(msg.Split(':')[1]);
                        label1.Text = "";
                        for (int i = 0; i < wordLength; i++) label1.Text += "_";
                        label2.Text = msg.Split(':')[2];
                    }
                    else
                    {
                        if (commaIndicator == -1)
                        {
                            if (msg == label1.Text)
                            {

                                label3.Text = "Server turn";
                                panel1.Enabled = false;
                            }
                            else
                            {
                                if (underscoreIndicator == -1)
                                {
                                    label1.Text = msg;
                                    MessageBox.Show("You win");
                                    closeConnection();
                                }
                                else
                                {
                                    label1.Text = msg;
                                    label3.Text = "Your turn";
                                    panel1.Enabled = true;
                                }

                            }
                        }
                        else
                        {

                            string[] words = msg.Split(',');
                            int index = Array.IndexOf(letters, words[0]);
                            btns[index].Enabled = false;
                            btns[index].BackColor = Color.Gainsboro;
                            if (words[1] == label1.Text)
                            {
                                label3.Text = "Your turn";
                                panel1.Enabled = true;
                            }
                            else
                            {

                                if (words[1].IndexOf("_") == -1)
                                {
                                    MessageBox.Show("you lose");
                                    closeConnection();
                                }
                                else
                                {
                                    label1.Text = words[1];
                                    panel1.Enabled = false;
                                    label3.Text = "Server turn";
                                }

                            }

                        }
                    }



                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           
            label1.Text = "";
            label2.Text = "";
            panel1.Enabled = false;
        }

    
        void closeConnection()
        {
            sw.Close();
            sr.Close();
            nStream.Close();
            panel2.Visible = true;
            label1.Text = "";
            label2.Text = "";
            panel1.Enabled = false;
            foreach (var item in btns)
            {
                item.Enabled = true;
                item.BackColor = Color.White;
            }
            //this.Close();
        }

        private void button27_Click(object sender, EventArgs e)
        {
            sw.Write("0");
            closeConnection();
        }

        private void button28_Click_1(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();
                byte[] ip_byte = { 127, 0, 0, 1 };
                local_address = new IPAddress(ip_byte);
                client.Connect(local_address, 2000);
                nStream = client.GetStream();
                sr = new BinaryReader(nStream);
                sw = new BinaryWriter(nStream);
                
                startRecieveTask = new Task(RecieveMessage);
                startRecieveTask.Start();

                readmsgfromserver = new Task(readMsg);
            }
            catch
            {
                MessageBox.Show("Server is not open");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var val = btn.Text;
            int index = Array.IndexOf(letters, val);
            btns[index].Enabled = false;
            btns[index].BackColor = Color.Gainsboro;
            sw.Write(val);
        }

    }
}
