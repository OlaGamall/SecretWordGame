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
        Label label1;

        ////
        int client_num;

        public Form1()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            client = new TcpClient();
            byte[] ip_byte = { 127, 0, 0, 1 };
            local_address = new IPAddress(ip_byte);
            client.Connect(local_address, 2000);
            nStream = client.GetStream();
            sr = new BinaryReader(nStream);
            sw = new BinaryWriter(nStream);
            button1.Enabled = false;
            startRecieveTask = new Task(RecieveMessage);
            startRecieveTask.Start();

            readmsgfromserver = new Task(readMsg);


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
                if (nStream.DataAvailable)// (msg = sr.ReadString()) != null)
                {
                    msg = sr.ReadString();
                    //MessageBox.Show(msg);
                    var commaIndicator = msg.IndexOf(',');
                    var underscoreIndicator = msg.IndexOf('_');

                    if (msg == "0")
                    {
                        MessageBox.Show("server is out!");
                        closeConnection();
                    }
                    else
                    {
                        if (commaIndicator == -1)  //first time connection
                        {
                            string[] words = msg.Split('/');
                            client_num = int.Parse(words[0]);
                            //MessageBox.Show(client_num.ToString());
                            label1.Text = words[1];
                            if (client_num != 0)
                                panel1.Enabled = false;
                        }
                        else
                        {
                            string[] words = msg.Split('/');
                            int play_client = int.Parse(words[0]);
                            string[] words2 = words[1].Split(',');  //char,word
                            string letter = words2[0];
                            string newword = words2[1];
                            foreach (var button in panel1.Controls.OfType<Button>())
                            {
                                if (button.Text == letter.ToUpper())
                                    button.Enabled = false;
                            }
                           
                            //MessageBox.Show(newword);
                            if (play_client == client_num)  //your turn
                            {

                                if (newword == label1.Text)
                                {

                                    MessageBox.Show("wrong letter");
                                    panel1.Enabled = false;
                                }
                                else
                                {
                                    if (underscoreIndicator == -1)
                                    {
                                        label1.Text = newword;
                                        MessageBox.Show("You win");
                                        closeConnection();
                                    }
                                    else
                                    {
                                        label1.Text = newword;
                                        MessageBox.Show("your turn");
                                        panel1.Enabled = true;
                                    }

                                }
                            }
                            else //other client turn
                            {
                                if (newword == label1.Text) //wrong letter
                                {
                                    if (client_num == int.Parse(words2[2])) //your turn
                                    {
                                        MessageBox.Show("your turn");
                                        panel1.Enabled = true;
                                    }
                                }
                                else
                                {
                                    label1.Text = newword;

                                    if (underscoreIndicator == -1)
                                    {
                                        MessageBox.Show("you lose");
                                        closeConnection();
                                    }
                                    else
                                    {
                                        //MessageBox.Show("right letter");
                                        panel1.Enabled = false;
                                    }

                                }

                            }
                        }
                    }
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            label1 = new Label();
            label1.Size = new Size(350, 50);
            label1.Location = new Point(100, 50);
            label1.Font = new Font("Times New Roman", 30);
            label1.BackColor = Color.Transparent;

            this.Controls.Add(label1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sw.Write("0");
            closeConnection();
        }

        private void button28_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var val = btn.Text;
            sw.Write(val);
        }



        void closeConnection()
        {
            sw.Close();
            sr.Close();
            nStream.Close();
            this.Close();
        }
    }
}
