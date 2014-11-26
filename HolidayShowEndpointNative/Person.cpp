#include "stdafx.h"
#include "Person.h"
#include <iostream>
using namespace std;

Person::Person()
{
	int j = 15;

	_myVector.push_back(12);
	_myVector.push_back(j);

	j = 4;
	
	
	cout <<  "my size is " << sizeof(Person) << endl;

	//p = new char();
}


Person::~Person()
{
	cout << "Im done. out" << endl;
//	delete p;
}

int Person::GetAge2()
{
		
	return _age;
}