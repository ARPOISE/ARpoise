![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ArpoiseDirectory

The ArpoiseDirectory is run by the administrators of www.arpoise.com, content creators who want to use ARpoise or AR-vos to deliver their 3D content via the apps, do not need to run this service.

ArpoiseDirectory is a cgi-bin program written in C. It acts as a cgi filter between the Layar app and the porpoise.php poi provider.
If certain conditions are met, it duplicates the pois returned from porpoise. 
This allows to dynamically show sometimes more and sometimes less pois in Layar.

For compiling it you also need to clone the repository peterGraf/pbl.
