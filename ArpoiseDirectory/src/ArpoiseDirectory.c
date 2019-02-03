/*
ArpoiseDirectory.c - main for Arpoise Directory service.

Copyright (C) 2018   Tamiko Thiel and Peter Graf

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

For more information on Tamiko Thiel or Peter Graf,
please see: http://www.mission-base.com/.

$Log: ArpoiseDirectory.c,v $
Revision 1.10  2019/02/03 13:05:47  peter
Improved bundle handling

Revision 1.9  2019/02/03 12:52:47  peter
Fixed the count handling

Revision 1.8  2019/02/03 12:47:08  peter
Do not create statistics hits for refreshes

Revision 1.7  2019/02/03 12:03:06  peter
Creating the versions and layer names files for the hits

Revision 1.6  2019/02/03 01:30:41  peter
Versions file handling

Revision 1.5  2019/02/02 16:53:49  peter
Default layer is reign of gold

Revision 1.4  2019/01/30 23:45:24  peter
Fixed a bug with longer responses

Revision 1.3  2019/01/20 16:13:33  peter
Cleanup of traces

Revision 1.2  2019/01/19 15:50:38  peter
Improved the user agent string

Revision 1.1  2019/01/19 00:03:31  peter
Working on arpoise directory service


*/

/*
* Make sure "strings <exe> | grep Id | sort -u" shows the source file versions
*/
char * ArpoiseDirectory_c_id = "$Id: ArpoiseDirectory.c,v 1.10 2019/02/03 13:05:47 peter Exp $";

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
static int receiveBytesFromTcp(int socket, char * buffer, int bufferSize, struct timeval * timeout)
{
	char * tag = "readTcp";
	int    rc = 0;
	int    socketError = 0;
	int    optlen = sizeof(socketError);

	errno = 0;
	if (getsockopt(socket, SOL_SOCKET, SO_ERROR, (char *)&socketError, &optlen))
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
		rc = select(socket + 1, &readFds, (fd_set *)NULL, (fd_set *)NULL, timeout);
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
			if (getsockopt(socket, SOL_SOCKET, SO_ERROR, (char *)&socketError, &optlen))
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

/*
* Receive some string bytes and return the result in a malloced buffer.
*/
static char * receiveStringFromTcp(int socket, int timeoutSeconds)
{
	static char * tag = "receiveStringFromTcp";

	char * result = NULL;
	PblStringBuilder * stringBuilder = NULL;

	struct timeval timeoutValue;
	timeoutValue.tv_sec = timeoutSeconds;
	timeoutValue.tv_usec = 0;

	char buffer[64 * 1024];
	buffer[0] = '\0';

	for (;;)
	{
		int rc = receiveBytesFromTcp(socket, buffer, sizeof(buffer) - 1, &timeoutValue);
		if (rc < 0)
		{
			pblCgiExitOnError("%s: readTcp failed! rc %d\n", tag, rc);
		}
		else if (rc == 0)
		{
			break;
		}
		buffer[rc] = '\0';

		if (rc < sizeof(buffer) - 1 && stringBuilder == NULL)
		{
			result = pblCgiStrDup(buffer);
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
		if (pblStringBuilderAppendStr(stringBuilder, buffer) == ((size_t)-1))
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
static void sendBytesToTcp(int socket, char * buffer, int nBytesToSend)
{
	static char * tag = "sendBytesToTcp";

	char * ptr = buffer;
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
static int connectToTcp(char * hostname, int port)
{
	static char * tag = "connectToTcp";

	errno = 0;
	struct hostent * hostInfo = gethostbyname(hostname);
	if (!hostInfo)
	{
		pblCgiExitOnError("%s: gethostbyname(%s) error, errno %d.\n", tag, hostname, errno);
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
	if (connect(socketFd, (struct sockaddr *) &serverAddress, sizeof(struct sockaddr_in)) < 0)
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
static char * getHttpResponse(char * hostname, int port, char * uri, int timeoutSeconds, char * agent)
{
	int socketFd = connectToTcp(hostname, port);

	char * sendBuffer = pblCgiSprintf("GET %s HTTP/1.0\r\nUser-Agent: %s\r\nHost: %s\r\n\r\n", uri, agent, hostname);
	PBL_CGI_TRACE("HttpRequest=%s", sendBuffer);

	sendBytesToTcp(socketFd, sendBuffer, strlen(sendBuffer));
	PBL_FREE(sendBuffer);

	char * response = receiveStringFromTcp(socketFd, timeoutSeconds);
	PBL_CGI_TRACE("HttpResponse=%s", response);
	socket_close(socketFd);

	return response;
}

static char * getMatchingString(char * string, char start, char end, char **nextPtr)
{
	char * tag = "getMatchingString";
	char * ptr = string;
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

static char * getStringBetween(char * string, char * start, char * end)
{
	char * tag = "getStringBetween";
	char * ptr = *start ? strstr(string, start) : string;
	if (!ptr)
	{
		pblCgiExitOnError("%s: expected starting tag '%s' in string '%s'\n", tag, start, string);
	}
	ptr += strlen(start);
	char * ptr2 = strstr(ptr, end);
	if (!ptr2)
	{
		pblCgiExitOnError("%s: expected ending '%s' in string '%s'\n", tag, end, ptr);
	}
	return pblCgiStrRangeDup(ptr, ptr2);
}

static char * getNumberString(char * string, char * start)
{
	char * tag = "getNumberString";
	char * ptr = strstr(string, start);
	if (!ptr)
	{
		pblCgiExitOnError("%s: expected starting tag '%s' in string '%s'\n", tag, start, string);
	}
	ptr += strlen(start);

	char * ptr2 = ptr;
	while (*ptr2)
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

static void putString(char * string, PblStringBuilder * stringBuilder)
{
	char * tag = "putString";

	if (pblStringBuilderAppendStr(stringBuilder, string) == ((size_t)-1))
	{
		pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
	}
	fputs(string, stdout);
}

static char * changeLat(char * string, int i, int difference)
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

	char * lat = getNumberString(string, "\"lat\":");
	//PBL_CGI_TRACE("lat=%s", lat);

	char * oldLat = pblCgiSprintf("\"lat\":%s,", lat);
	//PBL_CGI_TRACE("oldLat=%s", oldLat);

	char * newLat = pblCgiSprintf("\"lat\":%d,", atoi(lat) + difference);
	//PBL_CGI_TRACE("newLat=%s", newLat);

	char * replacedLat = pblCgiStrReplace(string, oldLat, newLat);

	PBL_FREE(lat);
	PBL_FREE(oldLat);
	PBL_FREE(newLat);

	return replacedLat;
}

static char * changeLon(char * string, int i, int difference)
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

	char * lon = getNumberString(string, "\"lon\":");
	//PBL_CGI_TRACE("lon=%s", lon);

	char * oldLon = pblCgiSprintf("\"lon\":%s,", lon);
	//PBL_CGI_TRACE("oldLon=%s", oldLon);

	char * newLon = pblCgiSprintf("\"lon\":%d,", atoi(lon) + difference);
	//PBL_CGI_TRACE("newLon=%s", newLon);

	char * replacedLon = pblCgiStrReplace(string, oldLon, newLon);

	PBL_FREE(lon);
	PBL_FREE(oldLon);
	PBL_FREE(newLon);

	return replacedLon;
}

static char * changeLayerName(char * string, char * layerName)
{
	if (!strstr(string, "layerName="))
	{
		return pblCgiStrDup(string);
	}

	char * oldLayerName = getStringBetween(string, "layerName=", "&");

	char * oldLayerNameStr = pblCgiSprintf("layerName=%s", oldLayerName);
	char * newLayerNameStr = pblCgiSprintf("layerName=%s", layerName);

	char * replacedString = pblCgiStrReplace(string, oldLayerNameStr, newLayerNameStr);

	PBL_FREE(oldLayerName);
	PBL_FREE(oldLayerNameStr);
	PBL_FREE(newLayerNameStr);

	return replacedString;
}

static char * changeLatAndLon(char * queryString, char * lat, char * lon, int * latDifference, int * lonDifference)
{
	if (!pblCgiStrIsNullOrWhiteSpace(lat) && !pblCgiStrIsNullOrWhiteSpace(lon))
	{
		char * replacementLat = pblCgiStrCat("lat=", lat);
		char * replacementLon = pblCgiStrCat("lon=", lon);
		int replacementLatInteger = (int)(1000000.0 * strtof(replacementLat + 4, NULL));
		int replacementLonInteger = (int)(1000000.0 * strtof(replacementLon + 4, NULL));
		int latPtrInteger = 0;
		int lonPtrInteger = 0;

		char * latPtr = strstr(queryString, "lat=");
		if (latPtr)
		{
			char * ptr = strstr(latPtr, "&");
			if (ptr)
			{
				latPtr = pblCgiStrRangeDup(latPtr, ptr);
			}
			else
			{
				latPtr = pblCgiStrDup(latPtr);
			}
			latPtrInteger = (int)(1000000.0 * strtof(latPtr + 4, NULL));
			queryString = pblCgiStrReplace(queryString, latPtr, replacementLat);
		}
		char * lonPtr = strstr(queryString, "lon=");
		if (lonPtr)
		{
			char * ptr = strstr(lonPtr, "&");
			if (ptr)
			{
				lonPtr = pblCgiStrRangeDup(lonPtr, ptr);
			}
			else
			{
				lonPtr = pblCgiStrDup(lonPtr);
			}
			lonPtrInteger = (int)(1000000.0 * strtof(lonPtr + 4, NULL));
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

static PblList * devicePositionList = NULL;

static char * handleDevicePosition(char * deviceId, char * queryString, int * latDifference, int * lonDifference)
{
	if (pblCgiStrIsNullOrWhiteSpace(deviceId))
	{
		return NULL;
	}

	char * lat = NULL;
	char * lon = NULL;

	if (!devicePositionList)
	{
		char * devicePositionValue = pblCgiConfigValue("DevicePosition", NULL);
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
		char * device = pblListGet(devicePositionList, i);

		if (pblCgiStrEquals(deviceId, device))
		{
			lat = pblListGet(devicePositionList, i + 1);
			lon = pblListGet(devicePositionList, i + 2);
			break;
		}
	}
	return changeLatAndLon(queryString, lat, lon, latDifference, lonDifference);
}

static void traceDuration()
{
	struct timeval now;
	gettimeofday(&now, NULL);

	unsigned long duration = now.tv_sec * 1000000 + now.tv_usec;
	duration -= pblCgiStartTime.tv_sec * 1000000 + pblCgiStartTime.tv_usec;
	char * string = pblCgiSprintf("%lu", duration);
	PBL_CGI_TRACE("Duration=%s microseconds", string);
}

extern int showDefaultLayer = 1;

static int arpoiseDirectory(int argc, char * argv[])
{
	char * tag = "ArpoiseDirectory";

	struct timeval startTime;
	gettimeofday(&startTime, NULL);

#ifdef _WIN32

	pblCgiConfigMap = pblCgiFileToMap(NULL, "../config/Win32ArpoiseDirectory.txt");

#else

	pblCgiConfigMap = pblCgiFileToMap(NULL, "../config/ArpoiseDirectory.txt");

#endif

	char * traceFile = pblCgiConfigValue(PBL_CGI_TRACE_FILE, "/tmp/ArpoiseDirectory.txt");
	pblCgiInitTrace(&startTime, traceFile);
	PBL_CGI_TRACE("argc %d argv[0] = %s", argc, argv[0]);

	pblCgiParseQuery(argc, argv);

	PBL_CGI_TRACE("-------> Directory Request\n");

	char * hostName = pblCgiConfigValue("HostName", "www.arpoise.com");
	if (pblCgiStrIsNullOrWhiteSpace(hostName))
	{
		pblCgiExitOnError("%s: HostName must be given.\n", tag);
	}
	PBL_CGI_TRACE("HostName=%s", hostName);

	int port = 80;
	char * portString = pblCgiConfigValue("Port", "80");
	if (!pblCgiStrIsNullOrWhiteSpace(portString))
	{
		int givenPort = atoi(portString);
		if (givenPort < 1)
		{
			pblCgiExitOnError("%s: Bad port %d.\n", tag, givenPort);
		}
		port = givenPort;
	}
	PBL_CGI_TRACE("Port=%d", port);

	char * baseUri = pblCgiConfigValue("BaseUri", "/php/dir/web/porpoise.php");
	if (pblCgiStrIsNullOrWhiteSpace(baseUri))
	{
		pblCgiExitOnError("%s: BaseUri must be given.\n", tag);
	}
	//PBL_CGI_TRACE("BaseUri=%s", baseUri);

	char * queryString = pblCgiQueryString;

	// handle fixed device positions
	//
	int latDifference = 0;
	int lonDifference = 0;

	char * userId = pblCgiQueryValue("userId");
	if (!userId || !*userId)
	{
		userId = "UnknownUserId";
	}

	char * deviceQueryString = handleDevicePosition(userId, queryString, &latDifference, &lonDifference);
	if (deviceQueryString != NULL)
	{
		queryString = deviceQueryString;
	}

#ifdef _WIN32

	// Initialize Winsock
	WSADATA wsaData;
	int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (result != 0)
	{
		pblCgiExitOnError("%s: WSAStartup failed: %d\n", tag, result);
	}
	//PBL_CGI_TRACE("WSAStartup=ok");

#endif

	char * uri = pblCgiSprintf("%s?%s", baseUri, queryString);
	//PBL_CGI_TRACE("Uri=%s", uri);

	char * response = getHttpResponse(hostName, port, uri, 16, pblCgiSprintf("ArpoiseClient %s", userId));
	//PBL_CGI_TRACE("Response=%s", response);

	char * cookie = NULL;
	if (strstr(response, "Set-Cookie: "))
	{
		cookie = getStringBetween(response, "Set-Cookie: ", "\r\n");
	}

	/*
	* check for HTTP error code like HTTP/1.1 500 Server Error
	*/
	char * ptr = strstr(response, "HTTP/");
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

	if (!ptr)
	{
		pblCgiExitOnError("%s: Expecting HTTP response\n%s\n", tag, response);
	}

	ptr = strstr(ptr, "\r\n\r\n");
	if (!ptr)
	{
		ptr = strstr(ptr, "\n\n");
		if (!ptr)
		{
			pblCgiExitOnError("%s: Illegal HTTP response, no separator.\n%s\n", tag, response);
		}
		else
		{
			ptr += 2;
		}
	}
	else
	{
		ptr += 4;
	}
	response = ptr;

	char * baseUrlStart = "\"baseURL\":\"";
	char * layerHost = NULL;
	int    layerPort = 80;
	char * layerUrl = NULL;
	char * layerName = NULL;

	char * start = "{\"hotspots\":";
	int length = strlen(start);

	int doChangeLatAndLon = 0;

	if (strncmp(start, response, length))
	{
		if (!showDefaultLayer)
		{
			fputs("Content-Type: application/json\r\n", stdout);
			if (cookie)
			{
				fputs("Set-Cookie: ", stdout);
				fputs(cookie, stdout);
				fputs("\r\n", stdout);
			}
			fputs("\r\n", stdout);
			fputs(response, stdout);
			PBL_CGI_TRACE("Response does not start with %s, no handling", start);
			return 0;
		}

		doChangeLatAndLon = 1;
		layerHost = "www.arpoise.com";
		layerUrl = "/php/porpoise/web/porpoise.php";
		layerName = "Reign-of-Gold";
	}
	else
	{
		ptr = strstr(response, baseUrlStart);
		if (ptr)
		{
			layerUrl = getStringBetween(ptr, baseUrlStart, "\"");
			while (strchr(layerUrl, '\\'))
			{
				layerUrl = pblCgiStrReplace(layerUrl, "\\", "");
			}
			ptr = strchr(layerUrl, '/');
			if (ptr)
			{
				layerHost = getStringBetween(layerUrl, "", "/");
				layerUrl = ptr;

				char * colon = strchr(layerHost, ':');
				if (colon)
				{
					*colon = '\0';
					layerPort = atoi(colon + 1);
				}
			}
		}

		if (!layerHost || !*layerHost || layerPort < 0 || layerPort > 0xffff || !layerUrl || !*layerUrl)
		{
			fputs("Content-Type: application/json\r\n", stdout);
			if (cookie)
			{
				fputs("Set-Cookie: ", stdout);
				fputs(cookie, stdout);
				fputs("\r\n", stdout);
			}
			fputs("\r\n", stdout);
			fputs(response, stdout);
			PBL_CGI_TRACE("Response does not contain proper 'baseURL' value, no handling");
			return 0;
		}
		PBL_CGI_TRACE("LayerUrl=%s", layerUrl);

		char * titleStart = "\"title\":\"";

		ptr = strstr(response, titleStart);
		if (ptr)
		{
			layerName = getStringBetween(ptr, titleStart, "\"");
		}

		if (!layerName || !*layerName)
		{
			fputs("Content-Type: application/json\r\n", stdout);
			if (cookie)
			{
				fputs("Set-Cookie: ", stdout);
				fputs(cookie, stdout);
				fputs("\r\n", stdout);
			}
			fputs("\r\n", stdout);
			fputs(response, stdout);
			PBL_CGI_TRACE("Response does not contain proper 'title' value, no handling");
			return 0;
		}
	}

	PBL_CGI_TRACE("LayerName=%s", layerName);
	PBL_CGI_TRACE("HostName=%s", layerHost);
	PBL_CGI_TRACE("Port=%d", layerPort);

	ptr = changeLayerName(queryString, layerName);
	if (doChangeLatAndLon)
	{
		ptr = changeLatAndLon(ptr, "48.158809", "11.580103", NULL, NULL);
	}
	uri = pblCgiSprintf("%s?%s", layerUrl, ptr);

	//PBL_CGI_TRACE("Layer Uri=%s", uri);

	response = getHttpResponse(layerHost, layerPort, uri, 16, "ArpoiseDirectory/1.10");
	//PBL_CGI_TRACE("Response=%s", response);

	cookie = NULL;
	if (strstr(response, "Set-Cookie: "))
	{
		cookie = getStringBetween(response, "Set-Cookie: ", "\r\n");
	}

	/*
	* check for HTTP error code like HTTP/1.1 500 Server Error
	*/
	ptr = strstr(response, "HTTP/");
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

	if (!ptr)
	{
		pblCgiExitOnError("%s: Expecting HTTP response\n%s\n", tag, response);
	}

	ptr = strstr(ptr, "\r\n\r\n");
	if (!ptr)
	{
		ptr = strstr(ptr, "\n\n");
		if (!ptr)
		{
			pblCgiExitOnError("%s: Illegal HTTP response, no separator.\n%s\n", tag, response);
		}
		else
		{
			ptr += 2;
		}
	}
	else
	{
		ptr += 4;
	}
	response = ptr;

	start = "{\"hotspots\":";
	length = strlen(start);

	if (strncmp(start, response, length))
	{
		fputs("Content-Type: application/json\r\n", stdout);
		if (cookie)
		{
			fputs("Set-Cookie: ", stdout);
			fputs(cookie, stdout);
			fputs("\r\n", stdout);
		}
		fputs("\r\n", stdout);
		fputs(response, stdout);
		PBL_CGI_TRACE("Response does not start with %s, no handling", start);
		return 0;
	}

	PblStringBuilder * stringBuilder = pblStringBuilderNew();
	if (!stringBuilder)
	{
		pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
	}

	char * rest = NULL;
	char * hotspotsString = getMatchingString(response + length, '[', ']', &rest);

	//PBL_CGI_TRACE("hotspotsString=%s", hotspotsString);
	//PBL_CGI_TRACE("rest=%s", rest);

	PblList * list = pblListNewArrayList();
	if (!list)
	{
		pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
	}

	ptr = hotspotsString;
	while (*ptr == '{')
	{
		char * ptr2 = NULL;
		char * hotspot = getMatchingString(ptr, '{', '}', &ptr2);

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

	fputs("Content-Type: application/json\r\n", stdout);
	if (cookie)
	{
		fputs("Set-Cookie: ", stdout);
		fputs(cookie, stdout);
		fputs("\r\n", stdout);
	}
	fputs("\r\n", stdout);
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

		char * hotspot = pblListGet(list, j);

		char * ptr = hotspot;
		if (latDifference != 0 || lonDifference != 0)
		{
			char * replacedLat = changeLat(hotspot, 1, -1 * latDifference);
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

	char * count = pblCgiQueryValue("count");
	if (count && !strcmp("1", count))
	{
		PBL_CGI_TRACE("-------> Statistics Request\n");

		// Create a web hit for the os and bundle, so that web stats can be used to count hits
		//
		char * versionsDirectory = pblCgiConfigValue("VersionsDirectory", "");
		if (versionsDirectory && *versionsDirectory)
		{
			char * os = pblCgiQueryValue("os");
			if (!os || !*os || strstr(os, ".."))
			{
				os = "UnknownOperatingSystem";
			}

			char * bundle = pblCgiQueryValue("bundle");
			if (!bundle || !*bundle || strstr(bundle, ".."))
			{
				bundle = "UnknownBundle";
			}

			char * fileName = pblCgiSprintf("%s_%s.htm", os, bundle);
			char * filePath = pblCgiSprintf("%s/%s", versionsDirectory, fileName);

			FILE * stream = NULL;

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

			uri = pblCgiSprintf("/versions/%s", fileName);
			getHttpResponse("www.arpoise.com", 80, uri, 16, "ArpoiseDirectory/versions");
		}

		// Create a web hit for the layer, so that web stats can be used to count hits
		//
		char * layerNamesDirectory = pblCgiConfigValue("LayerNamesDirectory", "");
		if (layerNamesDirectory && *layerNamesDirectory && layerName && *layerName)
		{
			char * fileName = pblCgiSprintf("%s.htm", layerName);
			char * filePath = pblCgiSprintf("%s/%s", layerNamesDirectory, fileName);

			FILE * stream = NULL;

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

			uri = pblCgiSprintf("/layernames/%s", fileName);
			getHttpResponse("www.arpoise.com", 80, uri, 16, "ArpoiseDirectory/layernames");
		}
	}

	return 0;
}

int main(int argc, char * argv[])
{
	int rc = arpoiseDirectory(argc, argv);
	traceDuration();
	return rc;
}
