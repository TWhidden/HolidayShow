#pragma once

namespace HolidayShowEndpoint
{

	/// <summary>
	/// The GPIO direction
	/// </summary>
	enum class Direction : int
	{
		/// <summary>
		/// Configured for input
		/// </summary>
		Input,

		/// <summary>
		/// Configured for output
		/// </summary>
		Output
	};

	/// <summary>
	/// The Broadcom GPIO ID
	/// </summary>
	enum class BroadcomPinNumber : int
	{
		Unknown = -1,

		/// <summary>
		/// GPIO Pin 4
		/// </summary>
		Four = 4,

		/// <summary>
		/// GPIO Pin 14
		/// </summary>
		Fourteen = 14,

		/// <summary>
		/// GPIO Pin 15
		/// </summary>
		Fithteen = 15,

		/// <summary>
		/// GPIO Pin 17
		/// </summary>
		Seventeen = 17,

		/// <summary>
		/// GPIO Pin 18
		/// </summary>
		Eighteen = 18,

		/// <summary>
		/// GPIO Pin 21
		/// </summary>
		TwentyOne = 21,

		/// <summary>
		/// GPIO Pin 22
		/// </summary>
		TwentyTwo = 22,

		/// <summary>
		/// GPIO Pin 23
		/// </summary>
		TwentyThree = 23,

		/// <summary>
		/// GPIO Pin 24
		/// </summary>
		TwentyFour = 24,

		/// <summary>
		/// GPIO Pin 25
		/// </summary>
		TwentyFive = 25,

		/// <summary>
		/// GPIO Pin 27
		/// </summary>
		TwentySeven = 27,
	};

	/// <summary>
	/// The Raspberry Pi GPIO ID
	/// </summary>
	enum class RaspberryPinNumber : int
	{

		/// <summary>
		/// Invalid pin number - used to established a known invalid value to un-initialised variables.
		/// </summary>
		Zero = 0,

		/// <summary>
		/// GPIO Pin 1
		/// </summary>
		One,

		/// <summary>
		/// GPIO Pin 2
		/// </summary>
		Two,

		/// <summary>
		/// GPIO Pin 3
		/// </summary>
		Three,

		/// <summary>
		/// GPIO Pin 4
		/// </summary>
		Four,

		/// <summary>
		/// GPIO Pin 5
		/// </summary>
		Five,

		/// <summary>
		/// GPIO Pin 6
		/// </summary>
		Six,

		/// <summary>
		/// GPIO Pin 7
		/// </summary>
		Seven,
	};

	/// <summary>
	/// The physical GPIO pin ID
	/// </summary>
	enum class PhysicalPinNumber : int
	{
		/// <summary>
		/// Invalid pin number - used to established a known invalid value to un-initialised variables.
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// GPIO Pin 7
		/// </summary>
		Seven = 7,

		/// <summary>
		/// GPIO Pin 11
		/// </summary>
		Eleven = 11,

		/// <summary>
		/// GPIO Pin 12
		/// </summary>
		Twelve = 12,

		/// <summary>
		/// GPIO Pin 13
		/// </summary>
		Thirteen = 13,

		/// <summary>
		/// GPIO Pin 15
		/// </summary>
		Fifteen = 15,

		/// <summary>
		/// GPIO Pin 16
		/// </summary>
		Sixteen = 16,

		/// <summary>
		/// GPIO Pin 18
		/// </summary>
		Eighteen = 18,

		/// <summary>
		/// GPIO Pin 22
		/// </summary>
		TwentyTwo = 22
	};

}