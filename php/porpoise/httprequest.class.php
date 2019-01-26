<?php
/*
 *  Class to integrate with generic OAuth API.
 *    Authenticated calls are done using OAuth and require access tokens for a user.
 *    API calls which do not require authentication do not require tokens
 *
 *  Based on EpiTwitter, see
 *    http://wiki.github.com/jmathai/twitter-async
 *
 *  @author Jaisen Mathai <jaisen@jmathai.com>
 *  @author Johannes la Poutre <info@squio.nl>
 */

require_once (dirname( __FILE__ ).'/lib/oauth/EpiCurl.php');
require_once (dirname( __FILE__ ).'/lib/oauth/EpiOAuth.php');


class HttpRequest extends EpiOAuth
{
    const SIGNATURE_METHOD = 'HMAC-SHA1';

    protected $userAgent = 'PorPOISe (http://code.google.com/p/porpoise/)';
    protected $isAsynchronous = false;
    protected $apiUrl = '';

	// keep last HTTP response in EpiCurlManager object
	protected $response = null;

    /* OAuth methods */
    public function delete($endpoint, $params = null)
    {
        return $this->request('DELETE', $endpoint, $params);
    }

    public function get($endpoint, $params = null)
    {
        return $this->request('GET', $endpoint, $params);
    }

    public function post($endpoint, $params = null)
    {
        return $this->request('POST', $endpoint, $params);
    }

    public function useAsynchronous($async = true)
    {
        $this->isAsynchronous = (bool)$async;
    }

    public function __construct($consumerKey = null, $consumerSecret = null, $oauthToken = null, $oauthTokenSecret = null)
    {
        parent::__construct($consumerKey, $consumerSecret, self::SIGNATURE_METHOD);
        $this->setToken($oauthToken, $oauthTokenSecret);
    }

    /**
     * Initialize OAuth parameters from OAuthSetup
     * @return void
     * @param OAuthSetup $oauthSetup
     */
    public function init($oauthSetup)
    {
        if ($oauthSetup instanceof OAuthSetup)
        {
            $this->requestTokenUrl = $oauthSetup->getRequestTokenUrl();
            $this->accessTokenUrl = $oauthSetup->getAccessTokenUrl();
            $this->authorizeUrl = $oauthSetup->getAuthorizeTokenUrl();
            $this->consumerKey = $oauthSetup->getConsumerKey();
            $this->consumerSecret = $oauthSetup->getSecretKey();
        } else
        {
            throw new Exception('Parameter should be instance of OAuthSetup');
        }
    }

    private function request($method, $endpoint, $params = null)
    {
        // parse the keys to determine if this should be multipart
        $isMultipart = false;
        if ($params)
        {
            foreach ($params as $k=>$v)
            {
                if (strncmp('@', $k, 1) === 0)
                {
                    $isMultipart = true;
                    break;
                }
            }
        }

        $url = $this->getUrl($this->apiUrl.$endpoint);

        $resp = $this->httpRequest($method, $url, $params, $isMultipart);
        // $resp->{data, code, headers}
		$this->response = $resp;
		
        return $resp->data;
    }

	public function getResponseCode() {
		return ($this->response instanceof EpiCurlManager) ? $this->response->code : 500;
	}

	public function getResponseData() {
		return ($this->response instanceof EpiCurlManager) ? $this->response->data : '';
	}

	public function getResponseHeaders() {
		return ($this->response instanceof EpiCurlManager) ? $this->response->headers : array();
	}

}
