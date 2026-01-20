using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketServer
{
    public partial class ServerForm : Form
    {
        private ListBox clientsListBox;
        private TextBox messageTextBox;
        private Button sendButton;
        private Button startButton;
        private Label statusLabel;
        private TextBox portTextBox;

        private TcpListener server;
        private Dictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();
        private bool isRunning = false;

        public ServerForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Socket Server";
            this.Size = new System.Drawing.Size(500, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label portLabel = new Label();
            portLabel.Text = "Port:";
            portLabel.Location = new System.Drawing.Point(20, 20);
            portLabel.Size = new System.Drawing.Size(50, 20);

            portTextBox = new TextBox();
            portTextBox.Text = "8888";
            portTextBox.Location = new System.Drawing.Point(70, 18);
            portTextBox.Size = new System.Drawing.Size(100, 20);

            startButton = new Button();
            startButton.Text = "Start Server";
            startButton.Location = new System.Drawing.Point(180, 15);
            startButton.Size = new System.Drawing.Size(100, 25);
            startButton.Click += StartButton_Click;

            statusLabel = new Label();
            statusLabel.Text = "Server stopped";
            statusLabel.Location = new System.Drawing.Point(290, 20);
            statusLabel.Size = new System.Drawing.Size(180, 20);
            statusLabel.ForeColor = System.Drawing.Color.Red;

            Label clientsLabel = new Label();
            clientsLabel.Text = "Connected Clients:";
            clientsLabel.Location = new System.Drawing.Point(20, 55);
            clientsLabel.Size = new System.Drawing.Size(150, 20);

            clientsListBox = new ListBox();
            clientsListBox.Location = new System.Drawing.Point(20, 80);
            clientsListBox.Size = new System.Drawing.Size(440, 150);

            Label messageLabel = new Label();
            messageLabel.Text = "Message/Command to send:";
            messageLabel.Location = new System.Drawing.Point(20, 245);
            messageLabel.Size = new System.Drawing.Size(200, 20);

            messageTextBox = new TextBox();
            messageTextBox.Location = new System.Drawing.Point(20, 270);
            messageTextBox.Size = new System.Drawing.Size(340, 20);
            messageTextBox.Enabled = false;

            sendButton = new Button();
            sendButton.Text = "Send";
            sendButton.Location = new System.Drawing.Point(370, 268);
            sendButton.Size = new System.Drawing.Size(90, 25);
            sendButton.Enabled = false;
            sendButton.Click += SendButton_Click;

            Label infoLabel = new Label();
            infoLabel.Text = "Commands: ALERT:msg | SHUTDOWN | EXECUTE:cmd | BEEP | MSG:text";
            infoLabel.Location = new System.Drawing.Point(20, 305);
            infoLabel.Size = new System.Drawing.Size(440, 20);
            infoLabel.ForeColor = System.Drawing.Color.Blue;
            infoLabel.Font = new System.Drawing.Font(infoLabel.Font.FontFamily, 7.5f);

            this.Controls.Add(portLabel);
            this.Controls.Add(portTextBox);
            this.Controls.Add(startButton);
            this.Controls.Add(statusLabel);
            this.Controls.Add(clientsLabel);
            this.Controls.Add(clientsListBox);
            this.Controls.Add(messageLabel);
            this.Controls.Add(messageTextBox);
            this.Controls.Add(sendButton);
            this.Controls.Add(infoLabel);

            this.FormClosing += ServerForm_FormClosing;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                StartServer();
            }
            else
            {
                StopServer();
            }
        }

        private void StartServer()
        {
            try
            {
                int port = int.Parse(portTextBox.Text);
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                isRunning = true;

                statusLabel.Text = $"Server running on port {port}";
                statusLabel.ForeColor = System.Drawing.Color.Green;
                startButton.Text = "Stop Server";
                portTextBox.Enabled = false;
                messageTextBox.Enabled = true;
                sendButton.Enabled = true;

                Thread acceptThread = new Thread(AcceptClients);
                acceptThread.IsBackground = true;
                acceptThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting server: {ex.Message}", "Error");
            }
        }

        private void StopServer()
        {
            isRunning = false;
            server?.Stop();

            foreach (var client in clients.Values)
            {
                client?.Close();
            }
            clients.Clear();

            UpdateClientsList();
            statusLabel.Text = "Server stopped";
            statusLabel.ForeColor = System.Drawing.Color.Red;
            startButton.Text = "Start Server";
            portTextBox.Enabled = true;
            messageTextBox.Enabled = false;
            sendButton.Enabled = false;
        }

        private void AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    string clientId = ((IPEndPoint)client.Client.RemoteEndPoint).ToString();

                    clients[clientId] = client;
                    UpdateClientsList();

                    Thread clientThread = new Thread(() => HandleClient(clientId, client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch
                {
                    break;
                }
            }
        }

        private void HandleClient(string clientId, TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (isRunning && client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"Message from {clientId}: {message}", "Client Message");
                    });
                }
            }
            catch { }
            finally
            {
                clients.Remove(clientId);
                client.Close();
                UpdateClientsList();
            }
        }

        private void UpdateClientsList()
        {
            if (clientsListBox.InvokeRequired)
            {
                clientsListBox.Invoke((MethodInvoker)delegate { UpdateClientsList(); });
                return;
            }

            clientsListBox.Items.Clear();
            foreach (var clientId in clients.Keys)
            {
                clientsListBox.Items.Add(clientId);
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            string message = messageTextBox.Text;
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("Please enter a message or command to send.", "Warning");
                return;
            }

            if (clientsListBox.SelectedItem == null && clients.Count > 0)
            {
                MessageBox.Show("Please select a client from the list.", "Warning");
                return;
            }

            if (clients.Count == 0)
            {
                MessageBox.Show("No clients connected.", "Warning");
                return;
            }

            try
            {
                string selectedClient = clientsListBox.SelectedItem?.ToString();

                // Add command prefix if using commands
                string commandToSend = message;

                byte[] data = Encoding.UTF8.GetBytes(commandToSend);

                if (selectedClient != null && clients.ContainsKey(selectedClient))
                {
                    NetworkStream stream = clients[selectedClient].GetStream();
                    stream.Write(data, 0, data.Length);
                    MessageBox.Show($"Command/Message sent to {selectedClient}", "Success");
                }

                messageTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}", "Error");
            }
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopServer();
        }
    }
}
