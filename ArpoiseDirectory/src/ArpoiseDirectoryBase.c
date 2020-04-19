/*
ArpoiseDirectoryBase.c - base for ARpoise Directory front end service.

Copyright (C) 2018 , Tamiko Thiel and Peter Graf - All Rights Reserved

ARpoise - Augmented Reality Point Of Interest Service

This file is part of ARpoise.

	ARpoise is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	ARpoise is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with ARpoise.  If not, see <https://www.gnu.org/licenses/>.

For more information on

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
ARpoise, see www.ARpoise.com/

$Log: ArpoiseDirectoryBase.c,v $
Revision 1.1  2020/04/18 20:14:41  peter
Restructuring of code


*/

/*
* Make sure "strings <exe> | grep Id | sort -u" shows the source file versions
*/
char* ArpoiseDirectoryBase_c_id = "$Id: ArpoiseDirectoryBase.c,v 1.1 2020/04/18 20:14:41 peter Exp $";

#include <stdio.h>
#include <memory.h>

#ifndef __APPLE__
#include <malloc.h>
#endif

#include <assert.h>
#include <stdlib.h>

#ifdef _WIN32

#include <winsock2.h>
#include <direct.h>
#include <windows.h> 
#include <process.h>

#define socket_close closesocket

#else

#include <sys/socket.h>
#include <sys/time.h>
#include <unistd.h>
#include <netdb.h>
#include <netinet/in.h>
#include <dirent.h>
#include <sys/types.h>
#include <sys/stat.h>

#define socket_close close

#ifndef h_addr
#define h_addr h_addr_list[0] /* for backward compatibility */
#endif

#endif

#include "pblCgi.h"

/*
 * Receive some bytes from a socket
 */
static int receiveBytesFromTcp(int socket, char* buffer, int bufferSize, struct timeval* timeout)
{
	char* tag = "receiveBytesFromTcp";
	int    rc = 0;
	int    socketError = 0;
	unsigned int optlen = sizeof(socketError);

	errno = 0;
	if (getsockopt(socket, SOL_SOCKET, SO_ERROR, (char*)&socketError, &optlen))
	{
		pblCgiExitOnError("%s: getsockopt(%d) error, errno %d\n", tag, socket, errno);
	}

	int nBytesRead = 0;
	while (nBytesRead < bufferSize)
	{
		fd_set readFds;
		FD_ZERO(&readFds);
		FD_SET(socket, &readFds);

		errno = 0;
		rc = select(socket + 1, &readFds, (fd_set*)NULL, (fd_set*)NULL, timeout);
		switch (rc)
		{
		case 0:
			return (-1);

		case -1:
			if (errno == EINTR)
			{
				pblCgiExitOnError("%s: select(%d) EINTR error, errno %d\n", tag, socket, errno);
			}
			pblCgiExitOnError("%s: select(%d) error, errno %d\n", tag, socket, errno);
			break;

		default:
			errno = 0;
			if (getsockopt(socket, SOL_SOCKET, SO_ERROR, (char*)&socketError, &optlen))
			{
				pblCgiExitOnError("%s: getsockopt(%d) error, errno %d\n", tag, socket, errno);
			}

			if (socketError)
			{
				continue;
			}

			errno = 0;
			rc = recvfrom(socket, buffer + nBytesRead, bufferSize - nBytesRead, 0, NULL, NULL);
			if (rc < 0)
			{
				if (errno == EINTR)
				{
					pblCgiExitOnError("%s: recvfrom(%d) EINTR error, errno %d\n", tag, socket, errno);
				}
				pblCgiExitOnError("%s: recvfrom(%d) error, errno %d\n", tag, socket, errno);
			}
			else if (rc == 0)
			{
				return nBytesRead;
			}
			nBytesRead += rc;
		}
	}
	return nBytesRead;
}

