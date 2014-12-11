#include "stdafx.h"
#include "DelayExecutionContainer.h"

HolidayShowEndpoint::DelayExecutionContainer::DelayExecutionContainer(milliseconds executeIn, std::function<void()> functionToExecute)
{
	_startTime = high_resolution_clock::now();
	_executeIn = executeIn;
	_toExecute = functionToExecute;

}

bool HolidayShowEndpoint::DelayExecutionContainer::ExecuteIfReady()
{
	if (_executed) return false;

	if ((high_resolution_clock::now() - _startTime) > _executeIn)
	{
		_executed = true;

		_toExecute();

		return true;
	}
}