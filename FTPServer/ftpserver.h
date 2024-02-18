#pragma once
#pragma comment (lib,"Ws2_32.lib")
#define PORT_ADDR 21
#define NAME_SERVER_SOCKET "This is FTP server version 1.0"
#define QUEUE_SIZE 5
#include <iostream>
#include <winsock.h>
#include <stdio.h>
#include <stdlib.h>
class server
{
private:
    const int WINSOCK_VERSION = 0x0101;
    WSADATA wsaData;
    SOCKET servsocket;
    SOCKET acceptSocket;;
    const int WM_SERVER_ACCEPT = WM_USER + 1;
public:
    server();
    ~server();
    bool linkSocketPort();
    bool startWinSock();
    bool stopWinSock();
    bool waitSocket();
    bool getHostName();
    bool createSocket();
    void closeSocket();
    bool linkWindowSocket();
    void stopServer();
    bool startServer();
    bool listenSocket();
    bool waitForIncMsg();
    void sendData(LPSTR buff);
    HWND getConsoleHWND();
};