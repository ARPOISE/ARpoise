![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise-Directory Front End

## Overview
ARpoiseDirectory is a cgi-bin program written in C.
The program acts as a cgi-filter between the ARpoise client app and the ARpoise directory back end.

## Functionality
After receiving a request from the ARpoise client app,
it connects to the ARpoise directory back end and queries whether there are any layers within the user's range.

If there are more than one layers in the user's range, it returns the list of available layers to the client.

If there is exactly one layer, it redirects the user's client ARpoise app to the that layer.

If there is no layer, it redirects the user's client ARpoise app to the ARpoise default layer.

## Note
For compiling you also need the pbl
[repository](../pbl/src/).
