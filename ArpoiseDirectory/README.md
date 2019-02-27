![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise-Directory Frontend

## Overview
ARpoiseDirectory is a cgi-bin program written in C.
It acts as a cgi-filter between the ARpoise client app and the ARpoise directory backend.

## Functionality
After receiving a request from the ARpoise client app,
it connects to the ARpoise directory backend and queries whether there are any layers within the user's range.

If there are more than one layer in the user's range, it returns the list of available layers to the client.

If there is exactly one layer, it redirects the user's client ARpoise app to the that layer.

If not so, redirects the user's client ARpoise app to the ARpoise default layer.

## Note
For compiling you also need the pbl
[repository](../pbl/src/).
