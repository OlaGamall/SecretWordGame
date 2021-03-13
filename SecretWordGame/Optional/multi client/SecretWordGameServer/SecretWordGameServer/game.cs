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
        //Form1 obj;
        string word;
        char[] arr_checkword;
        Socket connection;
        NetworkStream nstream;
        BinaryWriter writer;
        BinaryReader reader;
        // tasks
        Task readLetterFromClient;
        public game(Form1 obj)
        {
            InitializeComponent();

            this.connection = obj.connection;
            this.nstream = obj.nstream;
            this.writer = obj.writer;
            this.reader = obj.reader;
            this.word = obj.SelectedWord;
            label2.Text = obj.categry_comboBox.SelectedItem.ToString();

            arr_checkword = new char[word.Length];
            arr_checkword = Enumerable.Repeat('_', word.Length).ToArray();
            // 
            readLetterFromClient = new Task(LetterFromClient);
            readLetterFromClient.Start();

            //  send to client _ to represent word length
            writer.Write(string.Concat(arr_checkword));

        }

        private void game_Load(object sender, EventArgs e)
        {
            label1.Text = string.Concat(arr_checkword);
            panel1.Enabled = false;  //client start playing
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
                        //MessageBox.Show(msg);
                        ch = char.Parse(msg.ToLower());
                        flag = checkLetter(ch);
                                                
                        label1.Text = string.Concat(arr_checkword);
                        /// send the word to client 
                        writer.Write(label1.Text);
                        //if (flag == 1) // char in word                         
                          //  panel1.Enabled = false;
                       
                        if (flag == 0)
                            panel1.Enabled = true; 
                        else if(flag == 2) // char is last char
                        {
                            MessageBox.Show("you lose");
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
            
            if (e.Button == MouseButtons.Left)
            {

                char letter = char.Parse(btn.Text.ToLower());
                int flag = checkLetter(letter);
                label1.Text = string.Concat(arr_checkword);
                writer.Write(letter +"," +label1.Text );

                if (flag == 0) panel1.Enabled = false;
                //else if (flag == 1) panel1.Enabled = true;
                else if (flag == 2) //win 
                {
                    MessageBox.Show("you win");
                    closeConnection();
                }
            }
        }

        private void button27_Click(object sender, EventArgs e)
        {
            writer.Write("0");
            closeConnection();
        }
        void closeConnection ()
        {
            writer.Close();
            reader.Close();
            connection.Close();
            nstream.Close();
            this.Close();
        }
    }
}
