#include "stdafx.h"
#include "LibGpio.h"
#include <sys/types.h>
#include <sys/stat.h>
#include <string>
#include <sstream>
#include "GpioPins.h"

#ifdef _WIN32
#include <direct.h>
#else
#define _SH_DENYNO      0x40    /* deny none mode */
#endif


namespace HolidayShowEndpoint
{

#if defined(WIN32) || defined(_WIN32) 
#define PATH_SEPARATOR "\\" 
#else 
#define PATH_SEPARATOR "/" 
#endif 


	LibGpio::LibGpio()
	{
		auto gpioPath = GetGpioPath();

		auto path = gpioPath.c_str();

		if (!PathExist(path))
		{
#ifdef _WIN32
			_mkdir(path);
#else
			mkdir(path, S_IRWXU);
#endif

			
		}
	}


	LibGpio::~LibGpio()
	{
	}

	//LibGpio *LibGpio::getGpio() const
	//{
	//	if (instance == nullptr)
	//	{
	//		instance = new LibGpio();
	//	}

	//	return instance;
	//}

	const bool &LibGpio::getTestMode() const
	{
		return privateTestMode;
	}

	void LibGpio::setTestMode(const bool &value)
	{
		privateTestMode = value;
	}

	void LibGpio::SetupChannel(RaspberryPinNumber pinNumber, Direction direction)
	{
		this->SetupChannel(ConvertToBroadcom(pinNumber), direction);
	}

	void LibGpio::SetupChannel(PhysicalPinNumber pinNumber, Direction direction)
	{
		this->SetupChannel(ConvertToBroadcom(pinNumber), direction);
	}

	void LibGpio::SetupChannel(BroadcomPinNumber pinNumber, Direction direction)
	{
		std::ostringstream gpioPath;
		gpioPath << this->GetGpioPath() << PATH_SEPARATOR << "gpio" << static_cast<int>(pinNumber);
		/*auto outputName = std::wstring::Format(L"gpio{0}", static_cast<int>(pinNumber));
		auto gpioPath = Path::Combine(this->GetGpioPath(), outputName);*/
		auto path = gpioPath.str();
		auto c = path.c_str();

		std::cout << "SetupChannel: " << path << std::endl;
		// If already exported, unexport it before continuing
		if (PathExist(c))
		{
			std::cout << "Unexport path!" << std::endl;
			this->UnExport(pinNumber);
			std::cout << "Done!" << std::endl;
		} else
		{
#ifdef _WIN32
			_mkdir(c);
#else
			mkdir(c, S_IRWXU);
#endif

		}

		// Now export the channel
		this->Export(pinNumber);

		// Set the IO direction
		this->SetDirection(pinNumber, direction);

		std::cout << "[PiSharp.LibGpio] Broadcom GPIO number '" << static_cast<int>(pinNumber) << "', configured for use" << std::endl;
	}

	void LibGpio::OutputValue(RaspberryPinNumber pinNumber, bool value)
	{
		this->OutputValue(ConvertToBroadcom(pinNumber), value);
	}

	void LibGpio::OutputValue(PhysicalPinNumber pinNumber, bool value)
	{
		this->OutputValue(ConvertToBroadcom(pinNumber), value);
	}

	void LibGpio::OutputValue(BroadcomPinNumber pinNumber, bool value)
	{
		// Check that the output is configured
		if (this->directions.find(pinNumber) == this->directions.end())
		{
			std::cout << "Attempt to output value on un-configured pin" << std::endl;
		}

		// Check that the channel is not being used incorrectly
		if (this->directions[pinNumber] == Direction::Input)
		{
			std::cout << "Attempt to output value on pin configured for input" << std::endl;
		}

		std::ostringstream gpioPath;
		gpioPath << this->GetGpioPath() << PATH_SEPARATOR << "gpio" << static_cast<int>(pinNumber) << PATH_SEPARATOR << "value";
		
		auto s = gpioPath.str();

#if _WIN32
		auto file = _fsopen(s.c_str(), "w", _SH_DENYNO);
#else
		auto file = fopen(s.c_str(), "w");
#endif
		
		if (value)
		{
			fputs("1", file);
		} else
		{
			fputs("0", file);
		}

	}

