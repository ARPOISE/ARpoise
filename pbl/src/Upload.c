/*
Upload.c - main for file upload front end service.

Copyright (C) 2020, Tamiko Thiel and Peter Graf - All Rights Reserved

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

$Log: Upload.c,v $
Revision 1.7  2023/08/28 20:30:43  peter
Added handling of old asset bundles on arpoise.com

Revision 1.6  2021/08/26 18:51:03  peter
Client specific area values


*/

/*
* Make sure "strings <exe> | grep Id | sort -u" shows the source file versions
*/
char* Upload_c_id = "$Id: Upload.c,v 1.7 2023/08/28 20:30:43 peter Exp $";

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

extern char* adbGetHttpResponse(char* hostname, int port, char* uri, int timeoutSeconds, char* agent);
extern char* adbGetStringBetween(char* string, char* start, char* end);
extern char* adbGetHttpResponseBody(char* response, char** cookiePtr);
extern char* adbChangeLatAndLon(char* queryString, char* lat, char* lon, int* latDifference, int* lonDifference);
extern char* adbChangeRedirectionUrl(char* string, char* redirectionUrl);
extern char* adbChangeLayer(char* string, char* layer);
extern char* adbChangeRedirectionLayer(char* string, char* redirectionLayer);
extern char* adbChangeLayerName(char* string, char* layerName);
extern char* adbChangeShowMenuOption(char* string, char* value);
extern char* adbHandleDevicePosition(char* deviceId, char* clientApplication, char* queryString, int* latDifference, int* lonDifference);
extern void adbTraceDuration();
extern void adbPrintHeader(char* cookie);
extern void adbHandleResponse(char* response, int latDifference, int lonDifference, int bundleInteger);
extern void adbCreateStatisticsHits(int layer, char* layerName, int layerServed);
extern char* adbGetArea(char* queryString, char* clientApplication);
extern char* adbGetAreaConfigValue(char* area, char* key, char* defaultValue);

