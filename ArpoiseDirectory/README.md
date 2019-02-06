![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise-Directory Frontend

## Overview
ARpoiseDirectory is a cgi-bin program written in C.
It acts as a cgi-filter between the ARpoise client app and the ARpoise directory backend.

## Functionality
After receiving a request from the ARpoise client app,
it connects to the ARpoise directory backend and queries whether there are any layers within the user's range.

If so, it redirects the user's client ARpoise app to the url and layer name of the layer closest to the client.

If not so, it returns the information of the default layer to the client.

## Note
For compiling, you also need the clone the pbl
[repository](../pbl/src/).
