/*
ArpoiseDirectory.c - main for ARpoise Directory front end service.

Copyright (C) 2018, Tamiko Thiel and Peter Graf - All Rights Reserved

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
ARpoise, see http://www.ARpoise.com/

$Log: ArpoiseDirectory.c,v $
Revision 1.55  2021/08/26 18:51:03  peter
Client specific area values

*/

/*
* Make sure "strings <exe> | grep Id | sort -u" shows the source file versions
*/
char* ArpoiseDirectory_c_id = "$Id: ArpoiseDirectory.c,v 1.55 2021/08/26 18:51:03 peter Exp $";

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

extern char* ArvosApplicationName;
extern char* ArpoiseApplicationName;
extern char* OperatingSystemAndroid;
extern char* OperatingSystemiOS;

extern char* adbGetHttpResponse(char* hostname, int port, char* uri, int timeoutSeconds, char* agent);
extern char* adbGetStringBetween(char* string, char* start, char* end);
extern char* adbGetHttpResponseBody(char* response, char** cookiePtr);
extern char* adbChangeLatAndLon(char* queryString, char* lat, char* lon, int* latDifference, int* lonDifference);
extern char* adbChangeRedirectionUrl(char* string, char* redirectionUrl);
extern char* adbChangeLayer(char* string, char* layer);
extern char* adbChangeRedirectionLayer(char* string, char* redirectionLayer);
extern char* adbChangeLayerName(char* string, char* layerName);
extern char* adbChangeShowMenuOption(char* string, char* value);
extern char* adbHandleDevicePosition(char* deviceId, char* client, char* queryString, int* latDifference, int* lonDifference);
extern void adbTraceDuration();
extern void adbPrintHeader(char* cookie);
extern void adbHandleResponse(char* response, int latDifference, int lonDifference);
extern void adbCreateStatisticsHits(int layer, char* layerName, int layerServed);
extern char* adbGetArea(char* queryString, char* clientApplication);
extern char* adbGetAreaConfigValue(char* area, char* key, char* defaultValue);

static char* getVersion()
{
	return adbGetStringBetween(ArpoiseDirectory_c_id, "ArpoiseDirectory.c,v ", " ");
}

char* exponentiARGrowth(int exponent);

