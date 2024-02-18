#include "ftpserver.h"
#include <Windows.h>
void main()
{
    server s;
    printf("Enter '+' to start server\n");
    if (getchar() == '+')
        s.startServer();
    rewind(stdin);
    printf("Enter '-' to stop server\n");
    if (getchar() == '-')
        s.stopServer();
    //system("pause");
}