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

        public ComboBox categry_comboBox { get { return comboBox1; } }
        public Form1()
        {
            InitializeComponent();                         
            byte[] ip = new byte[] { 127, 0, 0, 1 };
            IPAddress publicAddress = new IPAddress(ip);
            server = new TcpListener(publicAddress, 2000);           
            listen = new Task(StartListen); 
            words = new List<string>();
            
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

            listen.Start(); //start listening to client request
           
        }

        private void StartListen()
        {
            while (true)
            {
                server.Start();
                connection = server.AcceptSocket();
                nstream = new NetworkStream(connection);
                writer = new BinaryWriter(nstream);
                reader = new BinaryReader(nstream);
                DialogResult dresult = MessageBox.Show("Client wants to play!", "Game Request", 
                                                           MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dresult == DialogResult.OK)
                {
                  
                    writer.Write("Category: " + comboBox1.SelectedItem + ", Level: " + comboBox2.SelectedItem);
                    readMessage = new Task(GetMessageFromClient);//to get message from client wether he accepts playing on 
                                                                 //the recieved category and level or not
                    readMessage.Start();

                }
                else
                {
                    writer.Write("no"); //server rejected the request

                    //close the connection with the client
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
                    if (msg == "yes") //client accepts to play with the recieved category and level
                    {
                 
                        words.Clear(); //array that hold the words of the selected category and level

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

                        //code to select random word from the words-array
                        Random r = new Random();
                        int index = r.Next(words.Count);
                        SelectedWord = words[index];

                        //send the length of word and category to client
                        writer.Write("length:" + SelectedWord.Length + ":" + comboBox1.SelectedItem);

                        //
                        this.Visible = false;
                        //MessageBox.Show(SelectedWord);
                        //open the game window
                        game g = new game(this);
                        g.ShowDialog();

                    }

                    else MessageBox.Show("Play request is cancelled");
                    return;
                }
            }
        }

        private void getScoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StreamReader scorereader = File.OpenText(@"..\..\..\score.txt");
            string input = scorereader.ReadLine();
            scorereader.Close();
            MessageBox.Show("Server Score: " + input.Split(' ')[0] + "\nClient Score: " + input.Split(' ')[1]);
        }

      
    }
}
