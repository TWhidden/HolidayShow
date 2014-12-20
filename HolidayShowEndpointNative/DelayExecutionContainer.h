#pragma once

#include <chrono>
#include <functional>

using namespace std::chrono;

namespace HolidayShowEndpoint
{
	class LibGpio;

	class DelayExecutionContainer
	{

	private:

		high_resolution_clock::time_point _startTime;

		milliseconds _executeIn;

		bool _executed = false;

		std::function<void()> _toExecute;

	public:

		DelayExecutionContainer(milliseconds executeIn, std::function<void()> functionToExecute);
		
        std::chrono::milliseconds next_ready();
		bool ExecuteIfReady();
		

	};

};