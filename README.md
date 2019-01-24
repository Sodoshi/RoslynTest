# RoslynTest
Call Roslyn and run code on-the-fly in a seperate app domain

A new App Domain is created with a random GUID. (See below in orange)

Assemblies in the current domain are converted into byte arrays (using AssemblyHash) and passed via a proxy into the new appdomain and loaded. (With a blacklist for Roslyn assemblies)

Code is compiled by Roslyn and Emitted to a byte array, and loaded by the proxy. (See below in red)

The console output of the second app domain is redirected to a stringbuilder, and returned. (See below in green)

The app domain can be unloaded, and all roslyn assemblies with it. (See below in red)

The command object is marked Serializable, and is passed by reference to and from the second app domain, allowing for two-way communication other than via Console output.

![alt text](https://raw.githubusercontent.com/Sodoshi/RoslynTest/master/RoslynTest.png)
