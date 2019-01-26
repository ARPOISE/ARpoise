<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * PorPOISe GUI web service
 *
 * @package PorPOISe
 * @subpackage Dashboard
 */

/**
 * Requires rest of Dashboard
 */
require_once("dashboard.inc.php");

/**
 * Terminates request with a 401 Bad request answer
 *
 * @return void
 */
function badRequest() {
	header("HTTP/1.0 401 Bad request");
	die();
}

if (empty($_REQUEST["action"])) {
	badRequest();
}

switch($_REQUEST["action"]) {
case "newAction":
	if (empty($_REQUEST["index"])) {
		badRequest();
	}
	$index = $_REQUEST["index"];
	if (!is_numeric($index)) {
		badRequest();
	}
  if (empty($_REQUEST["layerAction"])) {
    $layerAction = FALSE;
  } else {
    $layerAction = $_REQUEST["layerAction"];
  }
	printf("%s", GUI::createActionSubtable($index, new POIAction(), $layerAction));
	exit();
case "newAnimation":
	if (empty($_REQUEST["index"])) {
		badRequest();
	}
	$index = $_REQUEST["index"];
	if (!is_numeric($index)) {
		badRequest();
	}
	printf("%s", GUI::createAnimationSubtable($index, "", new Animation()));
	exit();
default:
	badRequest();
}