static char adbReceiveBuffer[64 * 1024];
/*
* Receive some string bytes and return the result in a malloced buffer.
*/
static char* receiveStringFromTcp(int socket, int timeoutSeconds)
{
	static char* tag = "receiveStringFromTcp";

	char* result = NULL;
	PblStringBuilder* stringBuilder = NULL;

	struct timeval timeoutValue;
	timeoutValue.tv_sec = timeoutSeconds;
	timeoutValue.tv_usec = 0;

	adbReceiveBuffer[0] = '\0';

	for (;;)
	{
		int rc = receiveBytesFromTcp(socket, adbReceiveBuffer, sizeof(adbReceiveBuffer) - 1, &timeoutValue);
		if (rc < 0)
		{
			return NULL;
		}
		if (rc < 0 || rc > sizeof(adbReceiveBuffer) - 1)
		{
			pblCgiExitOnError("%s: readTcp failed! rc %d\n", tag, rc);
		}
		else if (rc == 0)
		{
			break;
		}
		else
		{
			adbReceiveBuffer[rc] = '\0';
		}

		if (rc < sizeof(adbReceiveBuffer) - 1 && stringBuilder == NULL)
		{
			result = pblCgiStrDup(adbReceiveBuffer);
			break;
		}

		if (stringBuilder == NULL)
		{
			stringBuilder = pblStringBuilderNew();
			if (!stringBuilder)
			{
				pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
			}
		}
		if (pblStringBuilderAppendStr(stringBuilder, adbReceiveBuffer) == ((size_t)-1))
		{
			pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
		}
	}

	if (result == NULL)
	{
		if (stringBuilder == NULL)
		{
			pblCgiExitOnError("%s: socket %d received 0 bytes as response\n", tag, socket);
		}

		result = pblStringBuilderToString(stringBuilder);
		if (!result)
		{
			pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
		}
	}
	if (stringBuilder)
	{
		pblStringBuilderFree(stringBuilder);
	}
	return result;
}

/*
* Send some bytes to a tcp socket
*/
static void sendBytesToTcp(int socket, char* buffer, int nBytesToSend)
{
	static char* tag = "sendBytesToTcp";

	char* ptr = buffer;
	while (nBytesToSend > 0)
	{
		errno = 0;
		int rc = send(socket, ptr, nBytesToSend, 0);
		if (rc > 0)
		{
			ptr += rc;
			nBytesToSend -= rc;
		}
		else
		{
			pblCgiExitOnError("%s: send(%d) error, rc %d, errno %d\n", tag, socket, rc, errno);
		}
	}
}

/*
* Connect to a tcp socket on machine with hostname and port
*/
static int connectToTcp(char* hostname, int port)
{
	static char* tag = "connectToTcp";

	errno = 0;
	struct hostent* hostInfo = gethostbyname(hostname);
	if (!hostInfo)
	{
		pblCgiExitOnError("%s: gethostbyname(%s) error, errno %d.\n", tag, hostname, errno);
		return -1;
	}

	short shortPort = 80;
	if (port > 0)
	{
		shortPort = port;
	}

	struct sockaddr_in serverAddress;
	memset((char*)&serverAddress, 0, sizeof(struct sockaddr_in));
	serverAddress.sin_family = AF_INET;
	serverAddress.sin_port = htons(shortPort);
	memcpy(&(serverAddress.sin_addr.s_addr), hostInfo->h_addr, sizeof(serverAddress.sin_addr.s_addr));

	errno = 0;
	int socketFd = socket(AF_INET, SOCK_STREAM, 0);
	if (socketFd < 0)
	{
		pblCgiExitOnError("%s: socket() error, errno %d\n", tag, errno);
	}

	errno = 0;
	if (connect(socketFd, (struct sockaddr*) & serverAddress, sizeof(struct sockaddr_in)) < 0)
	{
		pblCgiExitOnError("%s: connect(%d) error, host '%s' on port %d, errno %d\n", tag, socketFd, hostname, shortPort, errno);
		socket_close(socketFd);
	}
	return socketFd;
}

