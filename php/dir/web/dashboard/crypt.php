<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * Helper script for creating crypt()-hashes of passwords
 *
 * @package PorPOISe
 * @subpackage Dashboard
 */

/**
 * Prints usage information
 *
 * @return void
 */
function usage() {
	printf("%s\n",
<<<OUT
Usage (command line): php crypt.php <arg1> <arg2> ...
Usage (web): crypt.php?user1=<pass1>&user2=<pass2>&...

Outputs all arguments after running them through crypt() with no
second argument. When calling through the web the name of the arguments
does not really matter but you can use the user name for reference.
Every GET (name, value) parameter is processed and printed as PHP code
suitable for cut and pasting into users.inc.php.
OUT
	);
}

if (isset($_SERVER["TERM"])) {
	/* command line mode */
	$args = $_SERVER["argv"];
	unset($args[0]);	/* argv[0] contains the script's name */
} else {
	$args = $_GET;
	header("Content-Type: text/plain");
}

if (count($args) == 0) {
	usage();
	exit(1);
}

foreach ($args as $user => $pass) {
	printf("\$_access[\"%s\"] = '%s';\n", $user, crypt($pass));
}
