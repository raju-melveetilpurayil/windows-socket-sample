using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketClient
{
    public class ClientForm : Form
    {
        private TextBox serverTextBox;
        private TextBox portTextBox;
        private Button connectButton;
        private TextBox messageTextBox;
        private Button sendButton;
        private TextBox chatTextBox;
        private Label statusLabel;

        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected = false;

        public ClientForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Socket Client";
            this.Size = new System.Drawing.Size(500, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label serverLabel = new Label();
            serverLabel.Text = "Server IP:";
            serverLabel.Location = new System.Drawing.Point(20, 20);
            serverLabel.Size = new System.Drawing.Size(70, 20);

            serverTextBox = new TextBox();
            serverTextBox.Text = "127.0.0.1";
            serverTextBox.Location = new System.Drawing.Point(95, 18);
            serverTextBox.Size = new System.Drawing.Size(120, 20);

            Label portLabel = new Label();
            portLabel.Text = "Port:";
            portLabel.Location = new System.Drawing.Point(225, 20);
            portLabel.Size = new System.Drawing.Size(40, 20);

            portTextBox = new TextBox();
            portTextBox.Text = "8888";
            portTextBox.Location = new System.Drawing.Point(270, 18);
            portTextBox.Size = new System.Drawing.Size(70, 20);

            connectButton = new Button();
            connectButton.Text = "Connect";
            connectButton.Location = new System.Drawing.Point(350, 15);
            connectButton.Size = new System.Drawing.Size(110, 25);
            connectButton.Click += ConnectButton_Click;

            statusLabel = new Label();
            statusLabel.Text = "Disconnected";
            statusLabel.Location = new System.Drawing.Point(20, 50);
            statusLabel.Size = new System.Drawing.Size(440, 20);
            statusLabel.ForeColor = System.Drawing.Color.Red;

            Label chatLabel = new Label();
            chatLabel.Text = "Messages:";
            chatLabel.Location = new System.Drawing.Point(20, 80);
            chatLabel.Size = new System.Drawing.Size(150, 20);

            chatTextBox = new TextBox();
            chatTextBox.Multiline = true;
            chatTextBox.ReadOnly = true;
            chatTextBox.ScrollBars = ScrollBars.Vertical;
            chatTextBox.Location = new System.Drawing.Point(20, 105);
            chatTextBox.Size = new System.Drawing.Size(440, 200);

            Label messageLabel = new Label();
            messageLabel.Text = "Your message:";
            messageLabel.Location = new System.Drawing.Point(20, 320);
            messageLabel.Size = new System.Drawing.Size(150, 20);

            messageTextBox = new TextBox();
            messageTextBox.Location = new System.Drawing.Point(20, 345);
            messageTextBox.Size = new System.Drawing.Size(340, 20);
            messageTextBox.Enabled = false;
            messageTextBox.KeyPress += MessageTextBox_KeyPress;

            sendButton = new Button();
            sendButton.Text = "Send";
            sendButton.Location = new System.Drawing.Point(370, 343);
            sendButton.Size = new System.Drawing.Size(90, 25);
            sendButton.Enabled = false;
            sendButton.Click += SendButton_Click;

            this.Controls.Add(serverLabel);
            this.Controls.Add(serverTextBox);
            this.Controls.Add(portLabel);
            this.Controls.Add(portTextBox);
            this.Controls.Add(connectButton);
            this.Controls.Add(statusLabel);
            this.Controls.Add(chatLabel);
            this.Controls.Add(chatTextBox);
            this.Controls.Add(messageLabel);
            this.Controls.Add(messageTextBox);
            this.Controls.Add(sendButton);

            this.FormClosing += ClientForm_FormClosing;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                ConnectToServer();
            }
            else
            {
                Disconnect();
            }
        }

        private void ConnectToServer()
        {
            try
            {
                string server = serverTextBox.Text;
                int port = int.Parse(portTextBox.Text);

                client = new TcpClient();
                client.Connect(server, port);
                stream = client.GetStream();
                isConnected = true;

                statusLabel.Text = $"Connected to {server}:{port}";
                statusLabel.ForeColor = System.Drawing.Color.Green;
                connectButton.Text = "Disconnect";
                serverTextBox.Enabled = false;
                portTextBox.Enabled = false;
                messageTextBox.Enabled = true;
                sendButton.Enabled = true;

                AppendMessage("Connected to server successfully!");

                Thread receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error");
            }
        }

        private void Disconnect()
        {
            isConnected = false;
            stream?.Close();
            client?.Close();

            statusLabel.Text = "Disconnected";
            statusLabel.ForeColor = System.Drawing.Color.Red;
            connectButton.Text = "Connect";
            serverTextBox.Enabled = true;
            portTextBox.Enabled = true;
            messageTextBox.Enabled = false;
            sendButton.Enabled = false;

            AppendMessage("Disconnected from server.");
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (isConnected && client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessCommand(message);
                }
            }
            catch { }
            finally
            {
                if (isConnected)
                {
                    this.Invoke((MethodInvoker)delegate { Disconnect(); });
                }
            }
        }

        private void ProcessCommand(string message)
        {
            // Check if message is a command
            if (message.Contains(":"))
            {
                string[] parts = message.Split(new[] { ':' }, 2);
                string command = parts[0].ToUpper();
                string parameter = parts.Length > 1 ? parts[1] : "";

                switch (command)
                {
                    case "ALERT":
                        this.Invoke((MethodInvoker)delegate
                        {
                            MessageBox.Show(parameter, "Server Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            AppendMessage($"[ALERT] {parameter}");
                        });
                        break;

                    case "EXECUTE":
                        ExecuteCommand(parameter);
                        break;

                    case "MSG":
                        AppendMessage($"Server: {parameter}");
                        break;

                    default:
                        // Not a recognized command, treat as regular message
                        AppendMessage($"Server: {message}");
                        break;
                }
            }
            else if (message.ToUpper() == "SHUTDOWN")
            {
                AppendMessage("[COMMAND] Shutdown received from server");
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show("Server requested shutdown", "Shutdown", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Application.Exit();
                });
            }
            else if (message.ToUpper() == "BEEP")
            {
                System.Media.SystemSounds.Beep.Play();
                AppendMessage("[COMMAND] Beep executed");
            }
            else
            {
                // Regular message
                AppendMessage($"Server: {message}");
            }
        }

        private void ExecuteCommand(string command)
        {
            try
            {
                AppendMessage($"[EXECUTE] Running command: {command}");

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = $"/c {command}";
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo = psi;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                    AppendMessage($"[OUTPUT] {output}");

                if (!string.IsNullOrEmpty(error))
                    AppendMessage($"[ERROR] {error}");

                AppendMessage($"[EXECUTE] Command completed");
            }
            catch (Exception ex)
            {
                AppendMessage($"[ERROR] Failed to execute: {ex.Message}");
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void MessageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                SendMessage();
            }
        }

        private void SendMessage()
        {
            string message = messageTextBox.Text;
            if (string.IsNullOrWhiteSpace(message)) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                AppendMessage($"You: {message}");
                messageTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}", "Error");
                Disconnect();
            }
        }

        private void AppendMessage(string message)
        {
            if (chatTextBox.InvokeRequired)
            {
                chatTextBox.Invoke((MethodInvoker)delegate { AppendMessage(message); });
                return;
            }

            chatTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }

        
    }
}
