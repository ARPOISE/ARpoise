<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * PorPOISe configuration class
 *
 * @package PorPOISe
 */

/**
 * This class holds global PorPOISe configuration
 *
 * Contains developer key and ID, as well as layer definitions
 *
 * @package PorPOISe
 */
class PorPOISeConfig {
	/** @var string Source file of configuration */
	protected $source;

	/** @var string DEPRECATED Developer key */
	public $developerKey = "";
	/** @var int DEPRECATED Developer ID */
	public $developerID = "";
	/** @var LayerDefinition[] Layers */
	public $layerDefinitions;
	/** @var string[] POI connectors */
	public $poiConnectors;

	/**
	 * Constructor
	 *
	 * @param string $source Source to load from (filename or XML)
	 * @param bool $fromString $source is an XML string, not a filename. Default FALSE
	 */
	public function __construct($source = NULL, $fromString = FALSE) {
		$this->layerDefinitions = array();
		$this->poiConnectors = array();
		if (!empty($source)) {
			$this->load($source, $fromString);
		}
	}

	/**
	 * Load config from XML
	 *
	 * @param string $source Filename or XML string
	 * @param bool $fromString $source is an XML string, not a filename. Default FALSE
	 *
	 * @return void
	 * @throws Exception on XML failure
	 */
	public function load($source, $fromString = FALSE) {
		$this->source = $source;

		$config = new SimpleXMLElement($this->source, 0, !$fromString);
		if (!empty($config->{"developer-id"})) {
			$this->developerID = (string)$config->{"developer-id"};
		}
		if (!empty($config->{"developer-key"})) {
			$this->developerKey = (string)$config->{"developer-key"};
		}

		/* load the names of connector classes and which files they are in */
		foreach ($config->xpath("connectors/connector") as $node) {
			$this->connectors[(string)$node->name] = (string)$node->file;
		}

		/* load layers */
		foreach ($config->xpath("layers/layer") as $node) {
			$def = new LayerDefinition();
			$def->name = (string)$node->name;
			$def->connector = trim((string)$node->connector);
			if (!empty($node->connector->options)) {
				foreach ($node->connector->options->children() as $optionNode) {
					$def->connectorOptions[$optionNode->getName()] = (string)$optionNode;
				}
			}
			/* check to see if we need to load the connector */
			if (!class_exists($def->connector)) {
				/* include the connector's definition if it exists*/
				if (!empty($this->connectors[$def->connector])) {
					require_once($this->connectors[$def->connector]);
				} else {
					throw new Exception(sprintf("Unknown connector: %s", $def->connector));
				}
			}

			/* load the data source information */
			if (isset($node->source->dsn)) {
				$def->setSourceType(LayerDefinition::DSN);
				$def->source["dsn"] = (string)$node->source->dsn;
				if (isset($node->source->username)) {
					$def->source["username"] = (string)$node->source->username;
				}
				if (isset($node->source->password)) {
					$def->source["password"] = (string)$node->source->password;
				}
			} else {
				$def->source = (string)$node->source;
			}
			
			/* web app configuration */
			if (isset($node->web_app)) {
				$def->web_app["name"] = (string)$node->web_app->name;
				$def->web_app["file"] = (string)$node->web_app->file;
			}
			
			/* load OAuth settings */
			if (isset($node->oauth)) {
				$oauth = $def->oauth;
				$oauth->setConsumerKey((string)$node->oauth->consumer_key);
				$oauth->setSecretKey((string)$node->oauth->secret_key);
				$baseUrl = (string)$node->oauth->baseUrl;
				$oauth->setRequestTokenUrl($baseUrl . (string)$node->oauth->tokenPath->request);
				$oauth->setAccessTokenUrl($baseUrl . (string)$node->oauth->tokenPath->access);
				$oauth->setAuthorizeTokenUrl($baseUrl . (string)$node->oauth->tokenPath->authorize);
			}
			
			$this->layerDefinitions[] = $def;
		}
	}

	/**
	 * Save config to XML
	 *
	 * For saving the configuration to the config file the file must be
	 * writable. Only do this on a trusted environment because a writable
	 * config file is a security hazard.
	 *
	 * @param bool $asString Return XML as string instead of saving to file.
	 *
	 * @return mixed Number of bytes written when writing to a file, XML
	 * string when saveing as a string. FALSE on failure
	 */
	public function save($asString = FALSE) {
		$dom = new DOMDocument("1.0", "UTF-8");
		$dom->formatOutput = TRUE;

		$root = $dom->appendChild($dom->createElement("porpoise-configuration"));
		
		$root->appendChild($dom->createElement("developer-id", $this->developerID));
		$root->appendChild($dom->createElement("developer-key", $this->developerKey));

		$connectors = $root->appendChild($dom->createElement("connectors"));
		foreach ($this->connectors as $name => $file) {
			$connector = $connectors->appendChild($dom->createElement("connector"));
			$connector->appendChild($dom->createElement("name", $name));
			$connector->appendChild($dom->createElement("file", $file));
		}

		$layers = $root->appendChild($dom->createElement("layers"));
		foreach ($this->layerDefinitions as $layerDefinition) {
			$layer = $layers->appendChild($dom->createElement("layer"));
			$layer->appendChild($dom->createElement("name", $layerDefinition->name));
			$connector = $layer->appendChild($dom->createElement("connector", $layerDefinition->connector));
			foreach ($layerDefinition->connectorOptions as $key => $value) {
				$connector->appendChild($dom->createElement($key, $value));
			}
			$source = $layer->appendChild($dom->createElement("source"));
			switch($layerDefinition->getSourceType()) {
			case LayerDefinition::DSN:
				$source->appendChild($dom->createElement("dsn", $layerDefinition->source["dsn"]));
				$source->appendChild($dom->createElement("username", $layerDefinition->source["username"]));
				$source->appendChild($dom->createElement("password", $layerDefinition->source["password"]));
				break;
			case LayerDefinition::FILE:
				$source->appendChild($dom->createTextNode($layerDefinition->source));
				break;
			default:
				throw new Exception(sprintf("Invalid source type in configuration: %d\n", $layerDefinition->getSourceType()));
			}
		}

		if ($asString) {
			return $dom->saveXML();
		} else {
			return $dom->save($this->source);
		}
	}
}

