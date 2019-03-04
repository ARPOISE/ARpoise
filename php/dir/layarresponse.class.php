<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * Response for Layar
 *
 * @package PorPOISe
 */

/**
 * Response object
 *
 * Contains only overall response parameters, not specific ones
 * such as errorCode, errorMessage, nextPageKey and hasMorePages
 *
 * @package PorPOISe
 */
class LayarResponse {
	/** @var POI[] */
	public $hotspots = array();
	/** @var string[] When sending a POI update (instead of a full refresh), this specifies hotspots to delete from the previous set */
	public $deletedHotspots = array();
	/** @var int Radius containing the returned POI set */
	public $radius = 0;
	/** @var int number of hotspots/pois in layer */
	public $numberOfHotspots = 0;
	/** @var int Bleaching value, 0 - 100 */
	public $bleachingValue = 0;
	/** @var int Refresh interval in seconds */
	public $refreshInterval = 300;
	/** @var int Visibility range in meters */
	public $visibilityRange = 1500;
	/** @var int Area size in meters */
	public $areaSize = 0;
	/** @var int Area width in meters */
	public $areaWidth = 0;
	/** @var int Refresh distance in meters */
	public $refreshDistance = 100;
	/** @var bool Show the menu button, not shown when the default layer is displayed */
	public $showMenuButton = TRUE;
	/** @var bool Do a full refresh or an update */
	public $fullRefresh = TRUE;
	/** @var bool Do apply the Kalman filter */
	public $applyKalmanFilter = TRUE;
	/** @var string Redirect the client to this url */
	public $redirectionUrl = NULL;
	/** @var string Redirect the client to this layer */
	public $redirectionLayer = NULL;
	/** @var string Response message to display */
	public $showMessage = NULL;
	/** @var string Message to if no pois are found */
	public $noPoisMessage = NULL;
	/** @var Action[] */
	public $actions = array();
	/** @var Animation[] */
	public $animations = array("onCreate" => array(), "onFocus" => array(), "onClick" => array());
	/** @var bool */
	public $morePages = FALSE;
	/** @var string */
	public $nextPageKey = NULL;
	/** @var string */
	public $layer = NULL;
	/** @var int */
	public $errorCode = 0;
	/** @var string */
	public $errorString = "ok";
}
