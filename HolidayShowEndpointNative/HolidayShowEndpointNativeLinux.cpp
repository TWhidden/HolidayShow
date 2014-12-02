#include "HolidayShowEndpointNativeLinux.h"

/*
	To test the library, include "HolidayShowEndpointNativeLinux.h" from an application project
	and call HolidayShowEndpointNativeLinuxTest().
	
	Do not forget to add the library to Project Dependencies in Visual Studio.
*/

static int s_Test = 0;

extern "C" int HolidayShowEndpointNativeLinuxTest();

int HolidayShowEndpointNativeLinuxTest()
{
	return ++s_Test;
}