/*
* Make a HTTP request with the given uri to the given host/port
* and return the result content in a malloced buffer.
*/
char* adbGetHttpResponse(char* hostname, int port, char* uri, int timeoutSeconds, char* agent)
{
	char* response = NULL;
	for (int n = 0; n < 2; n++)
	{
		int socketFd = connectToTcp(hostname, port);

		char* sendBuffer = pblCgiSprintf("GET %s HTTP/1.0\r\nUser-Agent: %s\r\nHost: %s\r\n\r\n", uri, agent, hostname);
		PBL_CGI_TRACE("HttpRequest=%s", sendBuffer);

		sendBytesToTcp(socketFd, sendBuffer, strlen(sendBuffer));
		PBL_FREE(sendBuffer);

		response = receiveStringFromTcp(socketFd, timeoutSeconds);
		socket_close(socketFd);
		if (!response)
		{
			PBL_CGI_TRACE("HttpResponse=NULL, n=%d", n);
			continue;
		}
		PBL_CGI_TRACE("HttpResponse=%s", response);
		break;
	}
	if (!response)
	{
		pblCgiExitOnError("getHttpResponse: receiveStringFromTcp returned NULL\n");
	}
	return response;
}

static char* getMatchingString(char* string, char start, char end, char** nextPtr)
{
	char* tag = "getMatchingString";
	char* ptr = string;
	if (start != *ptr)
	{
		pblCgiExitOnError("%s: expected %c at start of string '%s'\n", tag, start, string);
	}

	int level = 1;
	int c;
	while ((c = *++ptr))
	{
		if (c == start)
		{
			level++;
		}
		if (c == end)
		{
			level--;
			if (level < 1)
			{
				if (nextPtr)
				{
					*nextPtr = ptr + 1;
				}
				return pblCgiStrRangeDup(string + 1, ptr);
			}
		}
	}
	pblCgiExitOnError("%s: unexpected end of string in '%s'\n", tag, string);
	return NULL;
}

char* adbGetStringBetween(char* string, char* start, char* end)
{
	char* tag = "adbGetStringBetween";
	char* ptr = *start ? strstr(string, start) : string;
	if (!ptr)
	{
		pblCgiExitOnError("%s: expected starting tag '%s' in string '%s'\n", tag, start, string);
		return NULL;
	}
	ptr += strlen(start);
	char* ptr2 = strstr(ptr, end);
	if (!ptr2)
	{
		pblCgiExitOnError("%s: expected ending '%s' in string '%s'\n", tag, end, ptr);
	}
	return pblCgiStrRangeDup(ptr, ptr2);
}

static char* getNumberString(char* string, char* start)
{
	char* tag = "getNumberString";
	char* ptr = strstr(string, start);
	if (!ptr)
	{
		pblCgiExitOnError("%s: expected starting tag '%s' in string '%s'\n", tag, start, string);
	}
	ptr += strlen(start);

	char* ptr2 = ptr;
	while (ptr2 && *ptr2)
	{
		if (isdigit(*ptr2) || '.' == *ptr2 || '-' == *ptr2 || '+' == *ptr2)
		{
			ptr2++;
			continue;
		}
		break;
	}
	if (!ptr2)
	{
		pblCgiExitOnError("%s: expected number ending in string '%s'\n", tag, ptr);
	}
	return pblCgiStrRangeDup(ptr, ptr2);
}

char* adbGetHttpResponseBody(char* response, char** cookiePtr)
{
	static char* tag = "adbGetHttpResponseBody";

	// check for HTTP error code like HTTP/1.1 500 Server Error
	//
	if (cookiePtr && strstr(response, "Set-Cookie: "))
	{
		*cookiePtr = adbGetStringBetween(response, "Set-Cookie: ", "\r\n");
	}

	char* ptr = strstr(response, "HTTP/");
	if (ptr)
	{
		ptr = strstr(ptr, " ");
		if (ptr)
		{
			ptr++;
			if (strncmp(ptr, "200", 3))
			{
				pblCgiExitOnError("%s: Bad HTTP response\n%s\n", tag, response);
			}
		}
	}

	if (ptr)
	{
		char* end = strstr(ptr, "\r\n\r\n");
		if (!end)
		{
			end = strstr(ptr, "\n\n");
			if (!end)
			{
				pblCgiExitOnError("%s: Illegal HTTP response, no separator.\n%s\n", tag, response);
			}
			else
			{
				end += 2;
			}
		}
		else
		{
			end += 4;
		}
		return end;
	}

	pblCgiExitOnError("%s: Expecting HTTP response\n%s\n", tag, response);
	return NULL;
}

