<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * POI connector from "flat" files
 *
 * @package PorPOISe
 */

/**
 * POI connector from "flat" files
 *
 * @deprecated Since 1.0, flat files do not support all Layar
 * functionality
 *
 * @package PorPOISe
 */
class FlatPOIConnector extends POIConnector {
	/** @var string */
	protected $source;
	/** @var string */
	public $separator = "\t";

	/**
	 * Constructor
	 *
	 * The field separator can be configured by modifying the public
	 * member $separator.
	 *
	 * @param string $source Filename of the POI file
	 */
	public function __construct($source) {
		$this->source = $source;
	}

	/**
	 * Get POIs
	 *
	 * @param Filter $filter
	 *
	 * @return POI[]
	 *
	 * @throws Exception
	 */
	public function getPOIs(Filter $filter = NULL) {
		$file = @file($this->source);

		if (empty($file)) {
			throw new Exception("File not readable or empty");
		}

		if (!empty($filter)) {
			$lat = $filter->lat;
			$lon = $filter->lon;
			$radius = $filter->radius;
			$accuracy = $filter->accuracy;
		}

		$result = array();
		$requestedPOI = NULL;

		$headers = explode($this->separator, trim($file[0], "\r\n"));
		for ($i = 1; $i < count($file); $i++) {
			$line = trim($file[$i], "\r\n");
			if (empty($line)) {
				continue;
			}
			$fields = explode($this->separator, $line);

			$row = array_combine($headers, $fields);
			if (empty($row["dimension"]) || $row["dimension"] == 1) {
				$poi = new POI1D();
			} else if ($row["dimension"] == 2) {
				$poi = new POI2D();
			} else if ($row["dimension"] == 3) {
				$poi = new POI3D();
			} else {
				throw new Exception("Invalid dimension: " . $row["dimension"]);
			}
			foreach ($poi as $key => $value) {
				if (!isset($row[$key])) {
					// check for non-required values
					if (in_array($key, array("dimension", "distance", "alt", "relativeAlt", "actions", "imageURL", "doNotIndex", "showSmallBiw", "showBiwOnClick", "isVisible"))) {
						// start next iteration
						continue;
					}
				}
				if ($key == "object") {
					if ($poi->dimension > 1) {
						$value = new POIObject($row);
					}
				} else if ($key == "transform") {
					if ($poi->dimension > 1) {
						$value = new POITransform($row);
					}
				} else if ($key == "actions") {
					$value = array(new POIAction($row[$key]));
				} else {
					switch ($key) {
					case "dimension":
					case "type":
					case "alt":
						$value = (int)$row[$key];
						break;
					case "relativeAlt":
					case "lat":
					case "lon":
						$value = (float)$row[$key];
						break;
					case "showBiwOnClick":
					case "showSmallBiw":
					case "doNotIndex":
					case "isVisible":
						$value = (bool)(string)$row[$key];
						break;
					default:
						$value = (string)$row[$key];
						break;
					}
				}
				$poi->$key = $value;
			}
			if (empty($filter)) {
				$result[] = $poi;
			/*
			 * by @jfdsmit to @jlapoutre on Dec 12 2010 this bypasses distance calculation
			 * inclusion in the result set is handled below this if-block
			 *
			} else if (!empty($filter->requestedPoiId) && $filter->requestedPoiId == $poi["id"]) {
				// always return the requested POI at the top of the list to
				// prevent cutoff by the 50 POI response limit
				array_unshift($result, $poi);
			 */
			} else {
				$poi->distance = GeoUtil::getGreatCircleDistance(deg2rad($lat), deg2rad($lon), deg2rad($poi->lat), deg2rad($poi->lon));
				if (!empty($filter->requestedPoiId) && $filter->requestedPoiId == $poi["id"]) {
					// always return the requested POI at the top of the list to
					// prevent cutoff by the 50 POI response limit
					$requestedPOI = $poi;
				} else if ((empty($radius) || $poi->distance < $radius + $accuracy) && $this->passesFilter($poi, $filter)) {
					$result[] = $poi;
				}
			}
		}

		if (!empty($filter)) {
			// sort if filter is set
			$result = objectSort("distance", $result);
		}
		if (!empty($requestedPOI)) {
			// always make sure that the requested POI is the first to be returned
			array_unshift($result, $requestedPOI);
		}
		return $result;
	}

