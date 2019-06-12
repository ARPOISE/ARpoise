<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 *
 * Acknowledgments:
 * Guillaume Danielou of kew.org for the XSL transformation
 */

/**
 * POI connector from XML files
 *
 * @package PorPOISe
 */

/**
 * POI connector from XML files
 *
 * @package PorPOISe
 */
class XMLPOIConnector extends POIConnector
{

    const EMPTY_DOCUMENT = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<layer><pois/></layer>";

    /** @var string */
    protected $source;

    /** @var string */
    protected $styleSheetPath = "";

    /** @var SimpleXMLElement Loaded XML for the layer; do not reference directly but use getSimpleXMLFromSource() */
    protected $simpleXMLElement;

    /**
     * Constructor
     *
     * The field separator can be configured by modifying the public
     * member $separator.
     *
     * @param string $source
     *            Filename of the POI file
     */
    public function __construct($source)
    {
        $this->source = $source;
    }

    /**
     * Set the path of an XSL style sheet to transform the input XML
     *
     * @param string $styleSheetPath
     * @return void
     */
    public function setStyleSheet($styleSheetPath)
    {
        $this->styleSheetPath = $styleSheetPath;
    }

    protected function getSimpleXMLFromSource()
    {
        if (empty($this->simpleXML)) {
            if (! empty($this->styleSheetPath)) {
                $this->simpleXML = new SimpleXMLElement($this->transformXML(), 0, FALSE);
            } else {
                $this->simpleXML = new SimpleXMLElement($this->source, 0, TRUE);
            }
            if (empty($this->simpleXML)) {
                throw new Exception("Failed to load data");
            }
        }
        return $this->simpleXML;
    }

