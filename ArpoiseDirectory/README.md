# ArpoiseDirectory

ArpoiseDirectory is a cgi-bin program written in C. It acts as a cgi filter between the Arpoise app and the dir.php arpoise directory backend.

After receiving a request from the Arpoise app, it connects to the arpoise directory backend and queries whether there is content at the user's location. If so it contacts the layer's porpoise.php point of interest provider and returns the pois found.

For compiling it you also need to clone the repository peterGraf/pbl.
