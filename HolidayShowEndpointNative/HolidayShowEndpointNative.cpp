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
using namespace std;
using namespace HolidayShowLib;

int _tmain(int argc, _TCHAR* argv[])
{

	ProtocolHelper h;

	ProtocolMessage m(MessageTypeIdEnum::DeviceId);

	auto byeBuffer  = h.Wrap(m);

	auto message = h.UnWrap(byeBuffer);
	
	return 0;
}