	/**
	 * Store POIs
	 *
	 * @param POI[] $pois
	 * @param bool $asString return file as string instead of writing it out
	 * @return mixed FALSE on failure, TRUE or a string on success
	 */
	public function storePOIs(array $pois, $mode = "update", $asString = FALSE) {
		$fields = array("actions", "alt", "attribution", "dimension", "id", "imageURL", "lat", "lon", "line2", "line3", "line4", "object", "relativeAlt", "title", "transform", "type", "doNotIndex", "showSmallBiw", "showBiwOnClick", "isVisible");
		$actionFields = array("label", "uri", "autoTriggerRange", "autoTriggerOnly");
		$objectFields = array("baseURL", "full", "poiLayerName", "reduced", "icon", "size");
		$transformFields = array("angle", "rel", "scale");

		// keep track of maximum id in the set
		$maxID = 0;
		
		// if mode == "update" we need to combine these pois with the
		// existing ones
		if ($mode == "update") {
			// get old POIs
			$oldPOIs = $this->getPOIs(0,0,0,0, array());
			
			// build an index of the new POIs
			// while we're looping, work on our max ID
			$poisByID = array();
			foreach ($pois as $poi) {
				$poisByID[$poi->id] = $poi;
				if (!empty($poi->id) && $poi->id > $maxID) {
					$maxID = $poi->id;
				}
			}

			// add all old POIs that are not in the new set so they get preserved
			// while we're looping, work on our max ID
			foreach ($oldPOIs as $oldPOI) {
				if (!isset($poisByID[$oldPOI->id])) {
					$pois[] = $oldPOI;
					if ($oldPOI->id > $maxID) {
						$maxID = $oldPOI->id;
					}
				}
			}
		}
		
		// flow of control: build up a table (2-dimensional array) with header
		// row, transform that to a tab-separated string and output it
		
		// initialize variables
		$table = array();
		$table[] = array();
		$i = count($table) - 1;
		
		// build header
		foreach ($fields as $field) {
			if ($field == "object") {
				foreach ($objectFields as $objectField) {
					$table[$i][] = $objectField;
				}
			} else if ($field == "transform") {
				foreach ($transformFields as $transformField) {
					$table[$i][] = $transformField;
				}
			} else {
				$table[$i][] = $field;
			}
		}

		// build POI table
		foreach ($pois as $poi) {
			// assign id if necessary
			if (empty($poi->id)) {
				$poi->id = $maxID + 1;
				$maxID++;
			}
						
			$table[] = array();
			$i = count($table) - 1;

			foreach ($fields as $field) {
				if ($field == "object" || $field == "transform") {
					if ($field == "object") {
						$description = $objectFields;
					} else {
						$description = $transformFields;
					}
					foreach ($description as $subfield) {
						if (empty($poi->$field)) {
							$table[$i][] = "";
						} else {
							$table[$i][] = $poi->$field->$subfield;
						}
					}
				} else if ($field == "actions") {
					if (empty($poi->actions)) {
						$table[$i][] = "";
					} else {
						// only one action is supported per POI in tab-delimited format
						$table[$i][] = $poi->actions[0]->uri;
					}
				} else {
					$table[$i][] = $poi->$field;
				}
			}
		}

		// transform to string
		$result = "";
		foreach ($table as $row) {
			$result .= implode($this->separator, $row);
			$result .= "\n";
		}

		// and deliver the result
		if ($asString) {
			return $result;
		} else {
			$result = file_put_contents($this->source, $result);
			if ($result) {
				return TRUE;
			} else {
				throw new Exception("Failed to save POIs to file");
			}
		}
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
		$pois = $this->getPOIs(0,0,0,0,array());
		foreach ($pois as $key => $poi) {
			if ($poi->id == $poiID) {
				unset($pois[$key]);
				$this->storePOIs($pois, "replace");
				return;
			}
		}
		throw new Exception(sprintf("Could not delete POI: no POI found with ID %s", $poiID));
	}
}
