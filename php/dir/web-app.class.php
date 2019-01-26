<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * Includes all PorPOISe source files
 *
 * @package PorPOISe
 */


/** html template */
require_once(PORPOISE_CONFIG_PATH . DIRECTORY_SEPARATOR . "template.php");

/**
 * Basic web app, handler OAuth login, logout and persistence.
 * Extend for application specific purposes.
 *
 * @package PorPOISe
 */
class WebApp {
	public $layerName;

	protected $http; // OAuth aware http object
	protected $user; // User persistence object
	protected $script_name = null; // Use this to override script name for method getActionUrl()
	protected $definition; // LayerDefinition

	private $sessionStarted = false;

	/**
	 * Initialize WebApp, Http and User classes
	 * @return void
	 * @param LayerDefinition $definition
	 */
	public function __construct(LayerDefinition $definition) {
		$this->layerName = $definition->name;
		$this->definition = $definition;
	}

	/**
	 * Start a named session object
	 * @return void
	 */
	protected function session_start() {
		if ($this->sessionStarted) return;
		if (isset($_REQUEST['userId'])) 
			session_id($_REQUEST['userId']); // kludge
		session_name('PorPOISe');
		if (!@session_start()) {
			throw new Exception('Could not initialize new session', 500);
		}
		$this->sessionStarted = true;
	}

	/**
	* Initialize OAuth aware HTTP requester, get parameters from OAuthSetup
	* @return EpiOAuth $http
	* @param OAuthSetup $oauthSetup
	*/
	protected function httpInit(OAuthSetup $oauthSetup) {
		$http = new HttpRequest();
		$http->init($oauthSetup);
		return $http;
	}


	/**
	 * initialize private User persistence object
	 * @return void
	 * @param LayerDefinition $definition
	 */
	protected function userInit(LayerDefinition $definition) {

		// get data source.
		// NOTE: only Database persistence implemented for now
		if ($definition->getSourceType() === LayerDefinition::DSN) {
			$source = $definition->source;
			$this->user = new DbUser($source['dsn'], $source['username'], $source['password']);
		} else {
			$this->user = new DummyUser();
		}

		// try session
		if (isset($_SESSION[$this->layerName . 'User'])) {
			$this->user->fromJson($_SESSION[$this->layerName . 'User']);
			return;
		} elseif (isset($_COOKIE[$this->layerName . 'Id'])) {
			// restore user data from persistent storage keyed by cookie
			$this->user->getById($_COOKIE[$this->layerName . 'Id']);
			$_SESSION[$this->layerName . 'User'] = $this->user->toJson();
		} elseif (isset($_REQUEST['userId'])) {
			// restore user data from persistent storage keyed by Layar UID
			$this->user->getById($_REQUEST['userId']);
			$_SESSION[$this->layerName . 'User'] = $this->user->toJson();
		}
	}

	/**
	 * Main entry point, dispatch web requests according to 'action' parameter.
	 * All public methods of subclass are callable as action.
	 *
	 * @return void
	 */
	public function handleRequest() {
		$this->http = $this->httpInit($this->definition->oauth);		
		$this->session_start();
		$this->userInit($this->definition);
		
		$action = (string)$_REQUEST['action'];
		// catch Exceptions
		try {
			switch ($action) {
				case 'login':
					$view = $this->login();
					break;
				case 'callback':
					$view = $this->callback();
					break;
				case 'logout':
					$view = $this->logout();
					break;
				case 'handleRequest':
				case '__construct':
					throw new Exception('Forbidden', 403);
					break;
				default:
					// these actions are made against a authenticated http class
					// so initialize token for three-legged authentication
					// only if user is logged-in
					if ($this->user->getApp_user_name()) {
						try {
							$this->initToken();
						} catch (Exception $e) {
							// FIXME take appropriate action, e.g. logout and login depending on error
						}
					}
					// check if user is logged in using $this->user->getApp_user_name()
					// in method call.
					
					// poor man's reflection; call public method if it exists in called (sub)class
					// private and protected methods are not exposed
					if (method_exists($this, $action)) {
						$meth = new ReflectionMethod($this, $action);
						if (!$meth->isPublic()) {
							throw new Exception('Forbidden', 403);
						}
						$view = $this->{$action}();
					} else {
						$view = array(
							'title' => 'Error',
							'content' => 'Unknown action.'
						);
					}
			}
			$this->render($view);
			return;

		} catch (EpiOAuthUnauthorizedException $e) {
			$err = 'Unauthorized: ' . $e->getMessage();
		} catch (EpiOAuthBadRequestException $e) {
			$err = 'Bad Request: ' . $e->getMessage();
		} catch (EpiOAuthException $e) {
			$err = 'Unknown OAuth Error: ' . $e->getMessage();
		} catch (Exception $e) {
			$err = 'Unknown Error: ' . $e->getMessage();
		}
		$view = array(
			'title' => 'Error',
			'content' => $err
		);
		$this->render($view);
	}

