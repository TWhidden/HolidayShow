// HolidayShowEndpointNative.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <stdio.h>
#include <winsock2.h>
#include <iostream>
#include <memory>
#include <functional>

#include "ProtocolHelper.h"
#include "Client.h"
#include <SocketHandler.h>


using namespace std;
using namespace HolidayShowLib;


int _tmain(int argc, _TCHAR* argv[])
{
	//ProtocolHelper h;

	//ProtocolMessage m(MessageTypeIdEnum::);

	//auto byeBuffer  = h.Wrap(m);

	//auto message = h.UnWrap(byeBuffer);

	SocketHandler ss;
	//"GET / HTTP/1.0\r\n\r\n"
	
	SocketHandler socketHandler;
	
	HolidayShowEndpoint::Client c(socketHandler,"10.64.128.75", 5555);
	c.Start();
	
	// Pump the messages
	while (true)
	{
		socketHandler.Select(1, 0);
	}

	return 0;
}

