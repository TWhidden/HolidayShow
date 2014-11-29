#pragma once

#include <string>
#include <unordered_map>
#include <iostream>
#include <map>
#include "GpioPins.h"

//-----------------------------------------------------------------------
// <copyright file="LibGpio.cs" company="Andrew Bradford">
//     Copyright (C) 2012 Andrew Bradford
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
//     associated documentation files (the "Software"), to deal in the Software without restriction, 
//     including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//     and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject 
//     to the following conditions:
//     
//     The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//     
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//     WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//     COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//     ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------


namespace HolidayShowEndpoint
{
	

	/// <summary>
	/// Library for interfacing with the Raspberry Pi GPIO ports. 
	/// This class is implemented as a singleton - no attempt should be made to instantiate it
	/// </summary>
	class LibGpio
	{


	public:
		LibGpio();
		~LibGpio();

	private:
		bool privateTestMode = false;

			/// <summary>
		/// Stores the configured directions of the GPIO ports
		/// </summary>
		std::map<BroadcomPinNumber, Direction> directions;

		/// <summary>
		/// Gets the class instance 
		/// </summary>
	public:
		//static LibGpio *getGpio() const;
	

		/// <summary>
		/// Gets or sets a value indicating whether the library should be used in test mode.
		/// Note: If running under Windows or Mac OSx, test mode is assumed.
		/// </summary>
		const bool &getTestMode() const;
		void setTestMode(const bool &value);

		/// <summary>
		/// Configures a GPIO channel for use
		/// </summary>
		/// <param name="pinNumber">The Raspberry Pi pin number to configure</param>
		/// <param name="direction">The direction to configure the pin for</param>
		void SetupChannel(RaspberryPinNumber pinNumber, Direction direction);

		/// <summary>
		/// Configures a GPIO channel for use
		/// </summary>
		/// <param name="pinNumber">The physical pin number to configure</param>
		/// <param name="direction">The direction to configure the pin for</param>
		void SetupChannel(PhysicalPinNumber pinNumber, Direction direction);

		/// <summary>
		/// Configures a GPIO channel for use
		/// </summary>
		/// <param name="pinNumber">The Broadcom pin number to configure</param>
		/// <param name="direction">The direction to configure the pin for</param>
		void SetupChannel(BroadcomPinNumber pinNumber, Direction direction);

		/// <summary>
		/// Outputs a value to a GPIO pin
		/// </summary>
		/// <param name="pinNumber">The pin number to use</param>
		/// <param name="value">The value to output</param>
		/// <exception cref="InvalidOperationException">
		/// Thrown when an attempt to use an incorrectly configured channel is made
		/// </exception>
		void OutputValue(RaspberryPinNumber pinNumber, bool value);

		/// <summary>
		/// Outputs a value to a GPIO pin
		/// </summary>
		/// <param name="pinNumber">The pin number to use</param>
		/// <param name="value">The value to output</param>
		/// <exception cref="InvalidOperationException">
		/// Thrown when an attempt to use an incorrectly configured channel is made
		/// </exception>
		void OutputValue(PhysicalPinNumber pinNumber, bool value);

		/// <summary>
		/// Outputs a value to a GPIO pin
		/// </summary>
		/// <param name="pinNumber">The pin number to use</param>
		/// <param name="value">The value to output</param>
		/// <exception cref="InvalidOperationException">
		/// Thrown when an attempt to use an incorrectly configured channel is made
		/// </exception>
		void OutputValue(BroadcomPinNumber pinNumber, bool value);

		/// <summary>
		/// Reads a value form a GPIO pin
		/// </summary>
		/// <param name="pinNumber">The pin number to read</param>
		/// <returns>The value at that pin</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when an attempt to use an incorrectly configured channel is made
		/// </exception>
		bool ReadValue(RaspberryPinNumber pinNumber);

		/// <summary>
		/// Reads a value form a GPIO pin
		/// </summary>
		/// <param name="pinNumber">The pin number to read</param>
		/// <returns>The value at that pin</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when an attempt to use an incorrectly configured channel is made
		/// </exception>
		bool ReadValue(PhysicalPinNumber pinNumber);

		/// <summary>
		/// Reads a value form a GPIO pin
		/// </summary>
		/// <param name="pinNumber">The pin number to read</param>
		/// <returns>The value at that pin</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when an attempt to use an incorrectly configured channel is made
		/// </exception>
		bool ReadValue(BroadcomPinNumber pinNumber);

		/// <summary>
		/// Gets the Broadcom GPIO pin number form the Raspberry Pi pin number
		/// </summary>
		/// <param name="pinNumber">The Raspberry Pi pin number</param>
		/// <returns>The equivalent Broadcom ID</returns>
	private:
		BroadcomPinNumber ConvertToBroadcom(RaspberryPinNumber pinNumber);

		/// <summary>
		/// Gets the Broadcom GPIO pin number form the physical pin number
		/// </summary>
		/// <param name="pinNumber">The physical pin number</param>
		/// <returns>The equivalent Broadcom ID</returns>
		BroadcomPinNumber ConvertToBroadcom(PhysicalPinNumber pinNumber);

		/// <summary>
		/// Unexports a given GPIO pin number
		/// </summary>
		/// <param name="pinNumber">The pin number to unexport</param>
		void UnExport(BroadcomPinNumber pinNumber);

		/// <summary>
		/// Exports a given GPIO pin
		/// </summary>
		/// <param name="pinNumber">The pin number to export</param>
		void Export(BroadcomPinNumber pinNumber);

		/// <summary>
		/// Sets the direction of a GPIO channel
		/// </summary>
		/// <param name="pinNumber">The pin number to set the direction of</param>
		/// <param name="direction">The direction to set it to</param>
		void SetDirection(BroadcomPinNumber pinNumber, Direction direction);

		/// <summary>
		/// Gets the GPIO path to write to
		/// </summary>
		/// <returns>The correct path for the IO and mode</returns>
		std::string GetGpioPath();

		bool PathExist(const char * path);

	};
}
