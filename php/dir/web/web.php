<?php

// FIXME not for production
ini_set('display_errors', 1);

chdir(dirname(__FILE__) . "/..");
require_once("porpoise.inc.php");


/* use most strict warnings, enforces neat and correct coding */
error_reporting(E_ALL | E_STRICT);

	
/* open config file */
try {
	$config = new PorPOISeConfig("config.xml");
} catch (Exception $e) {
	printf("Error loading configuration: %s", $e->getMessage());
}


/* create server */
$server = WebAppServerFactory::createWebAppServer($config);

/* handle the request, and that's the end of it */
$server->handleRequest();

?>
