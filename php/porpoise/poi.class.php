<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * Classes for Point of Interest definition
 *
 * @package PorPOISe
 */

/**
 * Subclasses of this class can all be converted to (associative) arrays
 * (useful for a.o. JSON-ing).
 *
 * @package PorPOISe
 */
abstract class Arrayable {
	/**
	 * Stores the contents of this object into an associative array
	 * with elements named after the members of the object. Members that
	 * contain properties are converted recursively.
	 *
	 * @return array
	 */
	public function toArray() {
		$result = array();
		$reflectionClass = new ReflectionClass($this);
		$reflectionProperties = $reflectionClass->getProperties();
		foreach ($reflectionProperties as $reflectionProperty) {
			$propertyName = $reflectionProperty->getName();
			$result[$propertyName] = $this->$propertyName;
			if (is_object($result[$propertyName])) {
				$result[$propertyName] = $result[$propertyName]->toArray();
			} else if (is_array($result[$propertyName])) {
				$result[$propertyName] = self::arrayToArray($result[$propertyName]);
			}
		}
		return $result;
	}

	/**
	 * Traverse an array recursively to call toArray on each object
	 *
	 * @return array
	 */
	protected function arrayToArray($array) {
		foreach ($array as $key => $value) {
			if (is_array($value)) {
				$array[$key] = self::arrayToArray($value);
			} else if (is_object($value)) {
				$array[$key] = $value->toArray();
			} else {
				$array[$key] = $value;
			}
		}
		return $array;
	}
}

/** Class to store an action. Can be a "whole layer" action or a POI action
 *
 * @package PorPOISe
 */
class Action extends Arrayable {
	/** Default action label. Only for flat files */
	/* LEGACY */
	const DEFAULT_ACTION_LABEL = "Do something funky";

	/** @var string URI that should be invoked by activating this action */
	public $uri = NULL;
	/** @var string Label to show in the interface */
	public $label = NULL;
	/** @var string Content type */
	public $contentType = NULL;
	/** @var string HTTP method */
	public $method = "GET";
	/** @var int Activity type. Possible types are currently undocumented */
	public $activityType = NULL;
	/** @var string[] Which parameters to include in the call */
	public $params = array();
	/** @var bool Close the BIW after the action has finished */
	public $closeBiw = FALSE;
	/** @var bool Show activity indicator while action completes */
	public $showActivity = TRUE;
	/** @var string Message to show instead of default spinner */
	public $activityMessage = NULL;
	
	/**
	 * Constructor
	 *
	 * If $source is a string, it must be a URI and a default label will be
	 * assigned to it, no other properties will be changed.
	 * If $source is an array or an object all relevent properties will
	 * be extracted from it.
	 *
	 * @param mixed $source
	 */
	public function __construct($source = NULL) {
		if (empty($source)) {
			return;
		}
		$optionalFields = array("contentType", "method", "activityType", "params", "closeBiw", "showActivity", "activityMessage");

		if (is_string($source)) {
			$this->label = self::DEFAULT_ACTION_LABEL;
			$this->uri = $source;
		} else if (is_array($source)) {
			$this->label = $source["label"];
			$this->uri = $source["uri"];
			foreach ($optionalFields as $field) {
				if (isset($source[$field])) {
					switch($field) {
					case "activityType":
						$this->$field = (int)$source[$field];
						break;
					case "closeBiw":
					case "showActivity":
						$this->$field = (bool)$source[$field];
						break;
					case "params":
						$value = (string)$source[$field];
						if (!empty($value)) {
							$this->$field = explode(",", $value);
						}
						break;
					default:
						$this->$field = (string)$source[$field];
						break;
					}
				}
			}
		} else {
			$this->label = (string)$source->label;
			$this->uri = (string)$source->uri;
			foreach ($optionalFields as $field) {
				if (isset($source->$field)) {
					switch($field) {
					case "activityType":
						$this->$field = (int)$source->$field;
						break;
					case "closeBiw":
					case "showActivity":
						$this->$field = (bool)(string)$source->$field;
						break;
					case "params":
						$value = (string)$source->$field;
						if (!empty($value)) {
							$this->$field = explode(",", $value);
						}
						break;
					default:
						$this->$field = (string)$source->$field;
						break;
					}
				}
			}
		}
	}
}

/**
 * Class to store a POI action
 *
 * @package PorPOISe
 */