static void putString(char* string, PblStringBuilder* stringBuilder)
{
	char* tag = "putString";

	if (pblStringBuilderAppendStr(stringBuilder, string) == ((size_t)-1))
	{
		pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
	}
	fputs(string, stdout);
}

static char* changeLat(char* string, int i, int difference)
{
	if (!strstr(string, "\"lat\":"))
	{
		return pblCgiStrDup(string);
	}

	int factor = 1 + (i - 1) / 8;
	int modulo = (i - 1) % 8;

	switch (modulo)
	{
	case 0:
		difference *= factor;
		break;
	case 1:
		difference *= -factor;
		break;
	case 4:
	case 5:
		difference *= factor;
		break;
	case 6:
	case 7:
		difference *= -factor;
		break;
	default:
		return pblCgiStrDup(string);
	}

	char* lat = getNumberString(string, "\"lat\":");
	//PBL_CGI_TRACE("lat=%s", lat);

	char* oldLat = pblCgiSprintf("\"lat\":%s,", lat);
	//PBL_CGI_TRACE("oldLat=%s", oldLat);

	char* newLat = pblCgiSprintf("\"lat\":%d,", atoi(lat) + difference);
	//PBL_CGI_TRACE("newLat=%s", newLat);

	char* replacedLat = pblCgiStrReplace(string, oldLat, newLat);

	PBL_FREE(lat);
	PBL_FREE(oldLat);
	PBL_FREE(newLat);

	return replacedLat;
}

static char* changeLon(char* string, int i, int difference)
{
	if (!strstr(string, "\"lon\":"))
	{
		return pblCgiStrDup(string);
	}

	int factor = 1 + (i - 1) / 8;
	int modulo = (i - 1) % 8;

	switch (modulo)
	{
	case 2:
		difference *= factor;
		break;
	case 3:
		difference *= -factor;
		break;
	case 4:
	case 6:
		difference *= factor;
		break;
	case 5:
	case 7:
		difference *= -factor;
		break;
	default:
		return pblCgiStrDup(string);
	}

	char* lon = getNumberString(string, "\"lon\":");
	//PBL_CGI_TRACE("lon=%s", lon);

	char* oldLon = pblCgiSprintf("\"lon\":%s,", lon);
	//PBL_CGI_TRACE("oldLon=%s", oldLon);

	char* newLon = pblCgiSprintf("\"lon\":%d,", atoi(lon) + difference);
	//PBL_CGI_TRACE("newLon=%s", newLon);

	char* replacedLon = pblCgiStrReplace(string, oldLon, newLon);

	PBL_FREE(lon);
	PBL_FREE(oldLon);
	PBL_FREE(newLon);

	return replacedLon;
}

char* adbChangeLatAndLon(char* queryString, char* lat, char* lon, int* latDifference, int* lonDifference)
{
	if (!pblCgiStrIsNullOrWhiteSpace(lat) && !pblCgiStrIsNullOrWhiteSpace(lon))
	{
		char* replacementLat = pblCgiStrCat("lat=", lat);
		char* replacementLon = pblCgiStrCat("lon=", lon);
		double latDouble = strtod(replacementLat + 4, NULL);
		int replacementLatInteger = (int)(1000000.0 * latDouble);
		double lonDouble = strtod(replacementLon + 4, NULL);
		int replacementLonInteger = (int)(1000000.0 * lonDouble);
		int latPtrInteger = 0;
		int lonPtrInteger = 0;

		char* latPtr = strstr(queryString, "lat=");
		if (latPtr)
		{
			char* ptr = strstr(latPtr, "&");
			if (ptr)
			{
				latPtr = pblCgiStrRangeDup(latPtr, ptr);
			}
			else
			{
				latPtr = pblCgiStrDup(latPtr);
			}
			latPtrInteger = (int)(1000000.0 * strtod(latPtr + 4, NULL));
			queryString = pblCgiStrReplace(queryString, latPtr, replacementLat);
		}
		char* lonPtr = strstr(queryString, "lon=");
		if (lonPtr)
		{
			char* ptr = strstr(lonPtr, "&");
			if (ptr)
			{
				lonPtr = pblCgiStrRangeDup(lonPtr, ptr);
			}
			else
			{
				lonPtr = pblCgiStrDup(lonPtr);
			}
			lonPtrInteger = (int)(1000000.0 * strtod(lonPtr + 4, NULL));
			queryString = pblCgiStrReplace(queryString, lonPtr, replacementLon);
		}
		if (latDifference && lonDifference && latPtrInteger != 0 && lonPtrInteger != 0)
		{
			*latDifference = replacementLatInteger - latPtrInteger;
			*lonDifference = replacementLonInteger - lonPtrInteger;
		}
		return queryString;
	}
	return NULL;
}

