#pragma once
#include <string>
#include <ostream>

class AudioContainer
{
private:
	
	
public:
	AudioContainer(std::string fileLocation);
	~AudioContainer();

	void Start();

	void Stop();
};