class POIAction extends Action {
	/** @var int Range for action autotrigger */
	public $autoTriggerRange = NULL;
	/** @var bool Only act on autotrigger */
	public $autoTriggerOnly = FALSE;

	/**
	 * Constructor
	 *
	 * If $source is a string, it must be a URI and a default label will be
	 * assigned to it
	 * If $source is an array it is expected to contain elements "label"
	 * and "uri".
	 * If $source is an object, it is expected to have members "label" and
	 * "uri".
	 *
	 * @param mixed $source
	 */
	public function __construct($source = NULL) {
		if (empty($source)) {
			return;
		}

		parent::__construct($source);

		if (is_string($source)) {
			return;
		} else if (is_array($source)) {
			if (!empty($source["autoTriggerRange"])) {
				$this->autoTriggerRange = (int)$source["autoTriggerRange"];
				$this->autoTriggerOnly = (bool)$source["autoTriggerOnly"];
			}
		} else {
			if (!empty($source->autoTriggerRange)) {
				$this->autoTriggerRange = (int)$source->autoTriggerRange;
				$this->autoTriggerOnly = (bool)((string)$source->autoTriggerOnly);
			}
		}
	}
}


/**
 * Holds transformation information for multi-dimensional POIs
 *
 * @package PorPOISe
 */
class POITransform extends Arrayable {
	/** @var boolean Specifies whether the POIs position transformation is relative to
	 * the viewer, i.e. always facing the same direction */
	public $rel = FALSE;
	/** @var float Rotation angle in degrees to rotate the object around the z-axis. */
	public $angle = 0;
	/** @var float Scaling factor */
	public $scale = 1;

	/**
	 * Constructor
	 */
	public function __construct($source = NULL) {
		if (empty($source)) {
			return;
		}

		if (is_array($source)) {
			$this->rel = (bool)$source["rel"];
			$this->angle = (float)$source["angle"];
			$this->scale = (float)$source["scale"];
		} else {
			$this->rel = (bool)((string)$source->rel);	/* SimpleXMLElement objects always get cast to TRUE even when representing an empty element */
			$this->angle = (float)$source->angle;
			$this->scale = (float)$source->scale;
		}
	}
}

/**
 * Class for storing 2D/3D object information
 *
 * @package PorPOISe
 */
class POIObject extends Arrayable {
	/** @var string Base URL to resolve all the other references */
	public $baseURL;
	/** @var string Filename of the full object, in UG used as prefab name */
	public $full;
	/** @var string Name of layar to include in poi */
	public $poiLayerName = NULL;
	/** @var relative location (x,y,z) */
	public $relativeLocation = NULL;
	/** @var string Filename of an icon of the object for viewing from afar */
	public $icon = NULL;
	/** @var float Size of the object in meters, i.e. the length of the smallest cube that can contain the object */
	public $size;
	/** @var string URL of a trigger image for the poi */
	public $triggerImageURL;
	/** @var float width of the trigger image in real world meters */
	public $triggerImageWidth;

	/**
	 * Constructor
	 */
	public function __construct($source = NULL) {
		if (empty($source)) {
			return;
		}

		if (is_array($source)) {
			$this->baseURL = $source["baseURL"];
			$this->triggerImageURL = $source["triggerImageURL"];
			$this->full = $source["full"];
			$this->poiLayerName = $source["poiLayerName"];
			if (!empty($source["relativeLocation"])) {
				$this->relativeLocation = $source["relativeLocation"];
			}
			if (!empty($source["icon"])) {
				$this->icon = $source["icon"];
			}
			$this->size = (float)$source["size"];
			$this->triggerImageWidth = (float)$source["triggerImageWidth"];
		} else {
		    foreach (array("baseURL", "full", "poiLayerName", "relativeLocation", "icon", "size", "triggerImageURL", "triggerImageWidth") as $fieldName) {
				switch ($fieldName) {
				case "baseURL":
				case "full":
				case "poiLayerName":
				case "relativeLocation":
				case "icon":
				case "triggerImageURL":
					if (empty($source->$fieldName)) {
						break;
					}
					$this->$fieldName = (string)$source->$fieldName;
					break;
				case "size":
					$this->size = (float)$source->size;
					break;
				case "triggerImageWidth":
				    $this->triggerImageWidth = (float)$source->triggerImageWidth;
				    break;
				}
			}
		}
	}
}

/**
 * Class for storing an animation definition
 *
 * @package PorPOISe
 */