	/**
	 * Render view with template, output HTML
	 * @param array $view
	 *
	 * @todo override template class per layer name
	 */
	protected function render(array $view) {
		$user = (isset($this->user)) ? $this->user->getApp_user_name() : '';
		if ($user) {
			$view['user'] = $user;
			$view['logout'] = $this->getActionUrl('logout');
		} else {
			$view['login'] = $this->getActionUrl('login');
		}
		Template::render($view);
	}

	/**
	 *
	 * @return string $url link to call action on current layer
	 * @param object $action
	 * @param array $opts optional named query parameters
	 */
	protected function getActionUrl($action, array $opts = array()) {
		$url = sprintf('%s://%s%s?layerName=%s&action=%s',
			(isset($_SERVER['HTTPS']) ? 'https' : 'http'),
			$_SERVER["HTTP_HOST"],
			(($this->script_name) ? $this->script_name : $_SERVER['SCRIPT_NAME']),
			$this->layerName,
			urlencode($action)
		);
		foreach ($opts as $q => $v) {
			$url .= '&' . $q . '=' . urlencode($v);
		}
		return $url;
	}

	/**
	 * First stage OAuth login action
	 * @return array $view
	 *
	 * Example callback redirects:
	 * http://example.com/PorPOISe/web/web.php?layerName=linkedin&action=callback&oauth_token=xxyyzz&oauth_verifier=123456
	 * Some services ignore the callback parem, you have to register the callback url, e.g.
	 * http://example.com/PorPOISe/web/web.php?layerName=foursquare&action=callback
	 */
	public function login() {
		// construct callback: current base URL with param action=callback
		$callback = $this->getActionUrl('callback');

		// in case no callback is sent, use value 'oob' - out of band
		// oauth_callback=oob
		$params = array('oauth_callback' => $callback);
		// get OAuth tokens
		$token = $this->http->getRequestToken($params);
		// persist tokens
		$this->initToken($token);
		$authUrl = $this->http->getAuthorizeUrl($token, $params);
		$view = array(
			'title' => 'Login',
			'content' => sprintf('<div class="cntr"><a href="%s" class="btn">Login</a></div>', $authUrl) // FIXME
		);
		return $view;
	}

	/**
	 * Second stage OAuth login action,
	 * Callback called by remote server, persist user data
	 *
	 * @return array $view
	 */
	// oauth_token, oauth_verifier
	public function callback() {
		$token = $this->initToken();
		if ($token->oauth_token != @$_GET['oauth_token']) {
			throw new Exception('Token mismatch', 403);
		}
		if (isset($_GET['oauth_verifier'])) {
			$param = array('oauth_verifier' => $_GET['oauth_verifier']);
		} else {
			$param = null;
		}
		// get OAuth Acces Token
		$token = $this->http->getAccessToken($param);
		// persist token
		$this->initToken($token);
		// persists $token->oauth_token, $token->oauth_token_secret
		$this->persistUser($token);
		$view = array(
			'title' => 'Logged In',
			'content' => sprintf('Logged in to layer <b>%s</b>', $this->layerName)
		);
		return $view;
	}

