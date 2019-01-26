<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * PorPOISe Dashboard Data Management Layer
 *
 * @package PorPOISe
 * @subpackage Dashboard
 */

/* change working directory to root of PorPOISe */
chdir(dirname(__FILE__) . "/../..");
/** Configuration class */
require_once("porpoise.inc.php");

/**
 * Data Management Layer class
 *
 * @package PorPOISe
 * @subpackage Dashboard
 */
class DML {
	/**
	 * Get configuration
	 *
	 * @return PorPOISeConfig
	 */
	static public function getConfiguration() {
		return new PorPOISeConfig("config.xml");
	}

	/**
	 * Get a list of layer names
	 *
	 * @return string[]
	 */
	public static function getLayers() {
		$config = self::getConfiguration();
		$result = array();
		foreach ($config->layerDefinitions as $layerDefinition) {
			$result[] = $layerDefinition->name;
		}
		return $result;
	}

	/**
	 * Get POIs for a configured layer
	 *
	 * @param string $layerName
	 * @return POI[] FALSE if the layer is unknown
	 */
	static public function getPOIs($layerName) {
		$layerDefinition = self::getLayerDefinition($layerName);
		if ($layerDefinition == NULL) {
			return FALSE;
		}
		$poiConnector = self::getPOIConnectorFromDefinition($layerDefinition);
		return $poiConnector->getPOIs();
	}

	/**
	 * Create an appropriate POIConnector for a layer definition
	 *
	 * @param LayerDefinition $layerDefinition
	 *
	 * @return POIConnector
	 */
	protected static function getPOIConnectorFromDefinition(LayerDefinition $layerDefinition) {
		switch($layerDefinition->getSourceType()) {
		case LayerDefinition::DSN:
			$poiConnector = new $layerDefinition->connector($layerDefinition->source["dsn"], $layerDefinition->source["username"], $layerDefinition->source["password"]);
			break;
		case LayerDefinition::FILE:
			$poiConnector = new $layerDefinition->connector($layerDefinition->source);
			break;
		default:
			throw new Exception(sprintf("Invalid source type: %d", $layerDefinition->getSourceType()));
		}
		foreach ($layerDefinition->connectorOptions as $optionName => $option) {
			$poiConnector->setOption($optionName, $option);
		}
		return $poiConnector;
	}


	/**
	 * Get a single POI
	 *
	 * @param string $layerName The layer
	 * @param string $poiID The POI to look for
	 *
	 * @return POI
	 *
	 * @todo Rewrite as soon as POIConnectors support getPOI
	 */
	static public function getPOI($layerName, $poiID) {
		$pois = self::getPOIs($layerName);
		if (empty($pois)) {
			return NULL;
		}
		foreach ($pois as $poi) {
			if ($poi->id == $poiID) {
				return $poi;
			}
		}
		return NULL;
	}

	/**
	 * Save a POI to the data store
	 *
	 * @param string $layerName
	 * @param POI $poi
	 */
	public static function savePOI($layerName, $poi) {
		self::getPOIConnector($layerName)->storePOIs(array($poi));
	}

	/**
	 * Save a layer's properties
	 *
	 * @param string $layerName
	 * @param LayarResponse $properties
	 *
	 * @return void
	 */
	public static function saveLayerProperties($layerName, LayarResponse $properties) {
		self::getPOIConnector($layerName)->storeLayerProperties($properties);
	}

	/**
	 * Delete a POI from the data store
	 *
	 * @param string $layerName
	 * @param string $poiID
	 */
	public static function deletePOI($layerName, $poiID) {
		self::getPOIConnector($layerName)->deletePOI($poiID);
	}

	/**
	 * Check for validity of UN/PW combination
	 *
	 * @param string $username
	 * @param string $password In unencrypted form
	 *
	 * @return bool TRUE if combination is valid, FALSE otherwise
	 */
	public static function validCredentials($username, $password) {
		if (empty($GLOBALS["_access"][$username])) {
			return FALSE;
		}
		return ($GLOBALS["_access"][$username] === crypt($password, $GLOBALS["_access"][$username]));
	}

	/**
	 * Get a LayerDefinition from the configuration
	 *
	 * @param string $layerName
	 *
	 * @return LayerDefinition or NULL if the layer was not found
	 */
	public static function getLayerDefinition($layerName) {
		$config = self::getConfiguration();
		foreach ($config->layerDefinitions as $layerDefinition) {
			if ($layerDefinition->name == $layerName) {
				return $layerDefinition;
			}
		}
		return NULL;
	}

	/**
	 * Get a layer's properties
	 *
	 * Does NOT return POIs, only the layer's static properties
	 *
	 * @param string $layerName
	 *
	 * @return LayarResponse
	 */
	public static function getLayerProperties($layerName) {
		$connector = self::getPOIConnector($layerName);
		$result = $connector->getLayarResponse();
		$result->hotspots = array();	// this method should not be used to get hotspots. removing them here guarantees no other code relies on it
		return $result;
	}

	/**
	 * Get a POIConnector for a layer
	 *
	 * @param string $layerName
	 *
	 * @return POIConnector
	 *
	 * @throws Exception if the layer does not exist
	 */
	protected static function getPOIConnector($layerName) {
		$layerDefinition = self::getLayerDefinition($layerName);
		if (empty($layerDefinition)) {
			throw new Exception(sprintf("Unknown layer: %s\n", $layerName));
		}
		return self::getPOIConnectorFromDefinition($layerDefinition);
	}

	/**
	 * Migrate data from one layer to another
	 *
	 * @param string $from
	 * @param string $to
	 *
	 * @return void
	 *
	 * @throws Exception if the layer does not exist or data is invalid
	 */
	public static function migrateLayers($from, $to) {
		$fromPOIConnector = self::getPOIConnector($from);
		$toPOIConnector = self::getPOIConnector($to);

		$layerContents = $fromPOIConnector->getLayarResponse();
		$layerContents->layer = $to;
		$toPOIConnector->storePOIs($layerContents->hotspots, "replace");
		$toPOIConnector->storeLayerProperties($layerContents);
	}
}