char* adbChangeRedirectionUrl(char* string, char* redirectionUrl)
{
	if (!strstr(string, "\"redirectionUrl\":"))
	{
		return pblCgiStrDup(string);
	}

	char* oldValue = adbGetStringBetween(string, "\"redirectionUrl\":", ",\"");

	char* oldValueStr = pblCgiSprintf("\"redirectionUrl\":%s", oldValue);
	char* newValueStr = pblCgiSprintf("\"redirectionUrl\":\"%s\"", redirectionUrl);

	char* replacedString = pblCgiStrReplace(string, oldValueStr, newValueStr);

	PBL_FREE(oldValue);
	PBL_FREE(oldValueStr);
	PBL_FREE(newValueStr);

	return replacedString;
}

char* adbChangeLayer(char* string, char* layer)
{
	if (!strstr(string, "\"layer\":"))
	{
		return pblCgiStrDup(string);
	}

	char* oldValue = adbGetStringBetween(string, "\"layer\":", ",\"");

	char* oldValueStr = pblCgiSprintf("\"layer\":%s", oldValue);
	char* newValueStr = pblCgiSprintf("\"layer\":\"%s\"", layer);

	char* replacedString = pblCgiStrReplace(string, oldValueStr, newValueStr);

	PBL_FREE(oldValue);
	PBL_FREE(oldValueStr);
	PBL_FREE(newValueStr);

	return replacedString;
}

char* adbChangeRedirectionLayer(char* string, char* redirectionLayer)
{
	if (!strstr(string, "\"redirectionLayer\":"))
	{
		return pblCgiStrDup(string);
	}

	char* oldValue = adbGetStringBetween(string, "\"redirectionLayer\":", ",\"");

	char* oldValueStr = pblCgiSprintf("\"redirectionLayer\":%s", oldValue);
	char* newValueStr = pblCgiSprintf("\"redirectionLayer\":\"%s\"", redirectionLayer);

	char* replacedString = pblCgiStrReplace(string, oldValueStr, newValueStr);

	PBL_FREE(oldValue);
	PBL_FREE(oldValueStr);
	PBL_FREE(newValueStr);

	return replacedString;
}

char* adbChangeLayerName(char* string, char* layerName)
{
	if (!strstr(string, "layerName="))
	{
		return pblCgiStrDup(string);
	}

	char* oldLayerName = adbGetStringBetween(string, "layerName=", "&");

	char* oldLayerNameStr = pblCgiSprintf("layerName=%s", oldLayerName);
	char* newLayerNameStr = pblCgiSprintf("layerName=%s", layerName);

	char* replacedString = pblCgiStrReplace(string, oldLayerNameStr, newLayerNameStr);

	PBL_FREE(oldLayerName);
	PBL_FREE(oldLayerNameStr);
	PBL_FREE(newLayerNameStr);

	return replacedString;
}

char* adbChangeShowMenuOption(char* string, char* value)
{
	if (!strstr(string, "\"showMenuButton\":"))
	{
		return pblCgiStrDup(string);
	}

	char* oldValue = adbGetStringBetween(string, "\"showMenuButton\":", ",\"");

	char* oldValueStr = pblCgiSprintf("\"showMenuButton\":%s", oldValue);
	char* newValueStr = pblCgiSprintf("\"showMenuButton\":\"%s\"", value);

	char* replacedString = pblCgiStrReplace(string, oldValueStr, newValueStr);

	PBL_FREE(oldValue);
	PBL_FREE(oldValueStr);
	PBL_FREE(newValueStr);

	return replacedString;
}

static PblList* devicePositionList = NULL;

