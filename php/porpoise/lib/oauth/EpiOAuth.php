<?php
class EpiOAuth
{
  public $version = '1.0';

  protected $requestTokenUrl;
  protected $accessTokenUrl;
  protected $authenticateUrl;
  protected $authorizeUrl;
  protected $consumerKey;
  protected $consumerSecret;
  protected $token;
  protected $tokenSecret;
  protected $signatureMethod;
  protected $debug = false;
  protected $useSSL = false;
  protected $headers = array();
  protected $userAgent = 'EpiOAuth (http://github.com/jmathai/twitter-async/tree/)';
  protected $connectionTimeout = 5;
  protected $requestTimeout = 30;

  public function addHeader($header)
  {
    if(is_array($header) && !empty($header))
      $this->headers = array_merge($this->headers, $header);
    elseif(!empty($header))
      $this->headers[] = $header;
  }

  public function getAccessToken($params = null)
  {
    $resp = $this->httpRequest('POST', $this->getUrl($this->accessTokenUrl), $params);
    return new EpiOAuthResponse($resp);
  }

  public function getAuthenticateUrl($token = null, $params = null)
  { 
    $token = $token ? $token : $this->getRequestToken($params);
    $addlParams = empty($params) ? '' : '&'.http_build_query($params, '', '&');
    return $this->getUrl($this->authenticateUrl) . '?oauth_token=' . $token->oauth_token . $addlParams;
  }

  public function getAuthorizeUrl($token = null, $params = null)
  {
    $token = $token ? $token : $this->getRequestToken($params);
    return $this->getUrl($this->authorizeUrl) . '?oauth_token=' . $token->oauth_token;
  }

  // DEPRECATED in favor of getAuthorizeUrl()
  public function getAuthorizationUrl($token = null)
  { 
    return $this->getAuthorizeUrl($token);
  }

  public function getRequestToken($params = null)
  {
    $resp = $this->httpRequest('POST', $this->getUrl($this->requestTokenUrl), $params);
    return new EpiOAuthResponse($resp);
  }

  public function getUrl($url)
  {
    if($this->useSSL === true)
      return preg_replace('/^http:/', 'https:', $url);

    return $url;
  }

  public function httpRequest($method = null, $url = null, $params = null, $isMultipart = false)
  {
    if(empty($method) || empty($url))
      return false;

	$this->useSSL(0 === strpos($url, 'https:'));

    if(empty($params['oauth_signature']))
      $params = $this->prepareParameters($method, $url, $params);

    switch($method)
    {
      case 'GET':
        return $this->httpGet($url, $params);
        break;
      case 'POST':
        return $this->httpPost($url, $params, $isMultipart);
        break;
      case 'DELETE':
        return $this->httpDelete($url, $params);
        break;

    }
  }

  public function setDebug($bool=false)
  {
    $this->debug = (bool)$bool;
  }

  public function setTimeout($requestTimeout = null, $connectionTimeout = null)
  {
    if($requestTimeout !== null)
      $this->requestTimeout = floatval($requestTimeout);
    if($connectionTimeout !== null)
      $this->connectionTimeout = floatval($connectionTimeout);
  }

  public function setToken($token = null, $secret = null)
  {
    $this->token = $token;
    $this->tokenSecret = $secret;
  } 

  public function useSSL($use = false)
  {
    $this->useSSL = (bool)$use;
  }

  protected function addDefaultHeaders($url, $oauthHeaders)
  {
    $_h = array('Expect:');
    $urlParts = parse_url($url);
    $oauth = 'Authorization: OAuth realm="' . $urlParts['scheme'] . '://' . $urlParts['host'] . $urlParts['path'] . '",';
    foreach($oauthHeaders as $name => $value)
    {
      $oauth .= "{$name}=\"{$value}\",";
    }
    $_h[] = substr($oauth, 0, -1);
    $_h[] = "User-Agent: {$this->userAgent}";
    $this->addHeader($_h);
  }

  protected function buildHttpQueryRaw($params)
  {
    $retval = '';
    foreach((array)$params as $key => $value)
      $retval .= "{$key}={$value}&";
    $retval = substr($retval, 0, -1);
    return $retval;
  }