class Animation extends Arrayable {
    /** @var string name of the animation */
    public $name;
	/** @var string type of animation */
	public $type;
	/** @var float length of the animation in seconds */
	public $length;
	/** @var float delay in seconds before the animation starts */
	public $delay = NULL;
	/** @var string interpolation to apply */
	public $interpolation = NULL;
	/** @var float interpolation parameter */
	public $interpolationParam = NULL;
	/** @var bool persist post-state when animation completes */
	public $persist = FALSE;
	/** @var bool repeat the animation */
	public $repeat = FALSE;
	/** @var float modifier for start state */
	public $from = NULL;
	/** @var float to modifier for end state */
	public $to = NULL;
	/** @var string names of the animations this animation is followed by */
	public $followedBy;
	/** @var vector (x,y,z associative array) axis for the animation */
	public $axis = array("x" => NULL, "y" => NULL, "z" => NULL);

	public function axisString() {
		if ($this->axis["x"] === NULL || $this->axis["y"] === NULL || $this->axis["z"] === NULL) {
			return "";
		}
		return sprintf("%s,%s,%s", $this->axis["x"], $this->axis["y"], $this->axis["z"]);
	}
	
	public function set($key, $value) {
		switch($key) {
		case "axis":
			if (is_array($value)) {
				$this->axis = $value;
			} else {
				$axisValues = explode(",", (string)$value);
				foreach ($axisValues as $k => $v) {
					$axisValues[$k] = (float)$v;
				}
				if (count($axisValues) != 3) {
					$axisValues = array(NULL, NULL, NULL);
				}
				$this->axis = array_combine(array("x","y","z"), $axisValues);
			}
			break;
		case "type":
		case "interpolation":
		case "name":
		case "followedBy":
			$this->$key = (string)$value;
			break;
		case "length":
		case "delay":
			$this->$key = (float)$value;
			break;
		case "interpolationParam":
		case "from":
		case "to":
			$this->$key = (float)$value;
			break;
		case "persist":
		case "repeat":
			$this->$key = (bool)(string)$value;
			break;
		}
	}
	
	public function __construct($source = NULL) {
		if (empty($source)) {
			return;
		}

		if (is_array($source)) {
			foreach ($this as $key => $value) {
				// $value is not relevant here
				$this->set($key, $source[$key]);
			}
		} else {
			foreach ($this as $key => $value) {
				// $value is not relevant here
				$this->set($key, $source->$key);
			}
		}
	}
}

/**
 * Class for storing POI information
 *
 * Subclasses should define a "dimension" property or they will
 * always be interpreted by Layar as 1-dimensional points.
 *
 * @package PorPOISe
 */
abstract class POI extends Arrayable {
	/** @var POIAction[] Possible actions for this POI */
	public $actions = array();
	/** @var Animation[] Animations for this POI */
	public $animations = array("onCreate" => array(), "onFocus" => array(), "inFocus" => array(), "onClick" => array(), "onFollow" => array());
	/** @var string attribution text */
	public $attribution = NULL;
	/** @var int Distance in meters between the user and this POI */
	public $distance = NULL;
	/** @var int Visibility Range in meters of this POI */
	public $visibilityRange = 1500;
	/** @var string Identifier for this POI */
	public $id = NULL;
	/** @var string URL of an image to show for this POI */
	public $imageURL = NULL;
	/** @var int Latitude of this POI in microdegrees */
	public $lat = NULL;
	/** @var int Longitude of this POI in microdegrees */
	public $lon = NULL;
	/** @var string First line of text */
	public $line1 = NULL;
	/** @var string Second line of text */
	public $line2 = NULL;
	/** @var string Third line of text */
	public $line3 = NULL;
	/** @var string Fourth line of text */
	public $line4 = NULL;
	/** @var string Title */
	public $title = NULL;
	/** @var int POI type (for custom icons) */
	public $type = NULL;
	/** @var bool doNotIndex */
	public $doNotIndex = FALSE;
	/** @var bool Show the small BIW on the bottom of the screen */
	public $showSmallBiw = TRUE;
	/** @var show the big BIW when the POI is tapped */
	public $showBiwOnClick = TRUE;
	/** @var bool Is this poi visible in the scene */
	public $isVisible = TRUE;
	
