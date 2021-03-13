using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretWordGameServer
{
    public partial class game : Form
    {
        Form1 obj;
        string word;
        char[] arr_checkword;
        Socket connection;
        NetworkStream nstream;
        BinaryWriter writer;
        BinaryReader reader;
        Button[] btns;
        string[] letters;
        // tasks
        Task readLetterFromClient;
        public game(Form1 obj)
        {
            InitializeComponent();
            //
            this.obj = obj;
            this.connection = obj.connection;
            this.nstream = obj.nstream;
            this.writer = obj.writer;
            this.reader = obj.reader;
            this.word = obj.SelectedWord;
            label2.Text = obj.categry_comboBox.SelectedItem.ToString();
            //

            btns = new Button[26] { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z }; //names of the panel buttons
            letters = new string[26] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            arr_checkword = new char[word.Length];
            arr_checkword = Enumerable.Repeat('_', word.Length).ToArray();
            
            readLetterFromClient = new Task(LetterFromClient);
            readLetterFromClient.Start();

            ////  send to client _ to represent word length
            //writer.Write(string.Concat(arr_checkword));

        }

        private void game_Load(object sender, EventArgs e)
        {
            label1.Text = string.Concat(arr_checkword);
        }

        void LetterFromClient()
        {
            while (true)
            {
                int flag;
                string msg; char ch;
                if ((msg = reader.ReadString()) != null)
                {
                    if (msg == "0")  // client close connection
                    {
                        MessageBox.Show("client leave \n you win");
                        closeConnection();
                    }
                    else
                    {
                        int index = Array.IndexOf(letters, msg);
                        btns[index].Enabled = false;
                        btns[index].BackColor = Color.Gainsboro;
                        //MessageBox.Show(msg);
                        ch = char.Parse(msg.ToLower());
                        flag = checkLetter(ch);
                                                
                        label1.Text = string.Concat(arr_checkword);
                        /// send the word to client 
                        writer.Write(label1.Text);                       
                       
                        if (flag == 0) {
                            panel1.Enabled = true;
                            label3.Text = "Your turn";
                        }
                            
                        else if(flag == 2) // char is last char
                        {
                            MessageBox.Show("you lose");

                            StreamReader scorereader = File.OpenText(@"..\..\..\score.txt");
                            string input = scorereader.ReadLine();
                            scorereader.Close();
                        
                            input = input.Split(' ')[0] + " " + (int.Parse(input.Split(' ')[1]) + 1);
                            File.WriteAllText(@"..\..\..\score.txt", input);
                            closeConnection();
                        }
                    }
                }
            }
        }
        //// function to check if letter in word or not
        int checkLetter(char ch)
        {
            int flag = 0;

            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] == ch) { arr_checkword[i] = ch; flag = 1; }
            }

            if (flag == 1)
            {
                if (string.Concat(arr_checkword) == word)
                {

                    flag = 2;
                }
            }
            return flag;

        }

        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;

            int index = Array.IndexOf(letters, btn.Text);
            btns[index].Enabled = false;
            btns[index].BackColor = Color.Gainsboro;

            if (e.Button == MouseButtons.Left)
            {

                char letter = char.Parse(btn.Text.ToLower());
                int flag = checkLetter(letter);
                label1.Text = string.Concat(arr_checkword);
                writer.Write(btn.Text +"," +label1.Text );

                if (flag == 0)
                {
                    panel1.Enabled = false;
                    label3.Text = "Client turn";
                }
                else if (flag == 2) //win 
                {
                    MessageBox.Show("you win");

                    StreamReader reader = File.OpenText(@"..\..\..\score.txt");
                    string input = reader.ReadLine();
                    reader.Close();
                    
                    input = (int.Parse(input.Split(' ')[0]) + 1) + " " + input.Split(' ')[1];
                    File.WriteAllText(@"..\..\..\score.txt", input);
                    
                    closeConnection();
                }
            }
        }

        private void button27_Click(object sender, EventArgs e)
        {
            writer.Write("0");
            closeConnection();
        }
        void closeConnection()
        {
            writer.Close();
            reader.Close();
            connection.Close();
            nstream.Close();
            this.obj.Visible = true;
            this.Close();
        }

        private void A_Click(object sender, EventArgs e)
        {

        }
    }
}