/**
 * Class for holding a layer definition
 *
 * @package PorPOISe
 */
class LayerDefinition {
	/** Magic number to indicate source is a file */
	const FILE = 1;
	/** Magic number to indicate source is a DSN */
	const DSN = 2;

	/** @var string Layer name */
	public $name;
	/** @var mixed Layer source */
	public $source;
	/** @var string Name of connector class */
	public $connector;
	/** @var array Connector-specific options */
	public $connectorOptions = array();
	/** @var array OAuth options */
	public $oauth = null;

	/** @var int Source type */
	protected $sourceType = self::FILE;
	
	public function __construct() {
		$this->oauth = new OAuthSetup();
	}

	/**
	 * Set source type of this layer
	 *
	 * Valid values are LayerDefinition::FILE and LayerDefinition::DSN. Resets
	 * the current source value.
	 *
	 * @param int $type
	 *
	 * @return void
	 */
	public function setSourceType($type) {
		$this->sourceType = $type;
		switch ($this->sourceType) {
		case self::DSN:
			$this->source = array("dsn" => NULL, "username" => NULL, "password" => NULL);
			break;
		case self::FILE:
			$this->source = NULL;
			break;
		default:
			throw new Exception(sprintf("Invalid source type for layer: %d\n", $type));
		}
	}

	/**
	 * Get source type
	 *
	 * Returns the value set by setSourceType() or a default value if nothing
	 * has been explicitly set.
	 *
	 * @return int
	 */
	public function getSourceType() {
		return $this->sourceType;
	}
}

/**
 * Class for holding OAuth credentials and params
 * 
 * @package PorPOISe
 */
class OAuthSetup {
	protected $consumerKey = null;
	protected $secretKey = null;
	protected $requestTokenUrl = '';
	protected $accessTokenUrl = '';
	protected $authorizeTokenUrl = '';
	
    /**
     * Returns $accessTokenUrl.
     * @see OAuthSetup::$accessTokenUrl
     */
    public function getAccessTokenUrl()
    {
        return $this->accessTokenUrl;
    }
    
    /**
     * Sets $accessTokenUrl.
     * @param object $accessTokenUrl
     * @see OAuthSetup::$accessTokenUrl
     */
    public function setAccessTokenUrl($accessTokenUrl)
    {
        $this->accessTokenUrl = $accessTokenUrl;
    }
    
    /**
     * Returns $authorizeTokenUrl.
     * @see OAuthSetup::$authorizeTokenUrl
     */
    public function getAuthorizeTokenUrl()
    {
        return $this->authorizeTokenUrl;
    }
    
    /**
     * Sets $authorizeTokenUrl.
     * @param object $authorizeTokenUrl
     * @see OAuthSetup::$authorizeTokenUrl
     */
    public function setAuthorizeTokenUrl($authorizeTokenUrl)
    {
        $this->authorizeTokenUrl = $authorizeTokenUrl;
    }
    
    /**
     * Returns $consumerKey.
     * @see OAuthSetup::$consumerKey
     */
    public function getConsumerKey()
    {
        return $this->consumerKey;
    }
    
    /**
     * Sets $consumerKey.
     * @param object $consumerKey
     * @see OAuthSetup::$consumerKey
     */
    public function setConsumerKey($consumerKey)
    {
        $this->consumerKey = $consumerKey;
    }
    
    /**
     * Returns $requestTokenUrl.
     * @see OAuthSetup::$requestTokenUrl
     */
    public function getRequestTokenUrl()
    {
        return $this->requestTokenUrl;
    }
    
    /**
     * Sets $requestTokenUrl.
     * @param object $requestTokenUrl
     * @see OAuthSetup::$requestTokenUrl
     */
    public function setRequestTokenUrl($requestTokenUrl)
    {
        $this->requestTokenUrl = $requestTokenUrl;
    }
    
    /**
     * Returns $secretKey.
     * @see OAuthSetup::$secretKey
     */
    public function getSecretKey()
    {
        return $this->secretKey;
    }
    
    /**
     * Sets $secretKey.
     * @param object $secretKey
     * @see OAuthSetup::$secretKey
     */
    public function setSecretKey($secretKey)
    {
        $this->secretKey = $secretKey;
    }

}
