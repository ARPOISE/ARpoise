<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * PorPOISe Dashboard authorization check
 *
 * @package PorPOISe
 * @subpackage Dashboard
 */

/* start session */
session_start();

/* generate session key */
$_sessionKey = md5(__FILE__);

/* check for login attempt */
if (!empty($_REQUEST["username"])) {
	if (DML::validCredentials($_REQUEST["username"], $_REQUEST["password"])) {
		$_SESSION[$_sessionKey]["loggedIn"] = TRUE;
	} else {
		$_SESSION[$_sessionKey]["loggedIn"] = FALSE;
		GUI::printError("Invalid username or password");
	}
}

/* check for logout attempt */
if (!empty($_REQUEST["logout"]) && $_REQUEST["logout"]) {
	$_SESSION[$_sessionKey]["loggedIn"] = FALSE;
}

/* check for logged in status */
if (empty($_SESSION[$_sessionKey]["loggedIn"]) || !$_SESSION[$_sessionKey]["loggedIn"]) {
	/* not logged in */
	GUI::printMessage("%s", GUI::createLoginScreen());
	exit();
}

/* logged in, fall through to rest of site */
