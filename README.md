# Project Overview

This project consists of two main components, `ClamAVMicroservice` and `InsurancePolicyForm`. Each component serves a distinct purpose and is structured to facilitate specific functionalities within the broader application.

## ClamAVMicroservice

### Purpose
The `ClamAVMicroservice` is designed to integrate ClamAV antivirus functionalities into the application, providing scanning capabilities for uploaded files to ensure they are safe and virus-free.

### Key Contents
- **Controllers**: Handles HTTP requests and responses.
- **Models**: Defines the data structures used within the microservice.
- **Services**: Contains the business logic and interactions with ClamAV.
- **Configuration Files**: `appsettings.json` and `appsettings.Development.json` for environment-specific settings.
- **Program.cs**: Entry point of the microservice.

## InsurancePolicyForm

### Purpose
The `InsurancePolicyForm` component manages the user interface and logic for filling out and submitting insurance policy forms. It ensures that user data is collected accurately and securely.

### Key Contents
- **Controllers**: Manages form submissions and data processing.
- **Models**: Defines the data structures for insurance policies.
- **Services**: Contains business logic related to insurance policy handling.
- **Views**: Contains Razor views for rendering the user interface.
- **wwwroot**: Static files for the web application.
- **Program.cs**: Entry point of the application.

## Setup Instructions

1. **Clone the Repository**: Use `git clone` to download the project to your local machine.
2. **Restore Dependencies**: Run `dotnet restore` to install all necessary packages.
3. **Build the Solution**: Use `dotnet build` to compile the project.
4. **Run the Applications**: Navigate to each project directory and execute `dotnet run` to start the services.

## Usage Instructions

- **ClamAVMicroservice**: Ensure ClamAV is installed and configured. Use the service endpoints to scan files.
- **InsurancePolicyForm**: Access the web interface to fill out and submit insurance forms.

## Dependencies

- .NET Core SDK
- ClamAV
- Other dependencies as specified in the `.csproj` files.

## Contribution Guidelines

Contributions are welcome! Please fork the repository and submit a pull request with your changes. Ensure your code follows the project's style guidelines and includes appropriate tests.

## License

This project is licensed under the MIT License. See the LICENSE file for more details.

## ClamAV Setup Guide for .NET Microservices POC

### Overview

This guide walks you through setting up a portable ClamAV installation to work with your .NET microservices POC for virus scanning.

### Architecture

- **InsurancePolicyForm** (Port 5032): Handles file uploads and communicates with ClamAV microservice
- **ClamAVMicroservice** (Port 5259): Interfaces with ClamAV daemon via TCP
- **ClamAV Daemon** (Port 3310): Performs actual virus scanning

### Prerequisites

1. **Portable ClamAV**: Downloaded to `C:\Dev\Training\clamav-1.4.2.win.x64\clamav-1.4.2.win.x64`
2. **.NET 8 SDK**: Installed and working
3. **PowerShell**: For running setup commands
4. **Internet Connection**: For downloading virus definitions

### Step-by-Step Setup

#### Step 1: Project Structure Verification

Ensure your project structure looks like this:
```
C:\Dev\Training\ClamAV\
├── InsurancePolicyForm/
│   ├── Controllers/
│   │   └── FileUploadController.cs
│   ├── Program.cs
│   └── InsurancePolicyForm.csproj
├── ClamAVMicroservice/
│   ├── Controllers/
│   │   └── ScanController.cs
│   ├── Program.cs
│   └── ClamAVMicroservice.csproj
└── ClamAV-Setup-Guide.md (this file)

C:\Dev\Training\clamav-1.4.2.win.x64\clamav-1.4.2.win.x64\
├── clamd.exe
├── freshclam.exe
├── clamdscan.exe
├── conf_examples/
└── (other ClamAV files)
```

#### Step 2: Create ClamAV Configuration Files

##### 2.1 Create `clamd.conf` (Daemon Configuration)

Create file: `C:\Dev\Training\clamav-1.4.2.win.x64\clamav-1.4.2.win.x64\clamd.conf`

