#include<stdio.h>
#include<unistd.h>

int main()
{
	int pid = fork();
	if (pid != 0)
	{
		if (pid > 0)
		{
			printf("hello from Parent!\n");
		}
		else 
		{
			printf("Error!\n");
		}
	}
	else
	{
		printf("Hello from child!\n");
		execl("/bin/ls", "-la", NULL);
	}
	
	return 0;
}
