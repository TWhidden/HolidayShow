// HolidayShowEndpointNative.cpp : Defines the entry point for the console application.
//

	#include "stdafx.h"

#ifdef _WIN32
	#include <winsock2.h>
#endif

#include <stdio.h>

#include <iostream>
#include <memory>
#include <functional>

#include "ProtocolHelper.h"
#include "Client.h"
#include "LibGpio.h"
#include "GpioPins.h"
#include <SocketHandler.h>
#include <tclap/SwitchArg.h>
#include <tclap/ValueArg.h>
#include <tclap/CmdLine.h>


int main(int argc, char** argv)
{
	// Wrap everything in a try block.  Do this every time, 
	// because exceptions will be thrown for problems.
	try {

		TCLAP::CmdLine cmd("Holiday Show Endpoint", ' ', "0.1");

		TCLAP::ValueArg<std::string> serverArg("s", "server", "Server Address", false, "10.64.128.75", "string");
		cmd.add(serverArg);
		TCLAP::ValueArg<int> portArg("p", "port", "Server Port", false, 5555, "int");
		cmd.add(portArg);
		TCLAP::ValueArg<int> deviceIdArg("d", "deviceid", "The correct device ID to tell the server what messages to be sent", true, -1, "int");
		cmd.add(deviceIdArg);
		TCLAP::ValueArg<bool> skipInitLights("x", "skipinit", "To improve startup time, set to true.  Warning, lights may be in unknown state (on or off)", false, false, "bool");
		cmd.add(skipInitLights); 


		// Parse the argv array.
		cmd.parse(argc, argv);

		// Pins available, in the vector the same way as the C# version
		HolidayShowEndpoint::PinMap pinsAvailble;
		pinsAvailble.push_back(HolidayShowEndpoint::BroadcomPinNumber::Four);
		pinsAvailble.push_back(HolidayShowEndpoint::BroadcomPinNumber::Fourteen);
		pinsAvailble.push_back(HolidayShowEndpoint::BroadcomPinNumber::Fithteen);
		pinsAvailble.push_back(HolidayShowEndpoint::BroadcomPinNumber::Seventeen);
		pinsAvailble.push_back(HolidayShowEndpoint::BroadcomPinNumber::Eighteen);
		pinsAvailble.push_back(HolidayShowEndpoint::BroadcomPinNumber::TwentySeven);
		pinsAvailble.push_back(HolidayShowEndpoint::BroadcomPinNumber::TwentyTwo);
		pinsAvailble.push_back(HolidayShowEndpoint::BroadcomPinNumber::TwentyThree);
		pinsAvailble.push_back(HolidayShowEndpoint::BroadcomPinNumber::TwentyFour);
		pinsAvailble.push_back(HolidayShowEndpoint::BroadcomPinNumber::TwentyFive);

		if (!skipInitLights.getValue())
		{
			HolidayShowEndpoint::LibGpio libGpio;

			for (HolidayShowEndpoint::BroadcomPinNumber pin : pinsAvailble)
			{
				libGpio.SetupChannel(pin, HolidayShowEndpoint::Direction::Output);
			}

			for (HolidayShowEndpoint::BroadcomPinNumber pin : pinsAvailble)
			{
				libGpio.OutputValue(pin, true);
			}

			for (HolidayShowEndpoint::BroadcomPinNumber pin : pinsAvailble)
			{
				libGpio.OutputValue(pin, false);
			}
		}

		// Create the handler
		SocketHandler socketHandler;
		// Create the client
		HolidayShowEndpoint::Client tcpClient(socketHandler, serverArg.getValue(), portArg.getValue(), deviceIdArg.getValue(), pinsAvailble);
		tcpClient.Start();
	

		timeval tv;
		tv.tv_usec = 1000;
		tv.tv_sec = 0;

		// Pump the messages
		while (true)
		{
			socketHandler.Select(&tv);
			tcpClient.ProcessTimers();
		}

		}
	catch (TCLAP::ArgException &e)  // catch any exceptions
	{
		cerr << "error: " << e.error() << " for arg " << e.argId() << std::endl;
	}

	return 0;
}

static void ResetLights()
{
	
}