static int uploadFile()
{
	char* tag = "UploadFile";

	PBL_CGI_TRACE(">>> %s", tag);

	char* boundary = NULL;
	char* ptr = pblCgiGetEnv("CONTENT_TYPE");
	if (ptr && *ptr)
	{
		char* values[2 + 1];
		char* keyValuePair[2 + 1];

		int n = pblCgiStrSplit(ptr, " ", 2, values);
		if (n > 1)
		{
			if (pblCgiStrEquals("multipart/form-data;", values[0]))
			{
				pblCgiStrSplit(values[1], "=", 2, keyValuePair);
				if (pblCgiStrEquals("boundary", keyValuePair[0]))
				{
					boundary = keyValuePair[1];
				}
			}
		}
	}

	if (!boundary || !*boundary)
	{
		pblCgiExitOnError("%s: No multipart/form-data boundary defined\n", tag);
	}
	//PBL_CGI_TRACE(">>boundary '%s'", boundary);

	ptr = pblCgiPostData;
	if (!ptr || !*ptr)
	{
		pblCgiExitOnError("%s: No POST data defined\n", tag, boundary);
	}
	//PBL_CGI_TRACE(">>post data %s", ptr);

	char* found = strstr(ptr, boundary);
	if (!found || !*found)
	{
		pblCgiExitOnError("%s: No multipart/form-data boundary '%s' found in post data '%s'\n", tag, boundary, ptr);
	}

	char* contentTypeTag = "Content-Type: ";
	char* contentType = strstr(pblCgiPostData, contentTypeTag);
	if (!contentType || !*contentType)
	{
		pblCgiExitOnError("%s: No Content-Type tag '%s' defined in post data %s\n", tag, contentTypeTag, pblCgiPostData);
	}
	contentType = contentType + strlen(contentTypeTag);
	if (!contentType || !*contentType)
	{
		pblCgiExitOnError("%s: No Content-Type value defined in post data %s\n", tag, pblCgiPostData);
	}
	ptr = contentType;
	while (!isspace(*ptr++));
	contentType = pblCgiStrRangeDup(contentType, ptr);
	if (!contentType || !*contentType)
	{
		pblCgiExitOnError("%s: No Content-Type value found in post data %s\n", tag, pblCgiPostData);
	}

	char* end = strstr(pblCgiPostData, "\r\n\r\n");
	if (!end)
	{
		end = strstr(ptr, "\n\n");
		if (!end)
		{
			pblCgiExitOnError("%s: Illegal header, terminating newlines are missing: '%s'\n", tag, ptr);
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
	ptr = end;

	int length = pblCgiContentLength - (ptr - pblCgiPostData) -strlen(boundary) - 6;
	//PBL_CGI_TRACE(">>content length %d, type %s, data %s", length, contentType, ptr);

	char* uploadDirectoryPath = pblCgiConfigValue("UploadDirectoryPath", "/tmp/");

	char* extension = NULL;
	if (pblCgiStrEquals("image/png", contentType))
	{
		extension = "png";
	} 
	else if (pblCgiStrEquals("image/jpeg", contentType))
	{
		extension = "jpg";
	}
	else
	{
		pblCgiExitOnError("%s: Cannot handle content type '%s'", tag, contentType);
	}

	struct timeval startTime;
	gettimeofday(&startTime, NULL);
	time_t now = time(NULL);
	char* timeString = pblCgiStrFromTimeAndFormat(now, "%02d%02d%02d%02d%02d%02d");

	srand(rand() ^ (unsigned int)now ^ length);
	char *fileName = pblCgiSprintf("%s-%08x%08x.%s", timeString, rand(), rand(), extension);
	char* filePath = pblCgiSprintf("%s/upload-%s", uploadDirectoryPath, fileName);
	char* tempPath = pblCgiSprintf("%s/temp-%s", uploadDirectoryPath, fileName);

	FILE* stream;
	if (!(stream = pblCgiFopen(tempPath, "w")))
	{
		pblCgiExitOnError("%s: Failed to open file '%s'", tag, tempPath);
	}

	end = ptr + length;
	while (ptr < end)
	{
		fputc(*ptr++, stream);
	}
	fclose(stream);

	if (rename(tempPath, filePath))
	{
		pblCgiExitOnError("%s: Failed to rename file '%s' to '%s'", tag, tempPath, filePath);
	}

	PBL_CGI_TRACE("<<< %s: File '%s', length %d", tag, filePath, length);
	return 0;
}

static int upload(int argc, char* argv[])
{
	char* tag = "Upload";
	int layerServed = 0;
	int layer = 0;

	struct timeval startTime;
	gettimeofday(&startTime, NULL);
	srand(rand() ^ getpid() ^ startTime.tv_sec ^ startTime.tv_usec);

#ifdef _WIN32

	pblCgiConfigMap = pblCgiFileToMap(NULL, "../config/Win32Upload.txt");

#else

	pblCgiConfigMap = pblCgiFileToMap(NULL, "../config/Upload.txt");

#endif

	char* traceFile = pblCgiConfigValue(PBL_CGI_TRACE_FILE, "/tmp/Upload.txt");
	pblCgiInitTrace(&startTime, traceFile);
	PBL_CGI_TRACE("argc %d argv[0] = %s", argc, argv[0]);

	pblCgiParseQuery(argc, argv);
	char* queryString = pblCgiQueryString;

#ifdef _WIN32

	// Initialize Winsock
	WSADATA wsaData;
	int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (result != 0)
	{
		pblCgiExitOnError("%s: WSAStartup failed: %d\n", tag, result);
	}

#endif

	// read query values
	//
	char* action = pblCgiQueryValue("action");
	if (pblCgiStrEquals("upload", action))
	{
		uploadFile();
	}

	return 0;
}

static char* contentType = NULL;
static void pblCgiSetContentType(char* type)
{
	if (!contentType)
	{
		char* cookie = pblCgiValue(PBL_CGI_COOKIE);
		char* cookiePath = pblCgiValue(PBL_CGI_COOKIE_PATH);
		char* cookieDomain = pblCgiValue(PBL_CGI_COOKIE_DOMAIN);

		contentType = type;

		if (cookie && cookiePath && cookieDomain)
		{
			char* format = "Content-Type: %s\n";
			printf(format, contentType);
			PBL_CGI_TRACE(format, contentType);

			format = "Set-Cookie: %s%s; Path=%s; DOMAIN=%s; HttpOnly\n\n";
			printf(format, pblCgiCookieTag, cookie, cookiePath, cookieDomain);
			PBL_CGI_TRACE(format, pblCgiCookieTag, cookie, cookiePath, cookieDomain);
		}
		else
		{
			printf("Content-Type: %s\n\n", contentType);
			PBL_CGI_TRACE("Content-Type: %s\n", contentType);
		}
	}
}

int main(int argc, char* argv[])
{
	int rc = upload(argc, argv);
	adbTraceDuration();

	pblCgiSetContentType("text/html");

	printf(
		"<!DOCTYPE html>\n"
		"<html>\n"
		"<head>\n<title>Mission-Base PBL CGI Error</title>\n</head>\n"
		"<body>\n"
		"<h1>PBL CGI\n</h1>\n"
		"<p><hr><p>\n"
		"<h2>Upload OK</h2>\n");
	return rc;
}
