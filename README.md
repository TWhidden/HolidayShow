HolidayShow
===========

This project is designed to host any number of Raspberry Pi 2/3 running Windows IoT or <b>dotnetcore / ubuntu</b>.  Each RP will connect to a running windows server <b>[or linux server running .net core version]</b>, and receive its instructions real time. The server will control the client GPIOs or play audio files.  The client application will, on-demand, download the audio files from the server if it does not have them and store them in the server data store. There is also a WPF editor and SQL Database that hosts and manages all of the controls. New for 2018 - .net Core Web with React. <b>Dropping WPF dev and will build using this instead.</b>

This project has been expanding since 2013.   Used in conjunction with Solid State Relays as well as gas valves, we created a light and fire show using propane. Needless to say, it was the most insane house in the neighborhood.

Updated 2015/09 to support exclusively  Windows IoT projects (in this case, RasperryPi3, but nothing specifically written for it)
Update 2018/09 to now support .net Core.  Also will support Docker containers for rapid deployments.

<b>Compoents:</b>

<ol>
 <li>SQL Server 2008 or higher (Express is fine)</li>
 <li>Server software - Currently a console application</li>
 <li>Wpf Editor for managing GPIO and audio</li>
 <li>IoT Endpoint to be deployed on the Windows IoT device</li>
</ol>


<b>Dev Enviornment Required:</b>
<ol>
<li>Windows 10</li>
<li>Visual Studios 2015 (Community Edition works great)</li>
 <li>Raspberry Pi 2/3 or other supported Windows IoT devices OR <b>Ubuntu/Linux</b></li>
</ol>

<b>Setup:</b>
<ol>
<li>Clone this Repo</li>
<li>Compile the entire project</li>
<li>Setup a new database on your SQL Server such as HolidayShow</li>
<li>Grant Privs, update connection strings in Server and Editor app.config files</li>
<li>Run the Server console application (HolidayShowServer). This will auto-apply/update the database</li>
<li>Start a WinIoT device. *Currently, you can edit the host information on the RP user interface, or edit the XML file that is created on first-boot: \\WinIoTAddress\c$\Users\DefaultAccount\AppData\Local\Packages\HolidayShowEndPoint_7b8tge35t6pdg\LocalState\HolidayShowSettings.xml  
Also, it is handy if you modify the pre-processor directives available in the code</li>
<li>Once the WinIoT device connects, it will inform the server of all its available GPIO ports it can use. THese will be saved in the database</li>
<li>Open up the WPF Editor, click Devices, and click on the first Device.</li>
</ol>

<b>Examples in action</b>
<ol>
<li>2015 Halloween (Fire/Lights/Music): https://youtu.be/APfoZEe-HvU</li>
<li>2014 Surveillance Footage (there were 3k people that came): https://www.youtube.com/watch?v=7LtSyZfghlU</li>
<li>2013 First RP project (running Mono at the time) https://www.youtube.com/watch?v=RIQrfBfR6y4</li>
</ol>

<b>Special Note:</b>
<i>This is not enterprise grade software. This is get-it-done software.  I usually start one month before the upcoming holiday, and never have enough time to complete. Each year I add more.</i>

Screen Shots:
Device Editor. Here you will see all the device that are connected (or have connected). From here, you can select a device, and edit the pin patterns.  you can setup how long a pin in on for, and when it comes on.
![](https://raw.githubusercontent.com/TWhidden/HolidayShow/master/Images/DeviceEditor.png)

Effects Editor:
Witht he effects editor (new for 2015), you can program your own effects. This helps when have long running sets, but want the same patterns to be executing throughout the set. Such as a strobe light, or an always on light.
![](https://raw.githubusercontent.com/TWhidden/HolidayShow/master/Images/EffectsEditor.png)

Sets Editor:
This is the key spot, where you can configure all the devices to work in harmony. you can configure each device to turn on a pattern at any time. This set is the show that will be used.
![](https://raw.githubusercontent.com/TWhidden/HolidayShow/master/Images/SetsEditor.png)

Settings Editor:
What is nice, is you can configure when everything turns on, or off, as well as have a time for when the audio turns off. For example, if you have scarey music playing but do not want it playing pat 8:30pm, the lights will still continue to go.
you can also setup pins that should be monitored called "Danager Pins". This way when you are not around, you can just uncheck them, and they will not execute
![](https://raw.githubusercontent.com/TWhidden/HolidayShow/master/Images/ServerSettingsEditor.png)

Audio Editor:
The system will scan a directory and keep track of what files you have made available. Once the database has them, it knows how long each audio file is, and where the server can get the file.  If the WinIoT does not have the file, it will be auto-downloaded from the server over TCP
![](https://raw.githubusercontent.com/TWhidden/HolidayShow/master/Images/AudioEditor.png)

Server Image:
This is the console application that is running. The Console can later be re-created as a windows service. At this time, it is not.
![](https://raw.githubusercontent.com/TWhidden/HolidayShow/master/Images/ServerRunning.jpg)
