HolidayShow
===========

This project is designed to host any number of Raspberry Pi 2 running Windows IoT.  Each RP2 will connect to a running windows server, and receive its instructions real time. The server will control the client GPIOs or play audio files.  The client application will, on-demand, download the audio files from the server if it does not have them and store them in the Windows IoT data store. There is also a WPF editor and SQL Database that hosts and manages all of the controls.

This project has been expanding since 2013.   Used in conjunction with Solid State Relays as well as gas valves, we created a light and fire show using propane.   Needless to say, it was the most insane house in the neighborhood.

Updated 2015/09 to support exclusively  Windows IoT projects (in this case, RasperryPi2, but nothing specifically written for it)

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
<li>Raspberry Pi 2 or other supported Windows IoT devices</li>
</ol>

<b>Setup:</b>
<ol>
<li>Clone this Repo</li>
<li>Compile the entire project</li>
<li>Setup a new database on your SQL Server such as HolidayShow</li>
<li>Apply SQL Script available in \HolidayShow.Data\Sql\HolidayShow.sql</li>
<li>Grant Privs, update connection strings in Server and Editor app.config files</li>
<li>Run the Server console application (HolidayShowServer)</li>
<li>Start a WinIoT device. *Currently, you can edit the host information on the RP user interface, or edit the XML file that is created on first-boot: \\WinIoTAddress\c$\Users\DefaultAccount\AppData\Local\Packages\HolidayShowEndPoint_7b8tge35t6pdg\LocalState\HolidayShowSettings.xml  
Also, it is handy if you modify the pre-processor directives available in the code</li>
<li>Once the WinIoT device connects, it will inform the server of all its available GPIO ports it can use. THese will be saved in the database</li>
<li>Open up the WPF Editor, click Devices, and click on the first Device.</li>
</ol>

<b>Special Note:</b>
<i>This is not enterprise grade software. This is get-it-done software.  I usually start one month before the upcoming holiday, and never have enough time to complete. Each year I add more.</i>

Screen Shots:
