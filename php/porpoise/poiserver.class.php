<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 *
 * Acknowledgments:
 * Robert Harm for the "Increase range" error message
 */

/**
 * POI Server for Layar
 *
 * The server consists of a server class whose objects serve Layar responses when
 * properly configured and a factory class that helps you create a properly
 * configured server.
 *
 * @package PorPOISe
 */

/**
 * requires most of PorPOISe base files
 */
require_once ("porpoise.inc.php");

/**
 * Server class that serves up POIs for Layar
 *
 * @package PorPOISe
 */
class LayarPOIServer
{

    /**
     * Default error code
     */
    const ERROR_CODE_DEFAULT = 20;

    /**
     * Request has no POIs in result
     */
    const ERROR_CODE_NO_POIS = 21;

    /** @var string[] Error messages stored in an array because class constants cannot be arrays */
    protected static $ERROR_MESSAGES = array(
        self::ERROR_CODE_DEFAULT => "An error occurred",
        self::ERROR_CODE_NO_POIS => "No POIs found. Increase range or adjust filters to see POIs"
    );

    // layers in this server
    protected $layers = array();

    protected $requiredRequestFields = array(
        "userId",
        "layerName",
        "lat",
        "lon"
    );

    protected $optionalRequestFields = array(
        "accuracy",
        "timestamp",
        "RADIOLIST",
        "SEARCHBOX_1",
        "SEARCHBOX_2",
        "SEARCHBOX_3",
        "CUSTOM_SLIDER_1",
        "CUSTOM_SLIDER_2",
        "CUSTOM_SLIDER_3",
        "pageKey",
        "oath_consumer_key",
        "oauth_signature_method",
        "oauth_timestamp",
        "oauth_nonce",
        "oauth_version",
        "oauth_signature",
        "radius",
        "alt"
    );

    protected $optionalPOIFieldsDefaults = array(
        "alt" => NULL,
        "relativeAlt" => NULL,
        "timestamp" => NULL,
        "doNotIndex" => FALSE,
        "showSmallBiw" => TRUE,
        "showBiwOnClick" => TRUE,
        "isVisible" => TRUE,
        "visibilityRange" => 1500
    );

    protected $optionalResponseFieldsDefaults = array(
        "bleachingValue" => 0,
        "refreshInterval" => 300,
        "visibilityRange" => 1500,
        "areaSize" => 0,
        "areaWidth" => 0,
        "refreshDistance" => 100,
        "fullRefresh" => TRUE,
        "applyKalmanFilter" => TRUE,
        "isDefaultLayer" => FALSE,
        "actions" => array(),
        "showMessage" => NULL,
        "redirectionLayer" => NULL,
        "redirectionUrl" => NULL,
        "noPoisMessage" => NULL,
        "layerTitle" => NULL,
        "deletedHotspots" => array()
    );

    protected $optionalActionFieldsDefaults = array(
        "autoTriggerRange" => NULL,
        "autoTriggerOnly" => NULL,
        "contentType" => NULL,
        "method" => "GET",
        "activityType" => NULL,
        "params" => array(),
        "closeBiw" => FALSE,
        "showActivity" => TRUE,
        "activityMessage" => NULL
    );

    protected $optionalAnimationFieldsDefaults = array(
        "delay" => NULL,
        "interpolation" => NULL,
        "name" => NULL,
        "followedBy" => NULL,
        "interpolationParam" => NULL,
        "persist" => FALSE,
        "repeat" => FALSE,
        "from" => NULL,
        "to" => NULL,
        "axis" => array(
            "x" => NULL,
            "y" => NULL,
            "z" => NULL
        )
    );

    /**
     * Add a layer to the server
     *
     * @param Layer $layer
     *
     * @return void
     */
    public function addLayer(Layer $layer)
    {
        $this->layers[$layer->layerName] = $layer;
    }

    /**
     * Handle a request
     *
     * Request variables are expected to live in the $_REQUEST superglobal
     *
     * @return void
     */
    public function handleRequest(LayarLogger $loghandler = null)
    {
        $filter = $this->buildFilter();
        try {
            $this->validateRequest();

            $layer = $this->layers[$filter->layerName];
            $layer->determineNearbyPOIs($filter);

            $response = $layer->getLayarResponse();
            $response->layer = $filter->layerName;
            $numPois = count($response->hotspots);
            if ($loghandler) {
                $loghandler->log($filter, array(
                    'numpois' => $numPois
                ));
            }

            $this->sendLayarResponse($response);
        } catch (Exception $e) {
            if ($loghandler) {
                $loghandler->log($filter, array(
                    'errorMessage' => $e->getMessage()
                ));
            }

            $this->sendErrorResponse(self::ERROR_CODE_DEFAULT, $e->getMessage());
        }
    }

    /**
     * Send a Layar response to a client
     *
     * @param array $pois
     *            An array of POIs that match the client's request
     * @param bool $morePages
     *            Pass TRUE if there are more pages beyond this set of POIs
     * @param string $nextPageKey
     *            Pass a valid key if $morePages is TRUE
     *            
     * @return void
     */
    protected function sendLayarResponse(LayarResponse $response)
    {
        if (count($response->hotspots) == 0) {
            $response->errorCode = self::ERROR_CODE_NO_POIS;
            $response->errorString = self::$ERROR_MESSAGES[$response->errorCode];
        }
        $aResponse = array();
        foreach ($response as $name => $value) {
            switch ($name) {
                case "layer":
                case "nextPageKey":
                case "errorString":
                    $aResponse[$name] = (string) $value;
                    break;
                case "morePages":
                    $aResponse[$name] = (bool) $response->$name;
                    break;
                case "errorCode":
                    $aResponse[$name] = (int) $response->$name;
                    break;
                case "radius":
                    $aResponse[$name] = intval(1.25 * $response->$name); // extend radius with 25% to avoid far away POI's dropping off when location changes
                    break;
                case "hotspots":
                    foreach ($value as $poi) {
                        $aPoi = $poi->toArray();
                        // strip out optional fields to cut on bandwidth
                        foreach ($this->optionalPOIFieldsDefaults as $field => $defaultValue) {
                            // strip param from reponse if equal to default
                            //
                            // A note on the @ operator here:
                            // there is a slight difference in PHP between an undefined variable
                            // and one that has been defined and set to NULL. There is NO clean way
                            // right now to distinguish between the two as isset() returns FALSE
                            // on both cases, empty() returns TRUE on both cases and no other
                            // function will take an undefined variable without raising a warning
                            if (@$aPoi[$field] == $defaultValue) {
                                unset($aPoi[$field]);
                            }
                        }
                        foreach ($aPoi["actions"] as &$action) {
                            foreach ($this->optionalActionFieldsDefaults as $field => $defaultValue) {
                                if (@$action[$field] == $defaultValue) {
                                    unset($action[$field]);
                                }
                            }
                        }
                        foreach ($aPoi["animations"] as $event => &$animations) {
                            foreach ($animations as $k => &$animation) {
                                foreach ($this->optionalAnimationFieldsDefaults as $field => $defaultValue) {
                                    if (@$animation[$field] == $defaultValue) {
                                        unset($animation[$field]);
                                    }
                                }
                                if (! count($animations[$k])) {
                                    unset($animations[$k]);
                                }
                            }
                            if (! count($aPoi["animations"][$event])) {
                                unset($aPoi["animations"][$event]);
                            }
                        }
                        if (! count($aPoi["animations"])) {
                            unset($aPoi["animations"]);
                        }
                        // upscale coordinate values and truncate to int because of inconsistencies in Layar API
                        // (requests use floats, responses use integers?)
                        $aPoi["lat"] = (int) ($aPoi["lat"] * 1000000);
                        $aPoi["lon"] = (int) ($aPoi["lon"] * 1000000);
                        // fix some types that are not strings
                        $aPoi["type"] = (int) $aPoi["type"];
                        $aPoi["distance"] = (float) $aPoi["distance"];

                        $aResponse["hotspots"][] = $aPoi;
                    }
                    break;
                default:
                    $aResponse[$name] = $value;
                    break;
            }
        }

        // strip out optional global parameters
        foreach ($aResponse["actions"] as &$action) {
            foreach ($this->optionalActionFieldsDefaults as $field => $defaultValue) {
                if (@$action->field == $defaultValue) {
                    unset($action->field);
                }
            }
        }
        foreach ($aResponse["animations"] as $event => &$animations) {
            foreach ($animations as $k => &$animation) {
                foreach ($this->optionalAnimationFieldsDefaults as $field => $defaultValue) {
                    if (@$animation->{$field} == $defaultValue) {
                        unset($animation->{$field});
                    }
                }
                if (! count($animations[$k])) {
                    unset($animations[$k]);
                }
            }
            if (! count($aResponse["animations"][$event])) {
                unset($aResponse["animations"][$event]);
            }
        }
        if (! count($aResponse["animations"])) {
            unset($aResponse["animations"]);
        }
        foreach ($this->optionalResponseFieldsDefaults as $field => $defaultValue) {
            if (@$aResponse[$field] == $defaultValue) {
                unset($aResponse[$field]);
            }
        }

        /* Set the proper content type */
        header("Content-Type: application/json;charset=utf-8");

        printf("%s", json_encode($aResponse));
    }

