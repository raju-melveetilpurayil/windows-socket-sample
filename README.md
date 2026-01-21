# C# Socket Server-Client Application

A Windows Forms-based TCP socket application that enables real-time communication between a server and multiple clients with support for custom commands and remote execution.

## 📋 Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Usage](#usage)
- [Command Reference](#command-reference)
- [Architecture](#architecture)
- [Security Considerations](#security-considerations)
- [Screenshots](#screenshots)
- [Contributing](#contributing)
- [License](#license)

## ✨ Features

### Server Features
- Start/stop TCP server on configurable port
- View all connected clients in real-time with IP addresses and ports
- Send messages to individual selected clients
- Send custom commands for remote execution
- Automatic client connection/disconnection handling
- Multi-threaded client management
- Visual status indicators

### Client Features
- Connect to remote server via IP and port
- Send messages to server
- Receive and display timestamped messages
- Execute custom commands received from server
- Support for multiple command types (alerts, system commands, etc.)
- Automatic reconnection handling
- Clean disconnect functionality

### Command System
- **ALERT**: Display popup notifications
- **MSG**: Send regular chat messages
- **EXECUTE**: Run CMD commands remotely
- **SHUTDOWN**: Close client application
- **BEEP**: Play system sound

## 🔧 Prerequisites

- Windows OS (Windows 7 or later)
- .NET Framework 4.7.2 or higher / .NET 6.0 or higher
- Visual Studio 2019 or later (for development)

## 📥 Installation

### Option 1: Clone and Build

```bash
# Clone the repository
git clone https://github.com/raju-melveetilpurayil/socket-server-client.git

# Navigate to the project directory
cd socket-server-client

# Open the solution in Visual Studio
start SocketServerClient.sln
```

### Option 2: Download Release

1. Go to [Releases](https://github.com/raju-melveetilpurayil/socket-server-client/releases)
2. Download the latest version
3. Extract the ZIP file
4. Run `SocketServer.exe` and `SocketClient.exe`

## 🚀 Usage

### Running the Server

1. Launch `SocketServer.exe`
2. Configure the port (default: 8888)
3. Click **Start Server**
4. The status will change to "Server running on port XXXX"
5. Connected clients will appear in the list automatically

### Running the Client

1. Launch `SocketClient.exe`
2. Enter the server IP address (use `127.0.0.1` for local testing)
3. Enter the port number (must match server port)
4. Click **Connect**
5. Once connected, you can send messages or receive commands

### Sending Messages/Commands from Server

1. Select a client from the connected clients list
2. Type your message or command in the text box
3. Click **Send**

**Example:**
```
MSG:Hello, this is a notification
ALERT:Important system update!
EXECUTE:notepad
BEEP
```

## 📖 Command Reference

| Command | Syntax | Description | Example |
|---------|--------|-------------|---------|
| **MSG** | `MSG:message` | Send a regular chat message | `MSG:Hello from server!` |
| **ALERT** | `ALERT:message` | Display popup alert on client | `ALERT:Server maintenance in 5 minutes` |
| **EXECUTE** | `EXECUTE:command` | Execute CMD command on client | `EXECUTE:notepad` or `EXECUTE:dir C:\` |
| **BEEP** | `BEEP` | Play system beep sound | `BEEP` |
| **SHUTDOWN** | `SHUTDOWN` | Close the client application | `SHUTDOWN` |
| **Plain Text** | `your message` | Displays as regular message | `This is a simple message` |

### EXECUTE Command Examples

```bash
# Open applications
EXECUTE:notepad
EXECUTE:calc
EXECUTE:mspaint

# System commands
EXECUTE:dir C:\
EXECUTE:ipconfig
EXECUTE:tasklist
EXECUTE:systeminfo

# Create files
EXECUTE:echo Hello World > test.txt

# Network commands
EXECUTE:ping google.com
EXECUTE:netstat -an
```

## 🏗️ Architecture

### Project Structure

```
SocketServerClient/
├── SocketServer/
│   ├── ServerForm.cs          # Main server logic
│   ├── ServerForm.Designer.cs # UI designer code
│   └── Program.cs             # Entry point
├── SocketClient/
│   ├── ClientForm.cs          # Main client logic
│   ├── ClientForm.Designer.cs # UI designer code
│   └── Program.cs             # Entry point
└── README.md
```

### Communication Flow

```
Server                          Client
  |                               |
  |-- TCP Listener (Port 8888) --|
  |                               |
  |<-------- Connect -------------|
  |                               |
  |-- Add to Client List -------->|
  |                               |
  |-- Send Command: ALERT:Hi ---->|
  |                               |
  |                               |-- Process Command
  |                               |-- Show Alert Dialog
  |                               |
  |<----- Response Message -------|
```

### Threading Model

- **Server**: 
  - Main UI thread for form handling
  - Dedicated thread for accepting new connections
  - Separate thread for each connected client
  
- **Client**:
  - Main UI thread for form handling
  - Background thread for receiving messages

## 🔒 Security Considerations

⚠️ **Important Security Notes:**

1. **EXECUTE Command Risk**: The `EXECUTE` command can run ANY system command. This is a significant security risk if exposed to untrusted networks.

2. **Recommendations for Production**:
   - Implement authentication (username/password)
   - Use SSL/TLS encryption for data transmission
   - Whitelist allowed commands for EXECUTE
   - Add command confirmation prompts
   - Implement access control lists (ACL)
   - Log all commands for audit trails

3. **Network Security**:
   - Use this only on trusted networks
   - Configure firewall rules appropriately
   - Consider VPN for remote connections

4. **Code Improvements for Production**:
```csharp
// Example: Command whitelist
private bool IsCommandAllowed(string command)
{
    string[] allowedCommands = { "notepad", "calc", "mspaint" };
    return allowedCommands.Any(cmd => command.ToLower().StartsWith(cmd));
}
```

## 📸 Screenshots

### Server Interface
```
┌─────────────────────────────────────────┐
│ Socket Server                      [_][□][X]│
├─────────────────────────────────────────┤
│ Port: [8888]  [Start Server]            │
│ Status: Server running on port 8888     │
│                                          │
│ Connected Clients:                       │
│ ┌──────────────────────────────────────┐│
│ │ 192.168.1.100:54321                  ││
│ │ 192.168.1.101:54322                  ││
│ └──────────────────────────────────────┘│
│                                          │
│ Message/Command to send:                 │
│ [ALERT:Hello Client!        ] [Send]    │
│ Commands: ALERT:msg | SHUTDOWN | ...     │
└─────────────────────────────────────────┘
```

### Client Interface
```
┌─────────────────────────────────────────┐
│ Socket Client                      [_][□][X]│
├─────────────────────────────────────────┤
│ Server IP: [127.0.0.1] Port: [8888]     │
│                          [Connect]       │
│ Status: Connected to 127.0.0.1:8888     │
│                                          │
│ Messages:                                │
│ ┌──────────────────────────────────────┐│
│ │[10:30:15] Connected successfully     ││
│ │[10:30:45] Server: Hello!             ││
│ │[10:31:00] [ALERT] Important message  ││
│ └──────────────────────────────────────┘│
│                                          │
│ Your message:                            │
│ [Type here...              ] [Send]     │
└─────────────────────────────────────────┘
```

## 🛠️ Development

### Building from Source

1. Open `SocketServerClient.sln` in Visual Studio
2. Restore NuGet packages (if any)
3. Build the solution (Ctrl + Shift + B)
4. Run the projects:
   - Set `SocketServer` as startup project and run
   - Set `SocketClient` as startup project and run

### Testing Locally

1. Start the server application
2. Click "Start Server"
3. Start one or more client applications
4. Connect clients to `127.0.0.1:8888`
5. Test commands from server to clients

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines

- Follow C# coding conventions
- Add XML documentation for public methods
- Test thoroughly before submitting PR
- Update README if adding new features

## 📝 Changelog

### Version 1.0.0 (2026-01-21)
- Initial release
- Basic server-client communication
- Command system implementation
- Support for multiple clients
- EXECUTE, ALERT, MSG, BEEP, SHUTDOWN commands

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Raju Melveetilpurayil** - *Initial work* - https://github.com/raju-melveetilpurayil

## 🙏 Acknowledgments

- Built with Windows Forms and .NET
- Uses TCP sockets for reliable communication
- Inspired by remote administration tools

## 📞 Support

If you encounter any issues or have questions:

- Open an [Issue](https://github.com/raju-melveetilpurayil/socket-server-client/issues)
- Check existing issues for solutions

## ⚠️ Disclaimer

This software is provided for educational purposes. The authors are not responsible for any misuse or damage caused by this application. Always use responsibly and only on networks you own or have permission to use.
