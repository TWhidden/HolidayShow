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

    auto scheduled_time = _startTime + _executeIn;
    auto next_ready_val = scheduled_time - high_resolution_clock::now();

    if (next_ready_val <= milliseconds(0))
    {
        _executed = true;

        _toExecute();

        return true;
    }
}

std::chrono::milliseconds HolidayShowEndpoint::DelayExecutionContainer::next_ready()
{
    auto scheduled_time =  _startTime + _executeIn;
    auto next_ready_val =  scheduled_time - high_resolution_clock::now();
    
    if (next_ready_val < milliseconds(0))
    {
        return milliseconds(0);
    }

    return std::chrono::duration_cast<milliseconds>(next_ready_val);
}
