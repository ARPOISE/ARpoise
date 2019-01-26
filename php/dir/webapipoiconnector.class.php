<?php

/*
 * PorPOISe
 * Copyright 2010 Squio
 * Released under a permissive license (see LICENSE)
 *
 */

/**
 * POI connector from Web API interface (e.g. REST API)
 * Extends WebApp class to use oAuth aware connections
 *
 * @package PorPOISe
 */

/**
 * POI connector from http API services
 *
 * @package PorPOISe
 */
abstract class WebApiPOIConnector extends WebApp implements iPOIConnector {

	// subclass should implement method getPOIs()

	/**
	 * Override construtor for parent class WebApp
	 * to be compatible with interface iPOIConnector
	 */
	public function __construct($source) {
		// dummy
	}

	/**
	 * Initialize WebApp with Layerdefinition object
	 * 
	 * @param Laerdefinition $definition
	 *
	 * Initializes HTTP (optionally configured for oAuth requests)
	 */
	public function initDefinition($definition) {
		parent::__construct($definition);
	}

	/**
	 * INitialize user- and OAuth objects
	 * to be called just in time for any API request
	 * 
	 * @see getPOIs()
	 */
	public function init() {
		// try initialization of oAuth user token
		try {
			$this->http = $this->httpInit($this->definition->oauth);		
			$this->session_start();
			$this->userInit($this->definition);
			$this->initToken();
		} catch (Exception $e) {
			// fail silently
		}
	}

	/**
	 * Get a Layar response
	 *
	 * @param Filter $filter
	 *
	 * @return LayarResponse
	 *
	 * @throws Exception
	 */
	public function getLayarResponse(Filter $filter = NULL) {
		$result = new LayarResponse();
		$result->hotspots = $this->getPOIs($filter);
		return $result;
	}


	public function storePOIs(array $pois, $mode = "update") {
		throw new Exception(__METHOD__ . " not implemented.");
	}

	public function deletePOI($poiID) {
		throw new Exception(__METHOD__ . " not implemented.");
	}

	public function setOption($name, $value) {
		throw new Exception(__METHOD__ . " not implemented.");
	}


}