static int arpoiseDirectory(int argc, char* argv[])
{
	char* tag = "ArpoiseDirectory";
	int layerServed = 0;
	int layer = 0;

	struct timeval startTime;
	gettimeofday(&startTime, NULL);

#ifdef _WIN32

	pblCgiConfigMap = pblCgiFileToMap(NULL, "../config/Win32ArpoiseDirectory.txt");

#else

	pblCgiConfigMap = pblCgiFileToMap(NULL, "../config/ArpoiseDirectory.txt");

#endif

	char* traceFile = pblCgiConfigValue(PBL_CGI_TRACE_FILE, "/tmp/ArpoiseDirectory.txt");
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
	char* clientApplication = pblCgiQueryValue("client");
	char* deviceId = pblCgiQueryValue("deviceId");
	if (!deviceId || !*deviceId)
	{
		deviceId = pblCgiQueryValue("userId");
		if (!deviceId || !*deviceId)
		{
			deviceId = "UnknownDeviceId";
		}
	}

	// handle fixed device positions
	//
	int latDifference = 0;
	int lonDifference = 0;
	char* deviceQueryString = adbHandleDevicePosition(deviceId, clientApplication, queryString, &latDifference, &lonDifference);
	if (deviceQueryString != NULL)
	{
		queryString = deviceQueryString;
	}

	char* layerName = pblCgiQueryValue("layerName");
	char* layerUrl = "";
	char* uri = "";
	char* area = adbGetArea(queryString, clientApplication);

	if (pblCgiStrEquals("true", pblCgiQueryValue("innerLayer"))
		&& pblCgiStrEquals("0.000000", pblCgiQueryValue("lat"))
		&& pblCgiStrEquals("0.000000", pblCgiQueryValue("lon")))
	{
		// an inner layer request for a default layer
		//
		queryString = pblCgiQueryString;
	}

	// Read config values
	//
	char* hostName = adbGetAreaConfigValue(area, "HostName", "www.arpoise.com");
	if (pblCgiStrIsNullOrWhiteSpace(hostName))
	{
		pblCgiExitOnError("%s: HostName must be given.\n", tag);
	}
	PBL_CGI_TRACE("HostName=%s", hostName);

	int port = 80;
	char* portString = adbGetAreaConfigValue(area, "Port", "80");
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

	char* directoryUri = adbGetAreaConfigValue(area, "DirectoryUri", "/php/dir/web/porpoise.php");
	if (pblCgiStrIsNullOrWhiteSpace(directoryUri))
	{
		pblCgiExitOnError("%s: DirectoryUri must be given.\n", tag);
	}

	int isDirectoryRequest = pblCgiStrEquals(layerName, "Arpoise-Directory");
	if (isDirectoryRequest)
	{
		// See what operating system it is
		char* os = pblCgiQueryValue("os");
		if (!os || !*os)
		{
			os = "UnknownOperatingSystem";
		}

		int bundleInteger = 0;
		char* bundle = pblCgiQueryValue("bundle");
		if (bundle && isdigit(*bundle))
		{
			bundleInteger = atoi(bundle);
		}

		// This is a request for the Arpoise-Directory layer

		PBL_CGI_TRACE("-------> Directory Request\n");

		// See what client application it is
		if (pblCgiStrEquals(ArvosApplicationName, clientApplication))
		{
			if (pblCgiStrEquals(OperatingSystemAndroid, os) && bundleInteger < 200101)
			{
				// Request the default layer from porpoise and return it to the client

				layerUrl = adbGetAreaConfigValue(area, "ArvosDefaultLayerUrl", "/php/porpoise/web/porpoise.php");
				layerName = adbGetAreaConfigValue(area, "ArvosDefaultLayerName", "Default-ImageTrigger");

				layerServed = 1;
				PBL_CGI_TRACE("-------> Arvos Default Layer Request: '%s' '%s'\n", layerUrl, layerName);

				char* ptr = adbChangeLayerName(queryString, layerName);

				int myLatDifference = 0;
				int myLonDifference = 0;
				ptr = adbChangeLatAndLon(ptr, "0.000000", "0.000000", &myLatDifference, &myLonDifference);
				latDifference += myLatDifference;
				lonDifference += myLonDifference;

				uri = pblCgiSprintf("%s?p=%d&%s", layerUrl, getpid(), ptr);
				char* agent = pblCgiSprintf("ArpoiseDirectory/%s", getVersion());
				char* response = adbGetHttpResponse(hostName, port, uri, 16, agent);
				adbHandleResponse(response, latDifference, lonDifference);

				adbCreateStatisticsHits(layer, layerName, layerServed);
				return 0;
			}

			// Request the AR-vos-Directory

			queryString = adbChangeLayerName(queryString, "AR-vos-Directory");
		}
		if (pblCgiStrEquals("Arslam", clientApplication))
		{
			// Request the default layer from porpoise and return it to the client

			layerUrl = adbGetAreaConfigValue(area, "ArslamDefaultLayerUrl", "/php/porpoise/web/porpoise.php");
			layerName = adbGetAreaConfigValue(area, "ArslamDefaultLayerName", "Default-Slam");

			layerServed = 1;
			PBL_CGI_TRACE("-------> Arslam Default Layer Request: '%s' '%s'\n", layerUrl, layerName);

			char* ptr = adbChangeLayerName(queryString, layerName);

			int myLatDifference = 0;
			int myLonDifference = 0;
			ptr = adbChangeLatAndLon(ptr, "0.000000", "0.000000", &myLatDifference, &myLonDifference);
			latDifference += myLatDifference;
			lonDifference += myLonDifference;

			uri = pblCgiSprintf("%s?p=%d&%s", layerUrl, getpid(), ptr);
			char* agent = pblCgiSprintf("ArpoiseDirectory/%s", getVersion());
			char* response = adbGetHttpResponse(hostName, port, uri, 16, agent);
			response = adbChangeShowMenuOption(response, "false");
			adbHandleResponse(response, latDifference, lonDifference);

			adbCreateStatisticsHits(layer, layerName, layerServed);
			return 0;
		}

		uri = pblCgiSprintf("%s?p=%d&%s", directoryUri, getpid(), queryString);
		char* cookie = NULL;

		char* httpResponse = adbGetHttpResponse(hostName, port, uri, 16, pblCgiSprintf("ArpoiseClient %s", deviceId));
		char* response = adbGetHttpResponseBody(httpResponse, &cookie);

		char* start = "{\"hotspots\":";
		int length = strlen(start);

		if (strncmp(start, response, length))
		{
			// There is nothing at the location the client is at
			char* defaultDirectory = adbGetAreaConfigValue(area, "DefaultDirectory", "");
			char* arvosDefaultDirectory = adbGetAreaConfigValue(area, "ArvosDefaultDirectory", "");

			if (pblCgiStrEquals(ArvosApplicationName, clientApplication))
			{
				if (!pblCgiStrIsNullOrWhiteSpace(arvosDefaultDirectory) &&
					((pblCgiStrEquals(OperatingSystemAndroid, os) && bundleInteger >= 190208)
						|| (pblCgiStrEquals(OperatingSystemiOS, os) && bundleInteger >= 20190208)))
				{
					char* ptr = adbChangeLayerName(queryString, arvosDefaultDirectory);

					int myLatDifference = 0;
					int myLonDifference = 0;
					ptr = adbChangeLatAndLon(ptr, "0.000000", "0.000000", &myLatDifference, &myLonDifference);
					latDifference += myLatDifference;
					lonDifference += myLonDifference;

					uri = pblCgiSprintf("%s?p=%d&%s", directoryUri, getpid(), ptr);
					char* agent = pblCgiSprintf("ArpoiseDirectory/%s", getVersion());
					cookie = NULL;

					httpResponse = adbGetHttpResponse(hostName, port, uri, 16, agent);
					response = adbGetHttpResponseBody(httpResponse, &cookie);

					start = "{\"hotspots\":";
					length = strlen(start);

					if (strncmp(start, response, length))
					{
						latDifference -= myLatDifference;
						lonDifference -= myLonDifference;
					}
					else
					{
						httpResponse = adbChangeLayer(httpResponse, "Arpoise-Directory");
						response = adbGetHttpResponseBody(httpResponse, &cookie);
					}
				}
				else
				{
					// Request the default layer from porpoise and return it to the client

					layerUrl = adbGetAreaConfigValue(area, "ArvosDefaultLayerUrl", "/php/porpoise/web/porpoise.php");
					layerName = adbGetAreaConfigValue(area, "ArvosDefaultLayerName", "Default-ImageTrigger");

					layerServed = 1;
					PBL_CGI_TRACE("-------> Arvos Default Layer Request: '%s' '%s'\n", layerUrl, layerName);

					char* ptr = adbChangeLayerName(queryString, layerName);

					int myLatDifference = 0;
					int myLonDifference = 0;
					ptr = adbChangeLatAndLon(ptr, "0.000000", "0.000000", &myLatDifference, &myLonDifference);
					latDifference += myLatDifference;
					lonDifference += myLonDifference;

					uri = pblCgiSprintf("%s?p=%d&%s", layerUrl, getpid(), ptr);
					char* agent = pblCgiSprintf("ArpoiseDirectory/%s", getVersion());
					response = adbGetHttpResponse(hostName, port, uri, 16, agent);
					adbHandleResponse(response, latDifference, lonDifference);

					adbCreateStatisticsHits(layer, layerName, layerServed);
					return 0;
				}
			}
			else if (!pblCgiStrIsNullOrWhiteSpace(defaultDirectory) &&
				((pblCgiStrEquals(OperatingSystemAndroid, os) && bundleInteger >= 190208)
					|| (pblCgiStrEquals(OperatingSystemiOS, os) && bundleInteger >= 20190208)))
			{
				char* ptr = adbChangeLayerName(queryString, defaultDirectory);

				int myLatDifference = 0;
				int myLonDifference = 0;
				ptr = adbChangeLatAndLon(ptr, "0.000000", "0.000000", &myLatDifference, &myLonDifference);
				latDifference += myLatDifference;
				lonDifference += myLonDifference;

				uri = pblCgiSprintf("%s?p=%d&%s", directoryUri, getpid(), ptr);
				char* agent = pblCgiSprintf("ArpoiseDirectory/%s", getVersion());
				cookie = NULL;

				httpResponse = adbGetHttpResponse(hostName, port, uri, 16, agent);
				response = adbGetHttpResponseBody(httpResponse, &cookie);

				start = "{\"hotspots\":";
				length = strlen(start);

				if (strncmp(start, response, length))
				{
					latDifference -= myLatDifference;
					lonDifference -= myLonDifference;
				}
				else
				{
					httpResponse = adbChangeLayer(httpResponse, "Arpoise-Directory");
					response = adbGetHttpResponseBody(httpResponse, &cookie);
				}
			}
		}

		if (strncmp(start, response, length))
		{
			// There is nothing at the location the client is at

			// Request the default layer from porpoise and return it to the client

			layerUrl = adbGetAreaConfigValue(area, "DefaultLayerUrl", "/php/porpoise/web/porpoise.php");
			layerName = adbGetAreaConfigValue(area, "DefaultLayerName", "Default-Layer-Reign-of-Gold");

			if ((pblCgiStrEquals(OperatingSystemAndroid, os) && bundleInteger >= 190310)
				|| (pblCgiStrEquals(OperatingSystemiOS, os) && bundleInteger >= 20190310)
				)
			{
				char* layername190310 = adbGetAreaConfigValue(area, "DefaultLayerName190310", "");
				if (layername190310 && *layername190310)
				{
					layerName = layername190310;
				}
			}

			layerServed = 1;
			PBL_CGI_TRACE("-------> Default Layer Request: '%s' '%s'\n", layerUrl, layerName);

			char* ptr = adbChangeLayerName(queryString, layerName);

			int myLatDifference = 0;
			int myLonDifference = 0;
			ptr = adbChangeLatAndLon(ptr, "0.000000", "0.000000", &myLatDifference, &myLonDifference);
			latDifference += myLatDifference;
			lonDifference += myLonDifference;

			uri = pblCgiSprintf("%s?p=%d&%s", layerUrl, getpid(), ptr);
			char* agent = pblCgiSprintf("ArpoiseDirectory/%s", getVersion());
			char* httpResponse = adbGetHttpResponse(hostName, port, uri, 16, agent);
			adbHandleResponse(httpResponse, latDifference, lonDifference);
		}
		else
		{
			// There is at least one layer at the location the client is at

			// If there is more than one layer,
			// and the client can handle the response of the directory request,
			// send the response back to the client

			int numberOfHotspots = 0;
			char* numberOfHotspotsString = adbGetStringBetween(response, "\"numberOfHotspots\":", ",\"");
			if (numberOfHotspotsString && isdigit(*numberOfHotspotsString))
			{
				numberOfHotspots = atoi(numberOfHotspotsString);
			}

			if (numberOfHotspots > 1
				&& ((pblCgiStrEquals(OperatingSystemAndroid, os) && bundleInteger >= 190208)
					|| (pblCgiStrEquals(OperatingSystemiOS, os) && bundleInteger >= 20190208)
					)
				)
			{
				PBL_CGI_TRACE("-------> Client response");

				adbHandleResponse(httpResponse, latDifference, lonDifference);
			}
			else
			{
				char* baseUrlStart = "\"baseURL\":\"";
				char* ptr = strstr(response, baseUrlStart);
				if (ptr)
				{
					layerUrl = adbGetStringBetween(ptr, baseUrlStart, "\"");
					while (strchr(layerUrl, '\\'))
					{
						layerUrl = pblCgiStrReplace(layerUrl, "\\", "");
					}
				}

				if (!layerUrl || !*layerUrl)
				{
					adbPrintHeader(cookie);
					fputs(response, stdout);
					PBL_CGI_TRACE("Response does not contain proper 'baseURL' value, no handling");
					return 0;
				}

				char* titleStart = "\"title\":\"";
				ptr = strstr(response, titleStart);
				if (ptr)
				{
					layerName = adbGetStringBetween(ptr, titleStart, "\"");
				}

				if (!layerName || !*layerName)
				{
					adbPrintHeader(cookie);
					fputs(response, stdout);
					PBL_CGI_TRACE("Response does not contain proper 'title' value, no handling");
					return 0;
				}

				// Redirect the client to the url and layer specified

				layer = 1;
				ptr = adbChangeRedirectionUrl(response, layerUrl);
				ptr = adbChangeRedirectionLayer(ptr, layerName);

				adbPrintHeader(cookie);
				fputs(ptr, stdout);
				PBL_CGI_TRACE("-------> Client redirect: '%s' '%s'", layerUrl, layerName);
			}
		}
	}
	else
	{
		// This is a request for a specific layer, request the layer from porpoise and return it to the client
		char* porpoiseUri = adbGetAreaConfigValue(area, "PorpoiseUri", "/php/porpoise/web/porpoise.php");
		if (pblCgiStrIsNullOrWhiteSpace(porpoiseUri))
		{
			pblCgiExitOnError("%s: PorpoiseUri must be given.\n", tag);
		}
		layerServed = 1;

		char* defaultStr = "Default-";
		char* exponentialStr = "ExponentiARGrowth-";

		if (!pblCgiStrIsNullOrWhiteSpace(layerName)
			&& !strncmp(layerName, exponentialStr, strlen(exponentialStr)))
		{
			PBL_CGI_TRACE("-------> ExponentiARGrowth Layer Request: '%s'\n", layerName);

			int exponent = atoi(layerName + strlen(exponentialStr));
			if (exponent > 0)
			{
				if (exponent > 10)
				{
					exponent = 10;
				}
				adbHandleResponse(exponentiARGrowth(exponent), latDifference, lonDifference);
			}
		}
		else
		{
			if (!pblCgiStrIsNullOrWhiteSpace(layerName)
				&& !strncmp(layerName, defaultStr, strlen(defaultStr))
				&& !pblCgiStrEquals("0.000000", pblCgiQueryValue("lat"))
				&& !pblCgiStrEquals("0.000000", pblCgiQueryValue("lon")))
			{
				PBL_CGI_TRACE("-------> Default Layer Request: '%s' '%s'\n", porpoiseUri, layerName);

				int myLatDifference = 0;
				int myLonDifference = 0;
				queryString = adbChangeLatAndLon(queryString, "0.000000", "0.000000", &myLatDifference, &myLonDifference);
				latDifference += myLatDifference;
				lonDifference += myLonDifference;
			}
			else
			{
				PBL_CGI_TRACE("-------> Layer Request: '%s' '%s'\n", porpoiseUri, layerName);
			}

			uri = pblCgiSprintf("%s?p=%d&%s", porpoiseUri, getpid(), queryString);
			char* agent = pblCgiSprintf("ArpoiseFilter/%s", getVersion());
			adbHandleResponse(adbGetHttpResponse(hostName, port, uri, 16, agent), latDifference, lonDifference);
		}
	}

	adbCreateStatisticsHits(layer, layerName, layerServed);
	return 0;
}