char* adbHandleDevicePosition(char* deviceId, char* client, char* queryString, int* latDifference, int* lonDifference)
{
	if (pblCgiStrIsNullOrWhiteSpace(deviceId))
	{
		return NULL;
	}

	char* lat = NULL;
	char* lon = NULL;

	if (!devicePositionList)
	{
		char* devicePositionValue = pblCgiConfigValue("DevicePosition", NULL);
		if (pblCgiStrIsNullOrWhiteSpace(devicePositionValue))
		{
			return NULL;
		}
		devicePositionList = pblCgiStrSplitToList(devicePositionValue, ",");
		if (pblListIsEmpty(devicePositionList))
		{
			PBL_CGI_TRACE("DevicePositionList is empty");
			return NULL;
		}
	}

	int listSize = pblListSize(devicePositionList);

	for (int i = 0; i < listSize - 2; i += 3)
	{
		char* device = pblListGet(devicePositionList, i);

		if (pblCgiStrEquals(deviceId, device))
		{
			lat = pblListGet(devicePositionList, i + 1);
			lon = pblListGet(devicePositionList, i + 2);
			break;
		}
	}
	return adbChangeLatAndLon(queryString, lat, lon, latDifference, lonDifference);
}

void adbTraceDuration()
{
	struct timeval now;
	gettimeofday(&now, NULL);

	unsigned long duration = now.tv_sec * 1000000 + now.tv_usec;
	duration -= pblCgiStartTime.tv_sec * 1000000 + pblCgiStartTime.tv_usec;
	char* string = pblCgiSprintf("%lu", duration);
	PBL_CGI_TRACE("Duration=%s microseconds", string);
}

void adbPrintHeader(char* cookie)
{
	fputs("Content-Type: application/json\r\n", stdout);
	if (cookie)
	{
		fputs("Set-Cookie: ", stdout);
		fputs(cookie, stdout);
		fputs("\r\n", stdout);
	}
	fputs("\r\n", stdout);
}

void adbHandleResponse(char* response, int latDifference, int lonDifference)
{
	static char* tag = "adbHandleResponse";
	char* cookie = NULL;
	response = adbGetHttpResponseBody(response, &cookie);

	char* start = "{\"hotspots\":";
	int length = strlen(start);

	if (strncmp(start, response, length))
	{
		adbPrintHeader(cookie);
		fputs(response, stdout);
		PBL_CGI_TRACE("Response does not start with %s, no handling", start);
		return;
	}

	PblStringBuilder* stringBuilder = pblStringBuilderNew();
	if (!stringBuilder)
	{
		pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
	}

	char* rest = NULL;
	char* hotspotsString = getMatchingString(response + length, '[', ']', &rest);

	PblList* list = pblListNewArrayList();
	if (!list)
	{
		pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
	}

	char* ptr = hotspotsString;
	while (*ptr == '{')
	{
		char* ptr2 = NULL;
		char* hotspot = getMatchingString(ptr, '{', '}', &ptr2);

		if (pblListAdd(list, hotspot) < 0)
		{
			pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
		}
		if (*ptr2 != ',')
		{
			break;
		}
		ptr = ptr2 + 1;
	}

	adbPrintHeader(cookie);
	putString(start, stringBuilder);
	putString("[", stringBuilder);

	int nPois = pblListSize(list);
	PBL_CGI_TRACE("Number of pois=%d", nPois);

	for (int j = 0; j < nPois; j++)
	{
		if (j > 0)
		{
			putString(",", stringBuilder);
		}
		putString("{", stringBuilder);

		char* hotspot = pblListGet(list, j);

		char* ptr = hotspot;
		if (latDifference != 0 || lonDifference != 0)
		{
			char* replacedLat = changeLat(hotspot, 1, -1 * latDifference);
			ptr = changeLon(replacedLat, 5, -1 * lonDifference);
			PBL_CGI_TRACE("Applied latDifference=%d and lonDifference=%d", latDifference, lonDifference);
		}
		putString(ptr, stringBuilder);
		putString("}", stringBuilder);
	}

	putString("]", stringBuilder);

	putString(rest, stringBuilder);
	PBL_CGI_TRACE("output=%s", pblStringBuilderToString(stringBuilder));
	pblStringBuilderFree(stringBuilder);
}