    /**
     * Send an error response
     *
     * @param int $code
     *            Error code for this error
     * @param string $msg
     *            A message detailing what went wrong
     *            
     * @return void
     */
    protected function sendErrorResponse($code = self::ERROR_CODE_DEFAULT, $msg = NULL)
    {
        $response = array();
        if (isset($_REQUEST["layerName"])) {
            $response["layer"] = $_REQUEST["layerName"];
        } else {
            $response["layer"] = "unspecified";
        }
        $response["errorCode"] = $code;
        if (! empty($msg)) {
            $response["errorString"] = $msg;
        } else {
            $response["errorString"] = self::$ERROR_MESSAGES[$code];
        }
        $response["hotspots"] = array();
        $response["nextPageKey"] = NULL;
        $response["morePages"] = FALSE;

        /* Set the proper content type */
        header("Content-Type: application/json;encoding=utf-8");

        printf("%s", json_encode($response));
    }

    /**
     * Validate a client request
     *
     * If this function returns (i.e. does not throw anything) the request is
     * valid and can be processed with no further input checking
     *
     * @throws Exception Throws an exception of something is wrong with the request
     * @return void
     */
    protected function validateRequest()
    {
        foreach ($this->requiredRequestFields as $requiredRequestField) {
            if (empty($_REQUEST[$requiredRequestField])) {
                throw new Exception(sprintf("Missing parameter: %s", $requiredRequestField));
            }
        }
        foreach ($this->optionalRequestFields as $optionalRequestField) {
            if (! isset($_REQUEST[$optionalRequestField])) {
                $_REQUEST[$optionalRequestField] = "";
            }
        }

        $layerName = $_REQUEST["layerName"];
        if (empty($this->layers[$layerName])) {
            throw new Exception(sprintf("Unknown layer in request: %s", $layerName));
        }

        if ($_REQUEST["lat"] < - 90 || $_REQUEST["lat"] > 90) {
            throw new Exception(sprintf("Invalid latitude in request: %s", $_REQUEST["lat"]));
        }

        if ($_REQUEST["lon"] < - 180 || $_REQUEST["lon"] > 180) {
            throw new Exception(sprintf("Invalid longitude in request: %s", $_REQUEST["lon"]));
        }
    }

    /**
     * Build a filter object from the request
     *
     * @return LayarFilter
     */
    protected function buildFilter()
    {
        $result = new LayarFilter();
        foreach ($_REQUEST as $key => $value) {
            switch ($key) {
                case "userId":
                    $result->userID = $value;
                    break;
                case "pageKey":
                case "lang":
                case "countryCode":
                case "layerName":
                case "version":
                case "action":
                    $result->$key = $value;
                    break;
                case "requestedPoiId":
                    $result->$key = ($value == 'None') ? null : $value;
                    break;
                case "timestamp":
                case "accuracy":
                case "radius":
                case "alt":
                    $result->$key = (int) $value;
                    break;
                case "lat":
                case "lon":
                    $result->$key = (float) $value;
                    break;
                case "RADIOLIST":
                    $result->radiolist = $value;
                    break;
                case "SEARCHBOX":
                    $result->searchbox1 = $value;
                    break;
                case "SEARCHBOX_1":
				/* special case: if SEARCHBOX and SEARCHBOX_1 are set, SEARCHBOX takes precedence */
				if (empty($_REQUEST["SEARCHBOX"])) {
                        $result->searchbox1 = $value;
                    }
                    break;
                case "SEARCHBOX_2":
                    $result->searchbox2 = $value;
                    break;
                case "SEARCHBOX_3":
                    $result->searchbox3 = $value;
                    break;
                case "CUSTOM_SLIDER":
                    $result->customSlider1 = (float) $value;
                    break;
                case "CUSTOM_SLIDER_1":
				/* special case: if CUSTOM_SLIDER and CUSTOM_SLIDER_1 are set, CUSTOM_SLIDER takes precedence */
				if (empty($_REQUEST["CUSTOM_SLIDER"])) {
                        $result->customSlider1 = (float) $value;
                    }
                    break;
                case "CUSTOM_SLIDER_2":
                    $result->customSlider2 = (float) $value;
                    break;
                case "CUSTOM_SLIDER_3":
                    $result->customSlider3 = (float) $value;
                    break;
                case "CHECKBOXLIST":
                    $result->checkboxlist = explode(",", $value);
                    break;
            }
        }
        // As of 20100601 Format is: Layar/x.y [OS name]/x.y.z ([Brand] [Model])
        if (isset($_SERVER['HTTP_USER_AGENT'])) {
            $result->userAgent = $_SERVER['HTTP_USER_AGENT'];
        }

        if (! empty($_REQUEST["layerName"]) && ! empty($_COOKIE[$_REQUEST["layerName"] . "Id"])) {
            $result->porpoiseUID = $_COOKIE[$_REQUEST["layerName"] . "Id"];
        }

        $this->filter = $result;
        return $result;
    }
}

