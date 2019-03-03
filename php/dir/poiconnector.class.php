<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * File for abstract POI connector
 *
 * @package PorPOISe
 */

/**
 * POI connector base class
 *
 * A POI connector is in charge of getting POIs from a specified source.
 * Each specific POI connector should be able to open (connect to) the
 * specified source and get POI information from it somehow. A POIConnector
 * is also in charge of storing POIs in the same format at the same source.
 *
 * @package PorPOISe
 */
abstract class POIConnector implements iPOIConnector {
	/**
	 * Constructor
	 *
	 * Providing a source is a minimal requirement
	 *
	 * @param string $source
	 */
	// public abstract function __construct($source);

	/**
	 * Return POIs
	 *
	 * This method should only return POIs that pass the criteria
	 * stored in $filter. No filter means all POIs.
	 *
	 * @param Filter $filter
	 *
	 * @return POI[]
	 *
	 * @throws Exception
	 */
	// public abstract function getPOIs(Filter $filter = NULL);

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
		$result->numberOfHotspots = count($result->hotspots);
		return $result;
	}

	/**
	 * Store POIs
	 *
	 * Store a set of POIs
	 *
	 * @param POI[] $pois POIs to store
	 * @param string $mode "replace" to replace the current set, "update" to update current set (default)
	 * @return void
	 */
	public function storePOIs(array $pois, $mode = "update") {
		throw new Exception(__METHOD__ . " has not been implemented");
	}

	/**
	 * Save layer properties
	 *
	 * Note: uses LayarResponse as transport for properties but will not
	 * save the contents of $properties->hotspots. Use storePOIs for that
	 *
	 * @param LayarResponse $properties
	 * @param bool $asString
	 *
	 * @return mixed FALSE on failure, XML string or TRUE on success
	 *
	 * @throws Exception
	 */
	public function storeLayerProperties(LayarResponse $properties) {
		throw new Exception(__METHOD__ . " has not been implemented");
	}

	/**
	 * Delete a POI
	 *
	 * @param string $poiID ID of the POI to delete
	 *
	 * @return void
	 *
	 * @throws Exception If the source is invalid or the POI could not be deleted
	 */
	public function deletePOI($poiID) {
		throw new Exception(__METHOD__ . " has not been implemented");
	}

	/**
	 * Set a (connector-specific) option
	 *
	 * Connectors with specific options, such as an XML stylesheet, should
	 * override this method to handle those options but always call the
	 * parent method for unknown options.
	 *
	 * @param string $optionName
	 * @param string $optionValue
	 *
	 * @return void
	 */
	public function setOption($optionName, $optionValue) {
		/* no generic options defined as of yet */
	}

	/**
	 * Determines whether a POI passes the supplied filter options
	 *
	 * @param POI $poi
	 * @param Filter $filter
	 *
	 * @return bool
	 */
	protected function passesFilter(POI $poi, Filter $filter = NULL) {
	    return $poi->isVisible;
	}
}