static void createStatisticsFile(char* directory, char* fileName)
{
	char* filePath = pblCgiSprintf("%s/%s", directory, fileName);

	FILE* stream = NULL;

#ifdef WIN32
	errno_t err = fopen_s(&stream, filePath, "r");
	if (err != 0)
	{
		stream = NULL;
	}
#else
	stream = fopen(filePath, "r");
#endif
	if (!stream)
	{
		stream = pblCgiFopen(filePath, "a");
		if (stream)
		{
			fputs("<title>Arpoise</title>\n<body>copyright © 2019, Tamiko Thiel and Peter Graf</body>\n", stream);
		}
	}
	if (stream)
	{
		fclose(stream);
	}
	PBL_FREE(filePath);
}

void adbCreateStatisticsHits(int layer, char* layerName, int layerServed)
{
	char* count = pblCgiQueryValue("count");
	if (pblCgiStrEquals("1", count))
	{
		PBL_CGI_TRACE("-------> Statistics Request\n");

		// Create a web hit for the os and bundle, so that web stats can be used to count hits

		char* versionsDirectory = pblCgiConfigValue("VersionsDirectory", "");
		if (versionsDirectory && *versionsDirectory)
		{
			char* os = pblCgiQueryValue("os");
			if (!os || !*os || strstr(os, ".."))
			{
				os = "UnknownOperatingSystem";
			}

			char* bundle = pblCgiQueryValue("bundle");
			if (!bundle || !*bundle || strstr(bundle, ".."))
			{
				bundle = "UnknownBundle";
			}

			char* fileName = pblCgiSprintf("%s_%s.htm", os, bundle);
			createStatisticsFile(versionsDirectory, fileName);
			char* uri = pblCgiSprintf("/ArpoiseDirectory/AppVersions/%s", fileName);
			adbGetHttpResponse("www.arpoise.com", 80, uri, 16, "ArpoiseDirectory/AppVersions");
		}

		// Create a web hit for the location, so that web stats can be used to count hits

		char* locationsDirectory = pblCgiConfigValue("LocationsDirectory", "");
		if (locationsDirectory && *locationsDirectory)
		{
			char* queryLat = pblCgiQueryValue("lat");
			if (!queryLat || !*queryLat || strstr(queryLat, ".."))
			{
				queryLat = "UnknownLat";
			}
			else
			{
				queryLat = pblCgiStrDup(queryLat);
			}
			char* ptr = strstr(queryLat, ".");
			if (ptr && strlen(ptr) > 4)
			{
				ptr[4] = '\0'; // truncate latitude to 3 digits after the '.'
			}

			char* queryLon = pblCgiQueryValue("lon");
			if (!queryLon || !*queryLon || strstr(queryLon, ".."))
			{
				queryLon = "UnknownLon";
			}
			else
			{
				queryLon = pblCgiStrDup(queryLon);
			}
			ptr = strstr(queryLon, ".");
			if (ptr && strlen(ptr) > 4)
			{
				ptr[4] = '\0'; // truncate longitude to 3 digits after the '.'
			}

			char* fileName = pblCgiSprintf("%s_%s-%s.htm", queryLon, queryLat, layerName);
			createStatisticsFile(locationsDirectory, fileName);
			char* uri = pblCgiSprintf("/ArpoiseDirectory/Locations/%s", fileName);
			adbGetHttpResponse("www.arpoise.com", 80, uri, 16, "ArpoiseDirectory/Locations");
		}

		// Create a web hit for the layer, so that web stats can be used to count hits

		char* layersDirectory = pblCgiConfigValue("LayersDirectory", "");
		if (layer && layersDirectory && *layersDirectory && layerName && *layerName)
		{
			if (!layerName || !*layerName || strstr(layerName, ".."))
			{
				layerName = "UnknownLayer";
			}

			char* fileName = pblCgiSprintf("%s.htm", layerName);
			createStatisticsFile(layersDirectory, fileName);
			char* uri = pblCgiSprintf("/ArpoiseDirectory/Layers/%s", fileName);
			adbGetHttpResponse("www.arpoise.com", 80, uri, 16, "ArpoiseDirectory/Layers");
		}

		// Create a web hit for the layer served, so that web stats can be used to count hits

		char* layersServedDirectory = pblCgiConfigValue("LayersServedDirectory", "");
		if (layerServed && layersServedDirectory && *layersServedDirectory && layerName && *layerName)
		{
			if (!layerName || !*layerName || strstr(layerName, ".."))
			{
				layerName = "UnknownLayer";
			}

			char* fileName = pblCgiSprintf("%s.htm", layerName);
			createStatisticsFile(layersServedDirectory, fileName);
			char* uri = pblCgiSprintf("/ArpoiseDirectory/LayersServed/%s", fileName);
			adbGetHttpResponse("www.arpoise.com", 80, uri, 16, "ArpoiseDirectory/LayersServed");
		}
	}
}

