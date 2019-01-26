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
 * POI connector interface
 *
 * A POI connector is in charge of getting POIs from a specified source.
 * Each specific POI connector should be able to open (connect to) the
 * specified source and get POI information from it somehow. A POIconnector
 * is also in charge of storing POIs in the same format at the same source.
 *
 * @package PorPOISe
 */
interface iPOIConnector {
	/**
	 * Constructor
	 *
	 * Providing a source is a minimal requirement
	 *
	 * @param Object $source
	 */
	public function __construct($source);

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
	public function getPOIs(Filter $filter = NULL);

	/**
	 * Store POIs
	 *
	 * Store a set of POIs
	 *
	 * @param POI[] $pois POIs to store
	 * @param string $mode "replace" to replace the current set, "update" to update current set (default)
	 * @return void
	 */
	public function storePOIs(array $pois, $mode = "update");

	/**
	 * Delete a POI
	 *
	 * @param string $poiID ID of the POI to delete
	 *
	 * @return void
	 *
	 * @throws Exception If the source is invalid or the POI could not be deleted
	 */
	public function deletePOI($poiID);	
	
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
	public function setOption($optionName, $optionValue);
	
}