	bool LibGpio::ReadValue(RaspberryPinNumber pinNumber)
	{
		return this->ReadValue(ConvertToBroadcom(pinNumber));
	}

	bool LibGpio::ReadValue(PhysicalPinNumber pinNumber)
	{
		return this->ReadValue(ConvertToBroadcom(pinNumber));
	}

	bool LibGpio::ReadValue(BroadcomPinNumber pinNumber)
	{
		// Check that the output is configured
		if (this->directions.find(pinNumber) == this->directions.end())
		{
			std::cout << "Attempt to read value from un-configured pin" << std::endl;
		}

		// Check that the channel is not being used incorrectly
		if (this->directions[pinNumber] == Direction::Output)
		{
			std::cout << "Attempt to read value form pin configured for output" << std::endl;
		}


		std::ostringstream gpioPath;
		gpioPath << this->GetGpioPath() << PATH_SEPARATOR << "gpio" << static_cast<int>(pinNumber) << PATH_SEPARATOR << "value";
		std::string s = gpioPath.str();

#if _WIN32
		auto file = _fsopen(s.c_str(), "r", _SH_DENYNO);
#else
		auto file = fopen(s.c_str(), "r");
#endif

		char innerValue;
		auto read = fgets(&innerValue, 1, file);
		if (read != NULL && innerValue == '1')
		{
			return true;
		}
		return false;
	}

	BroadcomPinNumber LibGpio::ConvertToBroadcom(RaspberryPinNumber pinNumber)
	{
		switch (pinNumber)
		{
			case RaspberryPinNumber::Seven:
				return BroadcomPinNumber::Four;

			case RaspberryPinNumber::Zero:
				return BroadcomPinNumber::Seventeen;

			case RaspberryPinNumber::One:
				return BroadcomPinNumber::Eighteen;

			case RaspberryPinNumber::Two:
				return BroadcomPinNumber::TwentyOne;

			case RaspberryPinNumber::Three:
				return BroadcomPinNumber::TwentyTwo;

			case RaspberryPinNumber::Four:
				return BroadcomPinNumber::TwentyThree;

			case RaspberryPinNumber::Five:
				return BroadcomPinNumber::TwentyFour;

			case RaspberryPinNumber::Six:
				return BroadcomPinNumber::TwentyFive;

			default:
				return BroadcomPinNumber::Unknown;
		}
	}

	BroadcomPinNumber LibGpio::ConvertToBroadcom(PhysicalPinNumber pinNumber)
	{
		switch (pinNumber)
		{
			case PhysicalPinNumber::Seven:
				return BroadcomPinNumber::Four;

			case PhysicalPinNumber::Eleven:
				return BroadcomPinNumber::Seventeen;

			case PhysicalPinNumber::Twelve:
				return BroadcomPinNumber::Eighteen;

			case PhysicalPinNumber::Thirteen:
				return BroadcomPinNumber::TwentyOne;

			case PhysicalPinNumber::Fifteen:
				return BroadcomPinNumber::TwentyTwo;

			case PhysicalPinNumber::Sixteen:
				return BroadcomPinNumber::TwentyThree;

			case PhysicalPinNumber::Eighteen:
				return BroadcomPinNumber::TwentyFour;

			case PhysicalPinNumber::TwentyTwo:
				return BroadcomPinNumber::TwentyFive;
			default:
				return BroadcomPinNumber::Unknown;
		}
	}

