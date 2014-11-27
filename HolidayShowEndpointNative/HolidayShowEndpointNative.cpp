// HolidayShowEndpointNative.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "Person.h"
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
	ProtocolHelper h;

	ProtocolMessage m(MessageTypeIdEnum::DeviceId);

	auto byeBuffer  = h.Wrap(m);

	auto message = h.UnWrap(byeBuffer);

	SocketHandler ss;
	//"GET / HTTP/1.0\r\n\r\n"
	
	SocketHandler socketHandler;
	
	HolidayShowEndpoint::Client c(socketHandler);
	//c.SetDeleteByHandler();
	c.SetReconnect(true);
	c.Open("74.125.20.105", 80);
	c.Send("GET / HTTP/1.0\r\n\r\n");
	socketHandler.Add(&c);   // See below
	socketHandler.Select(1, 1);
	auto count = socketHandler.GetCount();
	cout << count << endl;
	
	while (count)
	{
		cout << count << endl;
		socketHandler.Select(1, 1);
		count = socketHandler.GetCount();
	}

	std::getchar();

	return 0;
}