```ini
# ClamAV Daemon Configuration
# Minimal configuration for development use

# TCP port address
TCPSocket 3310

# TCP address to listen on
TCPAddr 127.0.0.1

# Run in foreground (don't become a daemon)
Foreground yes

# Verbose logging
LogVerbose yes

# Database directory location
DatabaseDirectory C:\Dev\Training\clamav-1.4.2.win.x64\clamav-1.4.2.win.x64\database

# Maximum file size to scan (25MB)
MaxFileSize 25M

# Maximum number of files to scan inside an archive
MaxFiles 10000

# Follow directory symlinks
FollowDirectorySymlinks no

# Follow regular file symlinks
FollowFileSymlinks no

# Scan Portable Executable files
ScanPE yes

# Maximum number of concurrent connections
MaxConnectionQueueLength 15

# Maximum number of threads
MaxThreads 20

# Self-check database integrity every hour
SelfCheck 3600

# Enable the built-in scanners
ScanArchive yes
ScanMail yes
ScanOLE2 yes
ScanPDF yes
ScanSWF yes
ScanHTML yes

# Heuristic detection
HeuristicScanPrecedence yes

# Structured data detection
StructuredDataDetection yes

# Command timeout (seconds)
CommandReadTimeout 5

# Don't log clean files
LogClean no

# Exit on out-of-memory
ExitOnOOM yes
```

##### 2.2 Create Database Directory

```powershell
mkdir "C:\Dev\Training\clamav-1.4.2.win.x64\clamav-1.4.2.win.x64\database"
```

### Step 3: Download Virus Definitions

#### 3.1 Setup FreshClam Configuration

```powershell
# Set ClamAV path variable
$clamPath = "C:\Dev\Training\clamav-1.4.2.win.x64\clamav-1.4.2.win.x64"

# Copy sample configuration
copy "$clamPath\conf_examples\freshclam.conf.sample" "$clamPath\freshclam.conf"

# Remove the Example line to enable the configuration
(Get-Content "$clamPath\freshclam.conf") | Where-Object { $_ -notlike "*Example*" } | Set-Content "$clamPath\freshclam.conf"

# Update database directory path
(Get-Content "$clamPath\freshclam.conf") -replace "#DatabaseDirectory.*", "DatabaseDirectory $clamPath\database" | Set-Content "$clamPath\freshclam.conf"
```

#### 3.2 Download Virus Definitions

```powershell
cd "C:\Dev\Training\clamav-1.4.2.win.x64\clamav-1.4.2.win.x64"
.\freshclam.exe --config-file="freshclam.conf" --verbose
```

**Expected Output:**
- Downloads ~8.7 million virus signatures
- Creates files like `daily.cvd`, `main.cvd`, `bytecode.cvd` in database folder
- Shows "Database test passed" messages

### Step 4: Start ClamAV Daemon

#### 4.1 Start the Daemon

```powershell
cd "C:\Dev\Training\clamav-1.4.2.win.x64\clamav-1.4.2.win.x64"
.\clamd.exe
```
      
**Expected Output:**
```
Loading signatures from database...
Loaded 8723268 signatures
TCP: Bound to address 127.0.0.1 on port 3310
Ready to accept connections
```

**Important:** Keep this terminal open - ClamAV daemon runs in foreground mode.

#### 4.2 Verify ClamAV is Running

In a new terminal:
```powershell
# Check if port 3310 is listening
netstat -an | findstr :3310

# Test with clamdscan
cd "C:\Dev\Training\clamav-1.4.2.win.x64\clamav-1.4.2.win.x64"
.\clamdscan.exe --version
```

### Step 5: Configure and Start .NET Microservices

#### 5.1 Start ClamAV Microservice

Open **Terminal 2**:
```powershell
cd C:\Dev\Training\ClamAV\ClamAVMicroservice
dotnet run --urls="http://localhost:5259"
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5259
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

#### 5.2 Start Insurance Policy Form

Open **Terminal 3**:
```powershell
cd C:\Dev\Training\ClamAV\InsurancePolicyForm
dotnet run --urls="http://localhost:5032"
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5032
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Step 6: Test the Complete Setup

#### 6.1 Health Check

Test ClamAV connectivity:
```powershell
curl http://localhost:5259/api/scan/health
```

