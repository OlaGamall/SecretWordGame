using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace SecretWordGameServer
{
    public partial class Form1 : Form
    {
        TcpListener server;
        Task listen;
        Task readMessage;
        public Socket connection;
        public NetworkStream nstream;
        public BinaryWriter writer;
        public BinaryReader reader;
        List<string> words;
        public String SelectedWord;

        
        // multi client
        private List<NetworkStream> Clients;
        List<BinaryWriter> wList;
        List<BinaryReader> rList;
        List<Socket> conList;
        int client_num = 0;
        

        //game
        char[] arr_checkword;
        Task readLetterFromClient;

        public ComboBox categry_comboBox { get { return comboBox1; } }
        public Form1()
        {
            InitializeComponent();                         
            byte[] ip = new byte[] { 127, 0, 0, 1 };
            IPAddress publicAddress = new IPAddress(ip);
            //IPAddress publicAddress = Dns.GetHostByName(Dns.GetHostName()).AddressList[0];
            //MessageBox.Show(publicAddress.ToString());
            server = new TcpListener(publicAddress, 2000);           
            
            words = new List<string>();
            // ---------multi client
            wList = new List<BinaryWriter>();
            rList = new List<BinaryReader>();
            Clients = new List<NetworkStream>();
            conList = new List<Socket>();
            // tasks
            readLetterFromClient = new Task(LetterFromClient);
        }

        //[elm1, elm2, elm3, ......]  word-cat-level push --> []
        private void Form1_Load(object sender, EventArgs e)
        {
            StreamReader reader = File.OpenText(@"..\..\..\categories.txt");
            string input;
            while ((input = reader.ReadLine()) != null)
            {
                comboBox1.Items.Add(input.Split(' ')[0]);
            }
            reader.Close();
            comboBox1.SelectedIndex = 0;      
            comboBox2.SelectedItem = "1";

           
        }
        private void start_Click(object sender, EventArgs e)
        {
            listen = new Task(StartListen);
            listen.Start(); //start listening to client request
            comboBox1.Enabled = false; comboBox2.Enabled = false;textBox1.Enabled = false;
            start.Enabled = false;
            select_word();
        }
        private void StartListen()
        {
            while (client_num<int.Parse(textBox1.Text))
            {
                server.Start();
                connection = server.AcceptSocket();
                nstream = new NetworkStream(connection);
                writer = new BinaryWriter(nstream);
                reader = new BinaryReader(nstream);
                DialogResult dresult = MessageBox.Show("Client want to start a game!", "Game Request", 
                                                           MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dresult == DialogResult.OK)
                {
                  
                    writer.Write("Category: " + comboBox1.SelectedItem + ", Level: " + comboBox2.SelectedItem);
                    readMessage = new Task(GetMessageFromClient);
                    readMessage.Start();

                }
                else
                {
                    writer.Write("no");
                    writer.Close();
                    reader.Close();
                    nstream.Close();
                    connection.Close();
                }
            }
        }

        private void GetMessageFromClient()
        {
            string msg;
            while (true)
            {
                if ((msg = reader.ReadString()) != null)
                {
                    //MessageBox.Show(msg);
                    if (msg == "yes")
                    {
                        wList.Add(writer);
                        rList.Add(reader);
                        conList.Add(connection);
                        Clients.Add(nstream);               

                        //  send to client _ to represent word length
                        writer.Write(client_num+"/"+string.Concat(arr_checkword));
                        client_num++;

                        if (client_num == int.Parse(textBox1.Text))
                        {
                            MessageBox.Show("game start");
                            readLetterFromClient.Start();                            
                        }

                    }
                    else MessageBox.Show("no");
                    //break;
                    return;
                }
            }
        }
        void select_word()
        {
            words.Clear();
            StreamReader reader2 = File.OpenText(@"..\..\..\words.txt");
            string input;

            while ((input = reader2.ReadLine()) != null)
            {
                if (int.Parse(input.Split('-')[1]) == comboBox1.SelectedIndex && input.Split('-')[2] == comboBox2.SelectedItem.ToString())
                {
                    words.Add(input.Split('-')[0]);
                }

            }

            reader2.Close();
            Random r = new Random();
            int index = r.Next(words.Count);
            SelectedWord = words[index];
            MessageBox.Show(SelectedWord);

            arr_checkword = new char[SelectedWord.Length];
            arr_checkword = Enumerable.Repeat('_', SelectedWord.Length).ToArray();
            // 
        }
        void LetterFromClient()
        {
            string msg; char ch; int flag;
            while (true)
            {
                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].DataAvailable)
                    {
                        msg = rList[i].ReadString();
                        //MessageBox.Show(i + "-----" + msg);
                        if(msg == "0") //
                        {
                            MessageBox.Show("client"+i+" leave ");
                            //closeConnection();
                            closeConnection(i);
                        }
                        else
                        {
                            //MessageBox.Show(msg);
                            ch = char.Parse(msg.ToLower());
                            flag = checkLetter(ch);
                            // // // // // get next client
                            int next_client;
                            if (i == client_num - 1)  //last client
                                next_client = 0;
                            else
                                next_client = i + 1;
                            // // // // // //
                            /// send the word to client 
                            sendMsg(i+"/"+ch + ","+string.Concat(arr_checkword)+","+next_client);
                                                       
                           if (flag == 1) // char is last char
                           {
                                MessageBox.Show($"client{i} win");

                                closeConnection();
                           }
                        }
                    }
                }


            }
           
        }
        //done
        int checkLetter(char ch)
        {
            int flag = 0;

            for (int i = 0; i < SelectedWord.Length; i++)
            {
                if (SelectedWord[i] == ch) { arr_checkword[i] = ch; }
            }

            
            if (string.Concat(arr_checkword) == SelectedWord)
            {

                flag = 1;
            }
           
            return flag;

        }
        
        private void close_Click(object sender, EventArgs e)
        {
            sendMsg("0");
            closeConnection();
        }
        void closeConnection()
        {
            for (int i=0; i<Clients.Count; i++)
            {
                wList[i].Close();
                rList[i].Close();
                conList[i].Close();
                Clients[i].Close();
            }
            wList.Clear();rList.Clear();conList.Clear();Clients.Clear();client_num = 0;
            comboBox1.Enabled = true; comboBox2.Enabled = true; textBox1.Enabled = true;
            start.Enabled = true;

            
        }
        //void closeConnection(int i)
        //{         
        //    wList[i].Close();
        //    rList[i].Close();
        //    conList[i].Close();
        //    Clients[i].Close();

        //    wList.RemoveAt(i);
        //    rList.RemoveAt(i);
        //    conList.RemoveAt(i);
        //    Clients.RemoveAt(i);
        //    client_num--;
        //}
        void closeConnection(int index)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                if (i != index)
                    wList[i].Write("0");                
            }
            closeConnection();
        }
        void sendMsg(string msg)
        {
            foreach (var w in wList)
            {
                w.Write(msg);
            }
        }
    }
}