  protected function curlInit($url)
  {
    $ch = curl_init($url);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_HTTPHEADER, $this->headers); 
    curl_setopt($ch, CURLOPT_TIMEOUT, $this->requestTimeout);
    curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, $this->connectionTimeout);
	// see http://github.com/jmathai/twitter-async/issues#issue/30
	if (ini_get('open_basedir') == '' && ini_get('safe_mode' == 'Off')) 
	{
    	curl_setopt($ch, CURLOPT_FOLLOWLOCATION, true);
	}
    if($this->useSSL === true)
    {
      curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
      curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, 0);
    }
    return $ch;
  }

  protected function emptyHeaders()
  {
    $this->headers = array();
  }

  protected function encode_rfc3986($string)
  {
    return str_replace('+', ' ', str_replace('%7E', '~', rawurlencode(($string))));
  }

  protected function generateNonce()
  {
    if(isset($this->nonce)) // for unit testing
      return $this->nonce;

    return md5(uniqid(rand(), true));
  }

  // parameters should already have been passed through prepareParameters
  // no need to double encode
  protected function generateSignature($method = null, $url = null, $params = null)
  {
    if(empty($method) || empty($url))
      return false;

    // concatenating and encode
    $concatenatedParams = $this->encode_rfc3986($this->buildHttpQueryRaw($params));

    // normalize url
    $normalizedUrl = $this->encode_rfc3986($this->normalizeUrl($url));
    $method = $this->encode_rfc3986($method); // don't need this but why not?

    $signatureBaseString = "{$method}&{$normalizedUrl}&{$concatenatedParams}";
    return $this->signString($signatureBaseString);
  }

  protected function httpDelete($url, $params) {
      $this->addDefaultHeaders($url, $params['oauth']);
      $ch = $this->curlInit($url);
      curl_setopt($ch, CURLOPT_CUSTOMREQUEST, 'DELETE');
      $resp = $this->curl->addCurl($ch);
      $this->emptyHeaders();
      return $resp;
  }

  protected function httpGet($url, $params = null)
  {
    if(count($params['request']) > 0)
    {
      $url .= '?';
      foreach($params['request'] as $k => $v)
      {
        $url .= "{$k}={$v}&";
      }
      $url = substr($url, 0, -1);
    }
    $this->addDefaultHeaders($url, $params['oauth']);
    $ch = $this->curlInit($url);
    $resp  = $this->curl->addCurl($ch);
    $this->emptyHeaders();

    return $resp;
  }

  protected function httpPost($url, $params = null, $isMultipart)
  {
    $this->addDefaultHeaders($url, $params['oauth']);
    $ch = $this->curlInit($url);
    curl_setopt($ch, CURLOPT_POST, 1);
    // php's curl extension automatically sets the content type
    // based on whether the params are in string or array form
    if($isMultipart)
      curl_setopt($ch, CURLOPT_POSTFIELDS, $params['request']);
    else
      curl_setopt($ch, CURLOPT_POSTFIELDS, $this->buildHttpQueryRaw($params['request']));
    $resp  = $this->curl->addCurl($ch);
    $this->emptyHeaders();

    return $resp;
  }

  protected function normalizeUrl($url = null)
  {
    $urlParts = parse_url($url);
    $scheme = strtolower($urlParts['scheme']);
    $host   = strtolower($urlParts['host']);
    $port = isset($urlParts['port']) ? intval($urlParts['port']) : 0;

    $retval = strtolower($scheme) . '://' . strtolower($host);

    if(!empty($port) && (($scheme === 'http' && $port != 80) || ($scheme === 'https' && $port != 443)))
      $retval .= ":{$port}";

    $retval .= $urlParts['path'];
    if(!empty($urlParts['query']))
    {
      $retval .= "?{$urlParts['query']}";
    }

    return $retval;
  }

  protected function prepareParameters($method = null, $url = null, $params = null)
  {
    if(empty($method) || empty($url))
      return false;

    $oauth['oauth_consumer_key'] = $this->consumerKey;
	if ($this->token)
	    $oauth['oauth_token'] = $this->token;
    $oauth['oauth_nonce'] = $this->generateNonce();
    $oauth['oauth_timestamp'] = !isset($this->timestamp) ? time() : $this->timestamp; // for unit test
    $oauth['oauth_signature_method'] = $this->signatureMethod;
	if (isset($params['oauth_verifier'])) {
    	$oauth['oauth_verifier'] = $params['oauth_verifier'];
    	unset($params['oauth_verifier']);
	}
    $oauth['oauth_version'] = $this->version;
    // encode all oauth values
    foreach($oauth as $k => $v)
      $oauth[$k] = $this->encode_rfc3986($v);
    // encode all non '@' params
    // keep sigParams for signature generation (exclude '@' params)
    // rename '@key' to 'key'
    $sigParams = array();
    $hasFile = false;
    if(is_array($params))
    {
      foreach($params as $k => $v)
      {
        if(strncmp('@',$k,1) !== 0)
        {
          $sigParams[$k] = $this->encode_rfc3986($v);
          $params[$k] = $this->encode_rfc3986($v);
        }
        else
        {
          $params[substr($k, 1)] = $v;
          unset($params[$k]);
          $hasFile = true;
        }
      }
      
      if($hasFile === true)
        $sigParams = array();
    }

    $sigParams = array_merge($oauth, (array)$sigParams);

    // sorting
    ksort($sigParams);

    // signing
    $oauth['oauth_signature'] = $this->encode_rfc3986($this->generateSignature($method, $url, $sigParams));
    return array('request' => $params, 'oauth' => $oauth);
  }

  protected function signString($string = null)
  {
    $retval = false;
    switch($this->signatureMethod)
    {
      case 'HMAC-SHA1':
        $key = $this->encode_rfc3986($this->consumerSecret) . '&' . $this->encode_rfc3986($this->tokenSecret);
        $retval = base64_encode(hash_hmac('sha1', $string, $key, true));
        break;
    }

    return $retval;
  }

  public function __construct($consumerKey, $consumerSecret, $signatureMethod='HMAC-SHA1')
  {
    $this->consumerKey = $consumerKey;
    $this->consumerSecret = $consumerSecret;
    $this->signatureMethod = $signatureMethod;
    $this->curl = EpiCurl::getInstance();
  }
}

class EpiOAuthResponse
{
  private $__resp;
  private $debug = false;

  public function __construct($resp)
  {
    $this->__resp = $resp;
  }

  public function __get($name)
  {
    if($this->__resp->code != 200)
      EpiOAuthException::raise($this->__resp, $this->debug);

    parse_str($this->__resp->data, $result);
    foreach($result as $k => $v)
    {
      $this->$k = $v;
    }

    return $result[$name];
  }

  public function __toString()
  {
    return $this->__resp->data;
  }
}

class EpiOAuthException extends Exception
{
  public static function raise($response, $debug)
  {
    $message = $response->data;
	$code = $response->code;
	
    switch($code)
    {
      case 400:
        throw new EpiOAuthBadRequestException($message, $code);
      case 401:
        throw new EpiOAuthUnauthorizedException($message, $code);
      default:
        throw new EpiOAuthException($message, $code);
    }
  }
}
class EpiOAuthBadRequestException extends EpiOAuthException{}
class EpiOAuthUnauthorizedException extends EpiOAuthException{}
