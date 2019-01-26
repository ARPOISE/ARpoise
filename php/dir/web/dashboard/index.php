<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * PorPOISe Dashboard
 *
 * @package PorPOISe
 * @subpackage Dashboard
 */

/** Dashboard includes */
require_once("dashboard.inc.php");

/** authorization */
require_once("authorize.inc.php");

// use output buffering so we can prevent output of we want to
ob_start(array("GUI", "finalize"));

/* basic request validation */
if (empty($_REQUEST["action"])) {
	$_action = "main";
} else {
	$_action = $_REQUEST["action"];
}

try {
	/* handle POST (if any) */
	if ($_SERVER["REQUEST_METHOD"] == "POST") {
		GUI::handlePOST();
	}
	/* handle action */
	switch($_action) {
	case "main":
		GUI::printMessage("%s", GUI::createMainScreen());
		break;
	case "layer":
		GUI::printMessage("%s", GUI::createLayerScreen($_REQUEST["layerName"]));
		break;
	case "poi":
		$poi = DML::getPOI($_REQUEST["layerName"], $_REQUEST["poiID"]);
		if (empty($poi)) {
			throw new Exception(sprintf("POI not found: %s:%s", $_REQUEST["layerName"], $_REQUEST["poiID"]));
		}
		GUI::printMessage("%s", GUI::createPOIScreen($_REQUEST["layerName"], $poi, $_REQUEST["showLayer"], $_SERVER['PHP_SELF']));
		break;
	case "newPOI":
		GUI::printMessage("%s", GUI::createNewPOIScreen($_REQUEST["layerName"]));
		break;
	case "migrate":
		GUI::printMessage("%s", GUI::createMigrationScreen());
		break;
	default:
		throw new Exception(sprintf("Invalid action: %s", $_action));
	}
} catch (Exception $e) {
	GUI::printError("%s", $e->getMessage());
	GUI::printMessage("%s", GUI::createMainScreen());
}	
exit();

$pois = DML::getPOIs("example");
printf("<table>\n");
foreach ($pois as $poi) {
	printf("<tr><td>%s</td><td>%s,%s</td></tr>\n", $poi->title, $poi->lat, $poi->lon);
}
printf("</table>");