	/**
	 * Logout action, delete persisted user data
	 * @return array $view
	 */
	public function logout() {
		$this->session_start();
		session_destroy();

		// delete from persistent storage
		if ($this->user) {
			$this->user->delete();
		}
		$view = array(
			'title' => 'Logged Out',
			'content' => sprintf('Logged out of layer <b>%s</b>', $this->layerName)
		);
		return $view;
	}

	/**
	 * Save common user data.
	 * Override this function in a child class to add additional user data:
	 * - app_username
	 * - app_uid
	 * - layar_uid
	 *
	 * @return void
	 * @param object $token {oauth_token, oauth_token_secret}
	 */
	protected function persistUser($token) {
		$this->user->setOauth_token($token->oauth_token);
		$this->user->setOauth_token_secret($token->oauth_token_secret);
		$this->user->save();

		$id = $this->user->getId();

		// set UID cookie
		$exp = time() + 3600*24*365*2; // Expire after two years
		setcookie($this->layerName . 'Id', $id, $exp, '/');

		// save serialized data to session
		$_SESSION[$this->layerName . 'User'] = $this->user->toJson();
	}

	/**
	 * Initialize http object with OAuth tokens.
	 * Retrieves OAuth token from User object or Session if argument is empty.
	 * Throws exception on failure.
	 *
	 * @return object $token {oauth_token, oauth_token_secret}
	 * @param object $token[optional]
	 */
	protected function initToken($token = null) {
		$this->http = $this->httpInit($this->definition->oauth);
		$this->session_start();
		$this->userInit($this->definition);
		if (empty($token)) {
			// restore from User object
			if ($this->user && $this->user->getOauth_token()) {
				$token = (object) array(
					'oauth_token' => $this->user->getOauth_token(),
					'oauth_token_secret' => $this->user->getOauth_token_secret()
				);
			// restore from Session (temporary credentials for first callback)
			} elseif (isset($_SESSION['token'])) {
				$token = $_SESSION['token'];
			}
		}

		// persist token in session
		if (!empty($token)) {
			$_SESSION['token'] = (object) array(
				'oauth_token' => $token->oauth_token,
				'oauth_token_secret' => $token->oauth_token_secret
			);
		} else {
			throw new Exception('Missing OAuth token', 500);
		}

		// set user tokens for OAuth request
		$this->http->setToken($token->oauth_token, $token->oauth_token_secret);
		return $token;
	}


} // WebApp

/**
 * Server for WebApp
 *
 * @package PorPOISe
 */
class WebAppServer {
	protected $webApps = array();

	public function __construct() {

	}

	public function handleRequest() {
		if (isset($_REQUEST['layerName']) && isset($this->webApps[$_REQUEST['layerName']])) {
			$app = $this->webApps[$_REQUEST['layerName']];
			$app->handleRequest();
		} else {
			$view = array(
				'title' => 'General Error',
				'content' => '<p>There has been a general error.</p><p>This Layer is unable to process your request.</p>'
			);
			Template::render($view);
		}
	}

	public function addWebApp($webApp) {
		$this->webApps[$webApp->layerName] = $webApp;
	}

}


/**
 * Factory class to create WebAppServers
 *
 * @package PorPOISe
 */

class WebAppServerFactory {

	protected function __construct() {

	}

	protected function createServerFromLayerDefinitions(array $definitions) {
		$result = new WebAppServer();
		foreach ($definitions as $definition) {
			// see if an extended WebApp class exists for current definition
			if (isset($definition->web_app)) {
				$appClass = $definition->web_app["name"];
				$appFile = (string)$definition->web_app["file"];
				include_once($appFile);
				$webApp = new $appClass($definition);
			} else {
				$webApp = new WebApp($definition);
			}
			$result->addWebApp($webApp);
		}
		return $result;
	}

	/**
	 * initialize a new WebAppServer
	 * Create a new web app for every definition and add it to the WebAppServer
	 * @return $server WebAppServer
	 * @param object $definitions
	 */
	public static function createWebAppServer(PorPOISeConfig $config) {
		$factory = new self();
		return $factory->createServerFromLayerDefinitions($config->layerDefinitions);
	}

}