/**
 * Factory class to create LayarPOIServers
 *
 * @package PorPOISe
 */
class LayarPOIServerFactory
{

    /** @var Developer ID */
    protected $developerId;

    /** @var Developer key */
    protected $developerKey;

    /**
     * Constructor
     *
     * @param string $developerID
     *            Your developer ID
     * @param string $developerKey
     *            Your developer key
     */
    public function __construct($developerID, $developerKey)
    {
        $this->developerId = $developerID;
        $this->developerKey = $developerKey;
    }

    /**
     * Create a LayarPOIServer with content from a list of files
     *
     * @deprecated Use the more generic createLayarPOIServer
     *            
     * @param array $layerFiles
     *            The key of each element is expected to be the
     *            layer's name, the value to be the filename of the file containing the
     *            layer's POI in tab delimited format.
     *            
     * @return LayarPOIServer
     */
    public function createLayarPOIServerFromFlatFiles(array $layerFiles)
    {
        $result = new LayarPOIServer();
        foreach ($layerFiles as $layerName => $layerFile) {
            $layer = new Layer($layerName, $this->developerId, $this->developerKey);
            $poiConnector = new FlatPOIConnector($layerFile);
            $layer->setPOIConnector($poiConnector);
            $result->addLayer($layer);
        }
        return $result;
    }

    /**
     * Create a LayarPOIServer with content from a list of XML files
     *
     * @deprecated Use the more generic createLayarPOIServer
     *            
     * @param array $layerFiles
     *            The key of each element is expected to be the
     *            layer's name, the value to be the filename of the file containing the
     *            layer's POIs in XML format.
     *            
     * @param string[] $layerFiles
     * @param string $layerXSL
     *
     * @return LayarPOIServer
     */
    public function createLayarPOIServerFromXMLFiles(array $layerFiles, $layerXSL = "")
    {
        $result = new LayarPOIServer();
        foreach ($layerFiles as $layerName => $layerFile) {
            $layer = new Layer($layerName, $this->developerId, $this->developerKey);
            $poiConnector = new XMLPOIConnector($layerFile);
            $poiConnector->setStyleSheet($layerXSL);
            $layer->setPOIConnector($poiConnector);
            $result->addLayer($layer);
        }
        return $result;
    }

    /**
     * Create a server based on SimpleXML configuration directives
     *
     * @deprecated Use the more generic createLayarPOIServer
     *            
     *             $config is an array of SimpleXMLElements, each element should contain
     *             layer nodes specifying connector (class name), layer name and data source.
     *             The root node name is not important but "layers" is suggested.
     *             For XML, use a URI as source.
     *            
     * @param SimpleXMLElement $config
     *
     * @return LayarPOIServer
     */
    public function createLayarPOIServerFromSimpleXMLConfig(SimpleXMLElement $config)
    {
        $result = new LayarPOIServer();
        foreach ($config->xpath("layer") as $child) {
            $layer = new Layer((string) $child->name, $this->developerId, $this->developerKey);
            $connectorName = (string) $child->connector;
            $poiConnector = new $connectorName((string) $child->source);
            $layer->setPOIConnector($poiConnector);
            $result->addLayer($layer);
        }
        return $result;
    }

    /**
     * Create a server from an array of LayerDefinitions
     *
     * @param LayerDefinition[] $definitions
     * @return LayarPOIServer
     */
    public function createLayarPOIServerFromLayerDefinitions(array $definitions)
    {
        $result = new LayarPOIServer();
        foreach ($definitions as $definition) {
            $layer = new Layer($definition->name, $this->developerId, $this->developerKey);
            if ($definition->getSourceType() == LayerDefinition::DSN) {
                $poiConnector = new $definition->connector($definition->source["dsn"], $definition->source["username"], $definition->source["password"]);
            } else if ($definition->getSourceType() == LayerDefinition::FILE) {
                $poiConnector = new $definition->connector($definition->source);
            }
            foreach ($definition->connectorOptions as $optionName => $option) {
                $poiConnector->setOption($optionName, $option);
            }
            // for WebApi: pass full definition object
            if (method_exists($poiConnector, 'initDefinition')) {
                $poiConnector->initDefinition($definition);
            }
            $layer->setPOIConnector($poiConnector);
            $result->addLayer($layer);
        }
        return $result;
    }

    /**
     * Create a server from a PorPOISeConfig object
     *
     * @param PorPOISeConfig $config
     * @return LayarPOIServer
     */
    public static function createLayarPOIServer(PorPOISeConfig $config)
    {
        $factory = new self($config->developerID, $config->developerKey);
        return $factory->createLayarPOIServerFromLayerDefinitions($config->layerDefinitions);
    }
}