    /**
     * Return a Layar response
     *
     * @param Filter $filter
     *
     * @return LayarResponse
     */
    public function getLayarResponse(Filter $filter = NULL)
    {
        $libxmlErrorHandlingState = libxml_use_internal_errors(TRUE);

        $simpleXML = $this->getSimpleXMLFromSource();

        $result = new LayarResponse();

        $layerNodes = $simpleXML->xpath("/layer");
        if (count($layerNodes) > 0) { // when 0, this is an old style PorPOISe XML file
            $layerNode = $layerNodes[0]; // always pick first <layer> element, multiples are ignored
            foreach ($layerNode->children() as $childNode) {
                $name = $childNode->getName();
                switch ($name) {
                    case "bleachingValue":
                    case "refreshInterval":
                    case "areaSize":
                    case "areaWidth":
                    case "visibilityRange":
                    case "refreshDistance":
                        $result->$name = (int) $childNode;
                        break;
                    case "showMenuButton":
                    case "fullRefresh":
                    case "applyKalmanFilter":
                    case "isDefaultLayer":
                        $result->$name = (bool) ((string) $childNode);
                        break;
                    case "showMessage":
                    case "redirectionLayer":
                    case "redirectionUrl":
                    case "layerTitle":
                    case "noPoisMessage":
                        $result->$name = (string) $childNode;
                        break;
                    case "action":
                        $result->actions[] = new Action($childNode);
                        break;
                    case "animation":
                        if (in_array((string) $childNode, array(
                            "drop",
                            "spin",
                            "grow"
                        ))) {
                            $result->animations = (string) $childNode;
                        } else {
                            $events = (string) $childNode["events"];
                            if (! empty($events)) {
                                foreach (array(
                                    "onCreate",
                                    "onFollow",
                                    "onFocus",
                                    "inFocus",
                                    "onClick"
                                ) as $event) {
                                    if (strpos($events, $event) !== FALSE) {
                                        $result->animations[$event][] = new Animation($childNode);
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        // not relevant
                        break;
                }
            }
        }

        $result->hotspots = $this->getPOIs($filter);
        $result->numberOfHotspots = count($result->hotspots);

        libxml_use_internal_errors($libxmlErrorHandlingState);

        return $result;
    }

    /**
     * Provides an XPath query for finding POIs in the source file.
     *
     * For relative queries, the context node is the root element.
     * This method can be overridden to use a different query.
     *
     * @param Filter $filter
     *
     * @return string
     */
    public function buildQuery(Filter $filter = NULL)
    {
        return "//pois/poi";
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
    public function getPOIs(Filter $filter = NULL)
    {
        $libxmlErrorHandlingState = libxml_use_internal_errors(TRUE);

        $lat = $filter->lat;
        $lon = $filter->lon;
        $radius = $filter->radius;
        $accuracy = $filter->accuracy;
        // calculate here to prevent recalculation on each foreach loop later
        $dlat = GeoUtil::getLatitudinalDistance(($radius + $accuracy) * 1.25, $lat);
        $dlon = GeoUtil::getLongitudinalDistance(($radius + $accuracy) * 1.25, $lat);

        $simpleXML = $this->getSimpleXMLFromSource();

        $result = array();
        $requestedPOI = NULL;

        $xpathQuery = $this->buildQuery($filter);
        foreach ($simpleXML->xpath($xpathQuery) as $poiData) {
            if (empty($poiData->dimension) || (int) $poiData->dimension == 1) {
                $poi = new POI1D();
            } else if ((int) $poiData->dimension == 2) {
                $poi = new POI2D();
            } else /* if ((int)$poiData->dimension == 3) */
            {
                $poi = new POI3D();
            } // else {
            // throw new Exception("Invalid dimension: " . (string)$poiData->dimension);
            // }
            foreach ($poiData->children() as $child) {
                $nodeName = $child->getName();
                if ($nodeName == "action") {
                    $poi->actions[] = new POIAction($child);
                } else if ($nodeName == "object") {
                    $poi->object = new POIObject($child);
                } else if ($nodeName == "transform") {
                    $poi->transform = new POITransform($child);
                } else if ($nodeName == "animation") {
                    if (in_array((string) $child, array(
                        "drop",
                        "spin",
                        "grow"
                    ))) {
                        $poi->animations = (string) $child;
                    } else {
                        $events = (string) $child["events"];
                        if (! empty($events)) {
                            foreach (array(
                                "onCreate",
                                "onFollow",
                                "onFocus",
                                "inFocus",
                                "onClick"
                            ) as $event) {
                                if (strpos($events, $event) !== FALSE) {
                                    $poi->animations[$event][] = new Animation($child);
                                }
                            }
                        }
                    }
                } else {
                    switch ($nodeName) {
                        case "dimension":
                        case "type":
                        case "alt":
                        case "visibilityRange":
                            $value = (int) $child;
                            break;
                        case "lat":
                        case "lon":
                        case "relativeAlt":
                            $value = (float) $child;
                            break;
                        case "showSmallBiw":
                        case "showBiwOnClick":
                        case "doNotIndex":
                        case "isVisible":
                            $value = (bool) (string) $child;
                            break;
                        default:
                            $value = (string) $child;
                            break;
                    }
                    $poi->$nodeName = $value;
                }
            }
            if (empty($filter)) {
                $result[] = $poi;
            } else {
                if (! empty($filter->requestedPoiId) && $filter->requestedPoiId == $poi->id) {
                    // always return the requested POI at the top of the list to
                    // prevent cutoff by the 50 POI response limit
                    $poi->distance = GeoUtil::getGreatCircleDistance(deg2rad($lat), deg2rad($lon), deg2rad($poi->lat), deg2rad($poi->lon));
                    $requestedPOI = $poi;
                } else if ($this->passesFilter($poi, $filter)) {
                    if (empty($radius)) {
                        $poi->distance = GeoUtil::getGreatCircleDistance(deg2rad($lat), deg2rad($lon), deg2rad($poi->lat), deg2rad($poi->lon));
                        $result[] = $poi;
                    } else {
                        // verify if POI falls in bounding box (with 25% margin)
                        /**
                         *
                         * @todo handle wraparound
                         */
                        if ($poi->lat >= $lat - $dlat && $poi->lat <= $lat + $dlat && $poi->lon >= $lon - $dlon && $poi->lon <= $lon + $dlon) {
                            $poi->distance = GeoUtil::getGreatCircleDistance(deg2rad($lat), deg2rad($lon), deg2rad($poi->lat), deg2rad($poi->lon));
                            // filter passed, see if radius allows for inclusion
                            if ($poi->distance < $radius + $accuracy) {
                                $result[] = $poi;
                            }
                            else if ($poi->visibilityRange > 0 && $poi->visibilityRange >= $poi->distance) {
                                $result[] = $poi;
                            }
                        }
                        else if ($poi->visibilityRange > 0 && $poi->visibilityRange >= $radius + $accuracy) {
                            $poi->distance = GeoUtil::getGreatCircleDistance(deg2rad($lat), deg2rad($lon), deg2rad($poi->lat), deg2rad($poi->lon));
                            
                            if ($poi->visibilityRange >= $poi->distance){
                                $result[] = $poi;
                            }
                        }
                    }
                }
            }
        }

        libxml_use_internal_errors($libxmlErrorHandlingState);

        if (! empty($filter)) {
            // sort if filter is set
            $result = objectSort("distance", $result);
        }
        if (! empty($requestedPOI)) {
            // always make sure that the requested POI is the first to be returned
            array_unshift($result, $requestedPOI);
        }
        return $result;
    }

    /**
     * Store POIs
     *
     * Builds up an XML and writes it to the source file with which this
     * XMLPOIConnector was created. Note that there is no way to do
     * "reverse XSL" so any stylesheet is ignored and native PorPOISe XML
     * is written to the source file. If this file is not writable, this
     * method will return FALSE.
     *
     * @param POI[] $pois
     * @param string $mode
     *            "update" or "replace"
     * @param bool $asString
     *            Return XML as string instead of writing it to file
     * @return mixed FALSE on failure, TRUE or a string on success
     */
    public function storePOIs(array $pois, $mode = "update", $asString = FALSE)
    {
        $libxmlErrorHandlingState = libxml_use_internal_errors(TRUE);

        // keep track of the highest id
        $maxID = 0;

        // initialize result XML
        if ($mode == "update") {
            $simpleXML = $this->getSimpleXMLFromSource();
            // look for highest id in current set
            $idNodes = $simpleXML->xpath("//poi/id");
            foreach ($idNodes as $idNode) {
                $id = (int) $idNode;
                if ($id > $maxID) {
                    $maxID = $id;
                }
            }
        } else if ($mode == "replace") {
            $simpleXML = new SimpleXMLElement(self::EMPTY_DOCUMENT);
            // $maxID stays at 0 for now
        }
        $domXML = dom_import_simplexml($simpleXML);
        $simpleXMLPOIsElements = $simpleXML->xpath("//pois");
        if (count($simpleXMLPOIsElements) == 0) {
            throw new Exception("XML file is corrupt");
        }
        $domXMLPOIsElement = dom_import_simplexml($simpleXMLPOIsElements[0]);
        // look for high id in new set, see if it's higher than $maxID
        foreach ($pois as $poi) {
            if ($poi->id > $maxID) {
                $maxID = $poi->id;
            }
        }

        // add POIs to result
        foreach ($pois as $poi) {
            // see if POI is old or new
            if (empty($poi->id)) {
                // assign new id
                $poi->id = $maxID + 1;
                $maxID = $poi->id;
                $oldSimpleXMLElements = array();
            } else {
                // look for existing POI with this id
                $oldSimpleXMLElements = $simpleXML->xpath("//poi[id=" . $poi->id . "]");
            }
            // build element and convert to DOM
            // $simpleXMLElement = self::arrayToSimpleXMLElement("poi", $poi->toArray());
            $simpleXMLElement = self::poiToSimpleXMLElement($poi);
            $domElement = $domXML->ownerDocument->importNode(dom_import_simplexml($simpleXMLElement), TRUE);
            if (empty($oldSimpleXMLElements)) {
                $domXMLPOIsElement->appendChild($domElement);
            } else {
                $domXMLPOIsElement->replaceChild($domElement, dom_import_simplexml($oldSimpleXMLElements[0]));
            }
        }

        if ($asString) {
            return $simpleXML->asXML();
        } else {
            // write new dataset to file
            return file_put_contents($this->source, $simpleXML->asXML()) > 0;
        }

        libxml_use_internal_errors($libxmlErrorHandlingState);
    }

    /**
     * Delete a POI
     *
     * @param string $poiID
     *            ID of the POI to delete
     *            
     * @return void
     *
     * @throws Exception If the source is invalid or the POI could not be deleted
     */
    public function deletePOI($poiID)
    {
        $libxmlErrorHandlingState = libxml_use_internal_errors(TRUE);

        $dom = new DOMDocument();
        $dom->load($this->source);
        $xpath = new DOMXPath($dom);
        $nodes = $xpath->query(sprintf("//poi[id='%s']", $poiID));
        if ($nodes->length == 0) {
            throw new Exception(sprintf("Could not delete POI: no POI found with ID %s", $poiID));
        }
        $nodesToRemove = array();
        for ($i = 0; $i < $nodes->length; $i ++) {
            $nodesToRemove[] = $nodes->item($i);
        }
        foreach ($nodesToRemove as $node) {
            $node->parentNode->removeChild($node);
        }

        $dom->save($this->source);

        libxml_use_internal_errors($libxmlErrorHandlingState);
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
    public function storeLayerProperties(LayarResponse $response, $asString = FALSE)
    {
        $libxmlErrorHandlingState = libxml_use_internal_errors(TRUE);

        $simpleXML = $this->getSimpleXMLFromSource();
        if ($simpleXML->getName() == "pois") {
            // older version XML file, time to fix that
            $poisSimpleXML = $simpleXML;
            $simpleXML = new SimpleXMLElement(self::EMPTY_DOCUMENT);
            unset($simpleXML->pois);
            $dom = dom_import_simplexml($simpleXML);
            $poisDom = $dom->ownerDocument->importNode(dom_import_simplexml($poisSimpleXML), TRUE);
            $dom->appendChild($poisDom);
        }
        unset($simpleXML->action);
        unset($simpleXML->animation);

        $relevantFields = array(
            "bleachingValue",
            "refreshInterval",
            "areaSize",
            "areaWidth",
            "visibilityRange",
            "refreshDistance",
            "showMenuButton",
            "fullRefresh",
            "applyKalmanFilter",
            "isDefaultLayer",
            "showMessage",
            "redirectionLayer",
            "redirectionUrl",
            "layerTitle",
            "noPoisMessage"
        );
        foreach ($relevantFields as $fieldName) {
            $simpleXML->$fieldName = $response->$fieldName;
        }
        foreach ($response->actions as $action) {
            if (empty($simpleXML->action)) {
                $i = 0;
            } else {
                $i = count($simpleXML->action);
            }
            $actionFields = array(
                "label",
                "uri",
                "method",
                "contentType",
                "activityType",
                "params",
                "showActivity",
                "activityMessage"
            );
            foreach ($actionFields as $actionField) {
                if ($actionField == "params") {
                    $simpleXML->action[$i]->$actionField = implode(",", $action->$actionField);
                } else {
                    $simpleXML->action[$i]->$actionField = $action->$actionField;
                }
            }
        }
        foreach ($response->animations as $event => $animations) {
            foreach ($animations as $animation) {
                $animationElement = $simpleXML->addChild("animation");
                $animationElement["events"] = $event;
                foreach ($animation as $animationName => $animationValue) {
                    if ($animationName == "axis") {
                        $animationValue = $animation->axisString();
                    }
                    $animationElement->addChild($animationName, str_replace("&", "&amp;", $animationValue));
                }
            }
        }

        libxml_use_internal_errors($libxmlErrorHandlingState);

        if ($asString) {
            return $simpleXML->asXML();
        } else {
            return file_put_contents($this->source, $simpleXML->asXML()) > 0;
        }
    }

    /**
     * Convert an array to a SimpleXMLElement
     *
     * Converts $array to a SimpleXMLElement by mapping they array's keys
     * to node names and values to values. Traverses sub-arrays.
     *
     * @param string $rootName
     *            The name of the root element
     * @param array $array
     *            The array to convert
     * @return SimpleXMLElement
     */
    public static function arrayToSimpleXMLElement($rootName, array $array)
    {
        $result = new SimpleXMLElement(sprintf("<%s/>", $rootName));
        self::addArrayToSimpleXMLElement($result, $array);
        return $result;
    }

    /**
     * Recursive helper method for arrayToSimpleXMLElement
     *
     * @param SimpleXMLElement $element
     * @param array $array
     *
     * @return void
     */
    public static function addArrayToSimpleXMLElement(SimpleXMLElement $element, array $array)
    {
        foreach ($array as $key => $value) {
            if (is_array($value)) {
                $child = $element->addChild($key);
                self::addArrayToSimpleXMLElement($child, $value);
            } else {
                $element->addChild($key, $value);
            }
        }
    }

    /**
     * Create a SimpleXMLElement representation of a POI
     *
     * @param POI $poi
     * @return SimpleXMLElement
     */
    public static function poiToSimpleXMLElement(POI $poi)
    {
        $poiElement = new SimpleXMLElement("<" . "?xml version=\"1.0\" encoding=\"UTF-8\"?" . ">\n<poi/>");
        foreach ($poi as $key => $value) {
            if ($key == "actions") {
                foreach ($value as $action) {
                    $actionElement = $poiElement->addChild("action");
                    foreach ($action as $actionName => $actionValue) {
                        if ($actionName == "params") {
                            $actionValue = implode(",", $actionValue);
                        }
                        $actionElement->addChild($actionName, str_replace("&", "&amp;", $actionValue));
                    }
                }
            } else if ($key == "animations") {
                foreach ($value as $event => $animations) {
                    foreach ($animations as $animation) {
                        $animationElement = $poiElement->addChild("animation");
                        $animationElement["events"] = $event;
                        foreach ($animation as $animationName => $animationValue) {
                            if ($animationName == "axis") {
                                $animationValue = $animation->axisString();
                            }
                            $animationElement->addChild($animationName, str_replace("&", "&amp;", $animationValue));
                        }
                    }
                }
            } else if ($key == "transform") {
                $transformElement = $poiElement->addChild("transform");
                foreach (array(
                    "rel",
                    "angle",
                    "scale"
                ) as $elementName) {
                    $transformElement->addChild($elementName, str_replace("&", "&amp;", $poi->transform->$elementName));
                }
            } else if ($key == "object") {
                $objectElement = $poiElement->addChild("object");
                foreach (array(
                    "baseURL",
                    "full",
                    "poiLayerName",
                    "relativeLocation",
                    "icon",
                    "size",
                    "triggerImageURL",
                    "triggerImageWidth"
                ) as $elementName) {
                    $objectElement->addChild($elementName, str_replace("&", "&amp;", $poi->object->$elementName));
                }
            } else {
                $poiElement->addChild($key, str_replace("&", "&amp;", $value));
            }
        }
        return $poiElement;
    }

    /**
     * Transform the input source using the set XSL stylesheet
     *
     * @return string The resulting XML
     */
    public function transformXML()
    {
        $xslProcessor = new XSLTProcessor();
        $xsl = new DOMDocument();
        if ($xsl->load($this->styleSheetPath) == FALSE) {
            throw new Exception("transformXML - Failed to load stylesheet");
        }
        $xslProcessor->importStyleSheet($xsl);
        $xml = new DOMDocument();
        if ($xml->load($this->source) == FALSE) {
            throw new Exception("transformXML - Failed to load xml");
        }
        return $xslProcessor->transformToXml($xml);
    }

    /**
     * Set an option
     *
     * XMLPOIConnector supports one option, "stylesheet"
     *
     * @param string $optionName
     * @param string $optionValue
     *
     * @return void
     */
    public function setOption($optionName, $optionValue)
    {
        switch ($optionName) {
            case "stylesheet":
                $this->setStyleSheet($optionValue);
                break;
            default:
                parent::setOption($optionName, $optionValue);
                break;
        }
    }
}
