// helloworld1.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include<algorithm>
#include"iostream"
#include<memory>
using namespace std;



int main(int argc, _TCHAR* argv[])
{
	
	char s[100]="hello world!";
	int len=strlen(s);
	int i=0;
	int n=0;
	cin>>n;
	unique_ptr<char[]> ts(new char[len + 1]);
	while(i<len)
	{
		ts[(i+n)%len]=s[i];
		i++;
	}
	ts[len]='\0';
	printf("%s",ts);
	system("pause");
	return 0;
}

/*char s[100]="hello world!";
	int E=0;
	int L=0;
	for_each(s,s+strlen(s),[&](char c)
	{
		if (c=='e' || c=='E')
            E++;
        if (c=='l' || c=='L')
            L++;
	});
	cout<<E<<endl;
	cout<<L<<endl;
	system("pause");
	return 0;*/