	void LibGpio::UnExport(BroadcomPinNumber pinNumber)
	{
		std::cout << "Unexporting Pin: " << static_cast<int>(pinNumber) << std::endl;

		std::ostringstream gpioPath;
		gpioPath << this->GetGpioPath() << PATH_SEPARATOR << "unexport";

		std::ostringstream pin;
		pin << static_cast<int>(pinNumber);
		auto pinStr = pin.str();


		#if _WIN32
		auto file = _fsopen(pinStr.c_str(), "a", _SH_DENYNO);
		#else
		auto file = fopen(pinStr.c_str(), "a");
		#endif

		std::cout << "Unexported Pin: " << static_cast<int>(pinNumber) << std::endl;
//
////C# TO C++ CONVERTER NOTE: The following 'using' block is replaced by its C++ equivalent:
////		using (var fileStream = new FileStream(Path.Combine(this.GetGpioPath(), "unexport"), FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
//		auto fileStream = new FileStream(Path::Combine(this->GetGpioPath(), L"unexport"), FileMode::Open, FileAccess::Write, FileShare::ReadWrite);
//		try
//		{
////C# TO C++ CONVERTER NOTE: The following 'using' block is replaced by its C++ equivalent:
////			using (var streamWriter = new StreamWriter(fileStream))
//			auto streamWriter = new StreamWriter(fileStream);
//			try
//			{
//				streamWriter->Write(static_cast<int>(pinNumber));
//			}
////C# TO C++ CONVERTER TODO TASK: There is no native C++ equivalent to the exception 'finally' clause:
//			finally
//			{
//				if (streamWriter != nullptr)
//				{
//					streamWriter.Dispose();
//				}
//			}
//		}
////C# TO C++ CONVERTER TODO TASK: There is no native C++ equivalent to the exception 'finally' clause:
//		finally
//		{
//			if (fileStream != nullptr)
//			{
//				fileStream.Dispose();
//			}
//		}
//
//		std::cout << std::wstring::Format(L"[PiSharp.LibGpio] Broadcom GPIO number '{0}', un-exported", pinNumber) << std::endl;
	}

	void LibGpio::Export(BroadcomPinNumber pinNumber)
	{
		std::stringstream exportDir;
		exportDir << "gpio" << static_cast<int>(pinNumber);
		std::cout << "Export Dir: " << exportDir.str() << std::endl;
		std::stringstream exportPath;
		exportPath << this->GetGpioPath() << exportDir.str();

		std::cout << "Export Path: " << exportPath.str() << std::endl;
		if (!PathExist(exportPath.str().c_str()))
		{
			std::string path = exportPath.str();

			#ifdef _WIN32
				_mkdir(path.c_str());
			#else
				mkdir(path.c_str(), S_IRWXU);
			#endif

			

			// Create the file
			OutputValue(pinNumber, false);

		}
	}

	void LibGpio::SetDirection(BroadcomPinNumber pinNumber, Direction direction)
	{
		std::ostringstream gpioPath;
		gpioPath << this->GetGpioPath() << PATH_SEPARATOR << "gpio" << static_cast<int>(pinNumber) << PATH_SEPARATOR << "direction";

		std::string s = gpioPath.str();

#if _WIN32
		auto file = _fsopen(s.c_str(), "w", _SH_DENYNO);
#else
		auto file = fopen(s.c_str(), "w");
#endif

		if (direction == Direction::Input)
		{
			fputs("in", file);
		}
		else
		{
			fputs("out", file);
		}

		directions[pinNumber] = direction;
		
	}

	std::string LibGpio::GetGpioPath()
	{
		#ifdef _WIN32
				//define something for Windows (32-bit and 64-bit, this part is common)
				return "C:\\RasPiGpioTest";
		#ifdef _WIN64
				//define something for Windows (64-bit only)
				return L"C:\\RasPiGpioTest";
		#endif
		#elif __APPLE__
		#include "TargetConditionals.h"
			//#if TARGET_IPHONE_SIMULATOR
					// iOS Simulator
			//#elif TARGET_OS_IPHONE
					// iOS device
			//#elif TARGET_OS_MAC
					// Other kinds of Mac OS
			//#else
					// Unsupported platform
			return "/tmp/RasPiGpioTest";
			//#endif
		#elif __linux
				// linux
			return "/sys/class/gpio";
		#elif __unix // all unices not caught above
				// Unix
			return "/sys/class/gpio";
		#elif __posix
				// POSIX
			return "/sys/class/gpio";
		#endif
	}

	bool LibGpio::PathExist(const char* path)
	{
		struct stat info;

		if (stat(path, &info) != 0)
		{
			//printf("cannot access %s\n", pathname);
			return false;
			
		}
		else if (info.st_mode & S_IFDIR)
		{
			//printf("%s is a directory\n", pathname);
			return true;
		}
		else
		{
			//printf("%s is no directory\n", pathname);
			return false;
			
		}
	}
}
