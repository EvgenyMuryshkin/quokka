# Quokka
Quokka public tools, CLI, drivers and test controllers

## Getting started

Create directory on your local computer and clone quokka repository into it
```
git clone https://github.com/EvgenyMuryshkin/quokka.git
```

Quokka CLI requires .NET Core SDK of version 2.0.0 or higher to be installed. 
You can download it from [official website](https://www.microsoft.com/net/download/windows)

Once you have installed .NET Core, open location "Testing\FunctionalTest" in command line and run dotnet
```
cd Testing\FunctionalTest
dotnet run -p ../../public/quokkacli --c UART_MaxSpeed
```
First run will configure runtime, download all dependencies and perform runtime optimizations, this will take some time.
Once it is all completed, CLI should run in watch mode with single controller UART_MaxSpeed.



