#include "ftpserver.h"
server::server()
{
    SetConsoleTitle((LPCWSTR)NAME_SERVER_SOCKET);
}
server::~server()
{
}
bool server::startWinSock()
{
    if (WSAStartup(WINSOCK_VERSION, &wsaData))
    {
        printf("winsock is not initialized\n");
        WSACleanup();
        return false;
    }
    else printf("Whinsock initial ok!\n");
    return true;
}
bool server::stopWinSock()
{
    if (WSACleanup())
    {
        printf("Error crear!\n");
        return false;
    }
    else printf("Winsock stopped!\n");
    return true;
}
bool server::waitSocket()
{
    Sleep(100);
    getHostName();
    Sleep(100);
    createSocket();
    Sleep(100);
    linkSocketPort();
    Sleep(100);
    linkWindowSocket();
    Sleep(100);
    closeSocket();
    return true;
}
bool server::getHostName()
{
    char chInfo[64];
    if (gethostname(chInfo, sizeof(chInfo)))
    {
        printf("Not local host!\n");
        return false;
    }
    else
    {
        printf("Local host name:");
        printf(chInfo);
        printf("\n");
    }
    return true;
}
bool server::createSocket()
{
    //this->getHostName();
    servsocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (servsocket == INVALID_SOCKET)
    {
        printf("Error while creating socket!\n");
        return false;
    }
    return true;

}
bool server::linkSocketPort()
{
    SOCKADDR_IN socketaddr;
    socketaddr.sin_family = AF_INET;
    socketaddr.sin_addr.S_un.S_addr = INADDR_ANY;
    socketaddr.sin_port = htons(PORT_ADDR);
    if (bind(this->servsocket, (LPSOCKADDR)&socketaddr, sizeof(socketaddr)))
    {
        printf("Error bind socket\n");
        return false;
    }
    else printf("Sucsess bind!\n");
    //closesocket(servsocket);
    return true;

}
void server::closeSocket()
{
    closesocket(servsocket);
}

bool server::linkWindowSocket()
{
    int errors;
    errors = WSAAsyncSelect(servsocket, getConsoleHWND(), WM_SERVER_ACCEPT, FD_ACCEPT);//FD_ACCEPT
    if (errors == SOCKET_ERROR)
    {
        printf("Bad async!\n");
        system("pause");
        return false;
    }
    else printf("Good async!\n");
    //closesocket(servsocket);
    return true;
}

void server::stopServer()
{
    closeSocket();
    stopWinSock();
}

bool server::startServer()
{
    if (!startWinSock()) return false;
    Sleep(300);
    if (!getHostName()) return false;
    Sleep(300);
    if (!createSocket()) return false;
    Sleep(300);
    if (!linkSocketPort()) return false;
    Sleep(300);
    //if (!linkWindowSocket())return false;
    Sleep(300);
    if (!listenSocket()) return false;
    Sleep(300);
    if (!waitForIncMsg()) return false;
    return true;
}

bool server::listenSocket()
{
    int errors;
    errors = listen(servsocket, 1);
    if (errors == SOCKET_ERROR)
    {
        printf("Bad listen\n");
        return false;
    }
    else printf("Listen---OK!\n");
    //closesocket(servsocket);
    return true;
}

bool server::waitForIncMsg()
{
     // Accept the connection.
    while (getchar() != 'q')
    {
        wprintf(L"Waiting for client to connect...\n");
        acceptSocket = accept(servsocket, NULL, NULL);
        if (acceptSocket == INVALID_SOCKET) {
            wprintf(L"accept failed with error: %ld\n", WSAGetLastError());
            closesocket(servsocket);
            WSACleanup();
            return 1;
        }
        wprintf(L"Client connected.\n");
        sendData((LPSTR) (TEXT("You connected to server!\r\n")) );
        printf("Enter 'q' to stop serv\n");
        rewind(stdin);
    }
}

void server::sendData(LPSTR buff)
{
    int errors = send(acceptSocket, (LPSTR)buff, strlen(buff), 0);
    if (errors == SOCKET_ERROR) wprintf(L"Error while sending\n");
}

HWND server::getConsoleHWND()
{
    HWND consoleWindow;
    consoleWindow = FindWindow(NULL, (LPCWSTR) NAME_SERVER_SOCKET);
    if (consoleWindow == 0)
    {
        printf("No window!\n");
        system("pause");
        exit(1);
    }
    return consoleWindow;
}