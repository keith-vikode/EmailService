# Email Service
A common web-based API for sending emails, designed to be shared across multiple applications, and providing a central point for setting up templates, tracking messages and configuring transports.

The project consists of the following runnable components:

* `EmailService.Processor.Job` - dispatches queued email messages
* `EmailService.Web` - admin console, allowing users to set up applications, templates and transports
* `EmailService.Web.Api` - public API for sending messages

Also see the repository [EmailService.Client](https://sunbranding.visualstudio.com/EmailService/_git/EmailService.Client) for client implementations in .NET, CLI and PowerShell.

## Contacts

* [Keith Williams](kwilliams@sunbrandingsolutions.com) - architect and lead developer

## Requirements

* Visual Studio 2015
* Azure Storage Emulator 4.6+
* .NET Core 1.1
* SQL Server 2016, running as a default instance with integrated authentication

## Quick Start
Clone the project to your preferred local folder, then start a command window in the root folder of the repository.

The commands below will launch three command windows running the processor, website and API. Note that you must have all pre-requisites set up in order for this to work; if you have an unusual setup, you may need to configure specific user secrets for your machine.

### Windows
```
dotnet restore
installdb.cmd
runall.cmd
```

### Linux/OSX
```
dotnet restore
./installdb.sh
./runall.sh
```