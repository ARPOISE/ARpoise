# ARpoiseDirectory

ARpoiseDirectory is a cgi-bin program written in C. It acts as a cgi filter between the ARpoise client app and the dir.php ARpoise directory backend.

## Functionality
After receiving a request from the ARpoise client app,
it connects to the ARpoise directory PHP backend and queries whether there are any layers within 1500 meters of the user's location.
If so, it redirects the client to the url and layer name of the layer closest to the client.
If no so, it returns the information of the default layer to the client.

## Note
For compiling it, you also need the clone the pbl repository
[repository(../pbl/src/).