	/**
	 * Constructor
	 *
	 * $source is expected to be an array or an object, with element/member
	 * names corresponding to the member names of POI. This allows both
	 * constructing from an associatiev array as well as copy constructing.
	 *
	 * @param mixed $source
	 */
	public function __construct($source = NULL) {
		if (!empty($source)) {
			$reflectionClass = new ReflectionClass($this);
			$reflectionProperties = $reflectionClass->getProperties();
			foreach ($reflectionProperties as $reflectionProperty) {
				$propertyName = $reflectionProperty->getName();
				if (is_array($source)) {
					if (isset($source[$propertyName])) {
						if ($propertyName == "actions") {
							$value = array();
							foreach ($source["actions"] as $sourceAction) {
								$value[] = new POIAction($sourceAction);
							}
						} else if ($propertyName == "animations") {
						    $value = array("onCreate" => array(), "onFollow" => array(), "inFocus" => array(), "onFocus" => array(), "onClick" => array());
							foreach ($source["animations"] as $event => $animations) {
								foreach ($animations as $animation) {
									$value[$event][] = new Animation($animation);
								}
							}
						} else if ($propertyName == "object") {
							$value = new POIObject($source["object"]);
						} else if ($propertyName == "transform") {
							$value = new POITransform($source["transform"]);
						} else {
							switch ($propertyName) {
							case "dimension":
							case "type":
							case "alt":
							case "visibilityRange":
								$value = (int)$source[$propertyName];
								break;
							case "lat":
							case "lon":
							case "relativeAlt":
								$value = (float)$source[$propertyName];
								break;
							case "showSmallBiw":
							case "showBiwOnClick":
							case "doNotIndex":
							case "isVisible":
								$value = (bool)(string)$source[$propertyName];
								break;
							default:
								$value = (string)$source[$propertyName];
								break;
							}
						}
						$this->$propertyName = $value;
					}
				} else {
					if (isset($source->$propertyName)) {
						if ($propertyName == "actions") {
							$value = array();
							foreach ($source->actions as $sourceAction) {
								$value[] = new POIAction($sourceAction);
							}
						} else if ($propertyName == "animations") {
						    $value = array("onCreate" => array(), "onFollow" => array(), "inFocus" => array(), "onFocus" => array(), "onClick" => array());
							foreach ($source->animations as $event => $animations) {
								foreach ($animations as $animation) {
									$value[$event][] = new Animation($animation);
								}
							}
						} else if ($propertyName == "object") {
							$value = new POIObject($source->object);
						} else if ($propertyName == "transform") {
							$value = new POITransform($source->transform);
						} else {
							switch ($propertyName) {
							case "dimension":
							case "type":
							case "alt":
							case "visibilityRange":
								$value = (int)$source->$propertyName;
								break;
							case "relativeAlt":
							case "lat":
							case "lon":
								$value = (float)$source->$propertyName;
								break;
							case "showSmallBiw":
							case "showBiwOnClick":
							case "doNotIndex":
							case "isVisible":
								$value = (bool)(string)$source->$propertyName;
								break;
							default:
								$value = (string)$source->$propertyName;
								break;
							}
						}
						$this->$propertyName = $value;
					}
				}
			}
		}
	}
}

/**
 * Class for storing 1-dimensional POIs
 *
 * @package PorPOISe
 */
class POI1D extends POI {
	/** @var int Number of dimensions for this POI */
	public $dimension = 1;
}

/**
 * Abstract superclass for storing multidimensional POIs
 *
 * @package PorPOISe
 */
abstract class MultidimensionalPOI extends POI {
	/** @var int Altitude of this object in meters. */
	public $alt;
	/** @var POITransform Transformation specification */
	public $transform;
	/** @var POIObject Object specification */
	public $object;
	/** @var float Altitude difference with respect to user's altitude */
	public $relativeAlt;

	/**
	 * Extra constructor
	 */
	public function __construct($source = NULL) {
		parent::__construct($source);
		if (empty($this->transform)) {
			$this->transform = new POITransform();
		}
		if (empty($this->object)) {
			$this->object = new POIObject();
		}
	}
}

/**
 * Class for storing 2D POI information
 *
 * @package PorPOISe
 */
class POI2D extends MultidimensionalPOI {
	/** @var int Number of dimensions for this POI */
	public $dimension = 2;
}

/**
 * Class for storing 3D POI information
 *
 * @package PorPOISe
 */
class POI3D extends MultidimensionalPOI {
	/** @var int Number of dimensions for this POI */
	public $dimension = 3;
}