int main(int argc, char* argv[])
{
	int rc = arpoiseDirectory(argc, argv);
	adbTraceDuration();
	return rc;
}

char* exponentiARGrowth(int exponent)
{
	static char* tag = "exponentiARGrowth";
	char* expResponseStart = "HTTP/1.1 200 OK\r\n\r\n{\"hotspots\":[";
	char* expResponseEnd = "],\"radius\":0,\"numberOfHotspots\":1,\"refreshInterval\":0,\"showMenuButton\":true,\"noPoisMessage\":\"Sorry, there is nothing to show!\",\"actions\":[{\"uri\":\"\",\"label\":\"Arpoise\",\"contentType\":\"\",\"method\":\"GET\",\"activityType\":0,\"params\":[],\"closeBiw\":false,\"showActivity\":true,\"activityMessage\":\"\"}],\"morePages\":false,\"nextPageKey\":\"\",\"layer\":\"ExponentiARGrowth-2\",\"errorCode\":0,\"errorString\":\"ok\"}";

	char* hotSpot = "{\
		\"dimension\": 3,\
		\"transform\" : {\
			\"rel\": false,\
			\"angle\" : 0,\
			\"scale\" : 0.3\
	    },\
		\"object\" : {\
			\"baseURL\": \"www.arpoise.com/AB/nothingofhim.ace\",\
			\"full\" : \"NoH_BottleWaterlily\",\
			\"poiLayerName\" : null,\
			\"relativeLocation\" : \"{locationX},0,{locationZ}\",\
			\"icon\" : null,\
			\"size\" : 0,\
			\"triggerImageURL\" : null,\
			\"triggerImageWidth\" : 0\
		},\
	    \"actions\": [] ,\
		\"animations\" : {\
			\"onCreate\": [\
			{\
				\"type\": \"rotate\",\
				\"length\" : {length},\
				\"interpolation\" : \"sine\",\
				\"repeat\" : true,\
				\"to\" : {to},\
				\"axis\" : {\
					\"x\": 0,\
					\"y\" : 1,\
					\"z\" : 0\
				}\
			}\
			]\
		},\
		\"distance\": 0,\
		\"id\" : \"{id}\",\
		\"lat\" : 0,\
		\"lon\" : 0,\
		\"type\" : 0\
    }";

	PblStringBuilder* stringBuilder = pblStringBuilderNew();
	if (!stringBuilder)
	{
		pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
	}

	pblStringBuilderAppendStr(stringBuilder, expResponseStart);

	int n = 1;
	for (int i = 0; i < exponent; i++)
	{
		n *= 2;
	}
	n -= 1;

	for (int i = 0; i < n; i++)
	{
		int length = 15 + rand() % 15;
		int to = 20 + rand() % 20;
		float locationX = (-2000 + rand() % 4000) / (float)1000;
		float locationZ = (-2000 + rand() % 4000) / (float)1000;

		char* replacementPtr = pblCgiSprintf("%f", locationX);
		char* hotSpotPtr = pblCgiStrReplace(hotSpot, "{locationX}", replacementPtr);
		free(replacementPtr);

		replacementPtr = pblCgiSprintf("%f", locationZ);
		char* tempPtr = hotSpotPtr;
		hotSpotPtr = pblCgiStrReplace(tempPtr, "{locationZ}", replacementPtr);
		free(replacementPtr);
		free(tempPtr);

		replacementPtr = pblCgiSprintf("%d", length);
		tempPtr = hotSpotPtr;
		hotSpotPtr = pblCgiStrReplace(tempPtr, "{length}", replacementPtr);
		free(replacementPtr);
		free(tempPtr);

		replacementPtr = pblCgiSprintf("%d", to);
		tempPtr = hotSpotPtr;
		hotSpotPtr = pblCgiStrReplace(tempPtr, "{to}", replacementPtr);
		free(replacementPtr);
		free(tempPtr);

		replacementPtr = pblCgiSprintf("%d", i);
		tempPtr = hotSpotPtr;
		hotSpotPtr = pblCgiStrReplace(tempPtr, "{id}", replacementPtr);
		free(replacementPtr);
		free(tempPtr);

		if (i > 0)
		{
			pblStringBuilderAppendStr(stringBuilder, ",");
		}
		pblStringBuilderAppendStr(stringBuilder, hotSpotPtr);
		free(hotSpotPtr);
	}
	pblStringBuilderAppendStr(stringBuilder, expResponseEnd);

	char* result = pblStringBuilderToString(stringBuilder);
	if (!result)
	{
		pblCgiExitOnError("%s: pbl_errno = %d, message='%s'\n", tag, pbl_errno, pbl_errstr);
	}

	if (stringBuilder)
	{
		pblStringBuilderFree(stringBuilder);
	}
	return result;
}
