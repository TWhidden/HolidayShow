#pragma once
#include <vector>


class Person
{

private:

	int _age = 25;

	std::vector <int> _myVector;
	char p;

public:
	Person();
	~Person();


	int GetAge()
	{
		auto size = _myVector.size();

		for (int& i : _myVector)
		{
			
		}
		return _age;
	}

	int GetAge2();
};


class Travis : public Person
{

};


