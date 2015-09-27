HolidayShow
===========

C# / Mono project to run Raspberry Pi or Beagle bone blacks from a central machine controlling GPIOs.

This project has been expanding since 2013.   Used in conjuction with Solid State Relays as well
as gas valves, we created a light and fire show using propane.   Needless to say, it was the most insane house in the hood.

Updated 2015/09 to support Windows IoT on Raspberry Pi 2 devices.

Compoents:

1.) Server Compoent that handles commucnatino with 1-n IoT devices. This server requires SQL Server 2008 or higher (Express is fine) / Windows
2.) The Editor built in WPF, so windows is required.
3.) The EndPoint is available for Linux Raspberry Pi (v1) or Windows IoT Raspberry Pi 2 - Note, the Linux one will not be managed by me anymore. Only available for historic purposes

Dev Enviornment Required:
1.) Visual Studios 2015 (Community Edition is fine)
2.) Raspberry Pi 2 or other supported Windows IoT devices

Setup:

Once you have everything compiled, you will need to setup one of the devices to connect inwards. 
There is a user interface or you can modify the pre-processor directives that are hard coded. Handy if you dont have a monitor 

Once the device connects in to the running server, it will add itself by its ID number and available GPIO pins will be populated in the database

Open the editor, and you will be able to create patterns for each raspberry pi, and then assign those patterns to sets. The patterns are reusable, so you can make 
sets with many different patterns. 

You can also hook audio up, and have it play out the audio jacks.

Being that this is not some really nice installer, you will have to play with it. I am more then willing to give you some help if you are interested however. 

Open up a message under the discussions and I will do my best to help.