static void freeStringList(PblList* list)
{
	while (!pblListIsEmpty(list))
	{
		free(pblListPop(list));
	}
	pblListFree(list);
}

char* adbGetArea(char* queryString)
{
	int lat = 0;
	int lon = 0;
	char* latPtr = strstr(queryString, "lat=");
	if (latPtr)
	{
		char* ptr = strstr(latPtr, "&");
		if (ptr)
		{
			latPtr = pblCgiStrRangeDup(latPtr, ptr);
		}
		else
		{
			latPtr = pblCgiStrDup(latPtr);
		}
		double latDouble = strtod(latPtr + 4, NULL);
		lat = (int)(1000000.0 * latDouble);
	}
	char* lonPtr = strstr(queryString, "lon=");
	if (lonPtr)
	{
		char* ptr = strstr(lonPtr, "&");
		if (ptr)
		{
			lonPtr = pblCgiStrRangeDup(lonPtr, ptr);
		}
		else
		{
			lonPtr = pblCgiStrDup(lonPtr);
		}
		double lonDouble = strtod(lonPtr + 4, NULL);
		lon = (int)(1000000.0 * lonDouble);
	}
	for (int i = 1; i <= 1000; i++)
	{
		char* areaKey = pblCgiSprintf("Area_%d", i);
		char* areaValue = pblCgiConfigValue(areaKey, NULL);

		if (pblCgiStrIsNullOrWhiteSpace(areaValue))
		{
			PBL_CGI_TRACE("No value for area %s", areaKey);
			PBL_FREE(areaKey);
			return NULL;
		}

		PblList* locationList = pblCgiStrSplitToList(areaValue, ",");
		int size = pblListSize(locationList);
		if (size != 4)
		{
			PBL_CGI_TRACE("%s, expecting 4 location values, current value is %s", areaKey, areaValue);

			freeStringList(locationList);
			PBL_FREE(areaKey);
			continue;
		}

		int list0 = atoi(pblListGet(locationList, 0));
		int list1 = atoi(pblListGet(locationList, 1));
		int list2 = atoi(pblListGet(locationList, 2));
		int list3 = atoi(pblListGet(locationList, 3));

		if (lat < list0 || lon < list1 || lat > list2 || lon > list3)
		{
			PBL_CGI_TRACE("%s, lat %d, lon %d is outside area value %s", areaKey, lat, lon, areaValue);

			freeStringList(locationList);
			PBL_FREE(areaKey);
			continue;
		}
		PBL_CGI_TRACE("%s, lat %d, lon %d is inside area value %s", areaKey, lat, lon, areaValue);

		freeStringList(locationList);
		return areaKey;
	}
	return NULL;
}

char* adbGetAreaConfigValue(char* area, char* key, char* defaultValue)
{
	char* valueString = NULL;
	if (!pblCgiStrIsNullOrWhiteSpace(area))
	{
		char* areaKey = pblCgiSprintf("%s_%s", area, key);
		valueString = pblCgiConfigValue(areaKey, NULL);
		PBL_FREE(areaKey);
	}
	if (pblCgiStrIsNullOrWhiteSpace(valueString))
	{
		valueString = pblCgiConfigValue(key, defaultValue);
	}
	if (pblCgiStrIsNullOrWhiteSpace(valueString))
	{
		PBL_CGI_TRACE("No value for %s", key);
	}
	return valueString;
}