**Expected Response:**
```json
{"status":"Healthy","message":"ClamAV daemon is responsive"}
```

#### 6.2 Swagger UI Testing

- **InsurancePolicyForm**: http://localhost:5032/swagger
- **ClamAVMicroservice**: http://localhost:5259/swagger

#### 6.3 File Upload Test

Create a test file and upload:
```powershell
# Create test file
echo "This is a test file" > test.txt

# Upload via curl
curl -X POST -F "file=@test.txt" http://localhost:5032/api/fileupload/upload

# Or using PowerShell
Invoke-RestMethod -Uri "http://localhost:5032/api/fileupload/upload" -Method Post -Form @{file=Get-Item "test.txt"}
```

**Expected Response (Clean File):**
```json
{
  "message": "File uploaded and scanned successfully.",
  "scanResult": "Clean"
}
```

#### 6.4 Test Virus Detection (EICAR)

Download EICAR test file from https://www.eicar.org/download-anti-malware-testfile/

```powershell
# Upload EICAR test file
curl -X POST -F "file=@eicar.com" http://localhost:5032/api/fileupload/upload
```

**Expected Response (Virus Detected):**
```
File contains a virus and was rejected. Scan result: Infected: ...EICAR-TEST-FILE...
```

## Key Configuration Details

### ClamAV Daemon Configuration
- **Port**: 3310 (TCP)
- **Address**: 127.0.0.1 (localhost only)
- **Mode**: Foreground (for development)
- **Database**: ~8.7M virus signatures

### .NET Microservices Configuration
- **InsurancePolicyForm**: Port 5032, handles file uploads
- **ClamAVMicroservice**: Port 5259, interfaces with ClamAV
- **Protocol**: HTTP (HTTPS disabled for development)

### File Flow
1. File uploaded to InsurancePolicyForm
2. InsurancePolicyForm calls ClamAVMicroservice
3. ClamAVMicroservice sends file to ClamAV daemon via TCP
4. ClamAV scans using INSTREAM protocol
5. Result propagated back through the chain

## Troubleshooting

### Common Issues

**1. Port 3310 Connection Refused**
- Check if ClamAV daemon is running
- Verify `clamd.conf` has correct TCP settings
- Check Windows firewall

**2. Swagger Won't Load**
- Use HTTP URLs (not HTTPS)
- Clear browser cache
- Check application is running on correct port

**3. "Missing argument for option" Error**
- Remove "Example" line from config files
- Check config file syntax
- Ensure paths don't have trailing spaces

**4. Virus Definitions Not Downloaded**
- Check internet connectivity
- Verify freshclam.conf configuration
- Check database directory permissions

### Verification Commands

```powershell
# Check ClamAV daemon status
netstat -an | findstr :3310

# Check virus definition count
cd "C:\Dev\Training\clamav-1.4.2.win.x64\clamav-1.4.2.win.x64"
.\clamscan.exe --version

# Test PING to ClamAV daemon
.\clamdscan.exe --ping

# Check database files
dir database\*.cvd
```

## Reference

### Useful ClamAV Commands
```powershell
# Scan a single file
.\clamdscan.exe file.txt

# Update virus definitions
.\freshclam.exe

# Test configuration
.\clamd.exe --config-test

# Check version info
.\clamd.exe --version
```

### API Endpoints

**InsurancePolicyForm (Port 5032):**
- `POST /api/fileupload/upload` - Upload and scan files

**ClamAVMicroservice (Port 5259):**
- `POST /api/scan/scan` - Direct file scanning
- `GET /api/scan/health` - ClamAV health check

## Summary

You now have a complete ClamAV POC with:
- ✅ ClamAV daemon with 8.7M virus signatures
- ✅ .NET microservices architecture
- ✅ File upload and virus scanning workflow
- ✅ Health monitoring and testing endpoints
- ✅ Swagger UI for easy testing

The setup demonstrates real-world virus scanning integration that can be extended for production use!

## What is ClamAV?

ClamAV is an open-source antivirus engine designed for detecting trojans, viruses, malware, and other malicious threats. It is widely used for email scanning, web scanning, and endpoint security. ClamAV is known for its versatility and ability to integrate with various systems, making it a popular choice for developers and system administrators.
