<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV / 2009-2010 SQUIO
 * Released under a permissive license (see LICENSE)
 */

/**
 * Classes for holding user data for personalized web apps
 *
 * @package PorPOISe
 */

/**
 * User data
 *
 * @package PorPOISe
 */
abstract class User {
	/**
	 * @var String Unique user ID, auto generated
	 */
	private $id = null;
	
	/**
	 * @var String Layar User ID
	 */
	private $layar_uid = null;
	
	/**
	 * @var String Application specific user ID
	 */
	private $app_uid = null;

	/**
	 * @var String Application specific username
	 */
	private $app_user_name = null;
	
	/**
	 * @var String OAuth user token
	 */
	private $oauth_token = null;

	/**
	 * @var String OAuth user token secret
	 */
	private $oauth_token_secret = null;

	/**
	 * @var String last updated timestamp
	 */
	private $updated = null;

	/**
	 * Persist OAuth credentials
	 *
	 */
	public abstract function save();
	
	/**
	 * Look up user data by id or layar_uid.
	 * It is assumed that both native ID and layar_uid are unique
	 */
	public abstract function getById($id);

	/**
	 * Delete persisted OAuth credentials
	 *
	 */
	public abstract function delete();

    /**
     * Returns $app_uid.
     * @see User::$app_uid
     */
    public function getApp_uid()
    {
        return $this->app_uid;
    }
    
    /**
     * Sets $app_uid.
     * @param object $app_uid
     * @see User::$app_uid
     */
    public function setApp_uid($app_uid)
    {
        $this->app_uid = $app_uid;
    }
    
    /**
     * Returns $app_user_name.
     * @see User::$app_user_name
     */
    public function getApp_user_name()
    {
        return $this->app_user_name;
    }
    
    /**
     * Sets $app_user_name.
     * @param object $app_user_name
     * @see User::$app_user_name
     */
    public function setApp_user_name($app_user_name)
    {
        $this->app_user_name = $app_user_name;
    }
    
    /**
     * Returns $id.
     * @see User::$id
     */
    public function getId()
    {
    	if (!$this->id) {
    		$this->id = $token = md5(uniqid(mt_rand(), true));
    	}
        return $this->id;
    }
    
    /**
     * Sets $id.
     * @param object $id
     * @see User::$id
     */
    public function setId($id)
    {
        $this->id = $id;
    }
    
    /**
     * Returns $layar_uid.
     * @see User::$layar_uid
     */
    public function getLayar_uid()
    {
        return $this->layar_uid;
    }
    
    /**
     * Sets $layar_uid.
     * @param object $layar_uid
     * @see User::$layar_uid
     */
    public function setLayar_uid($layar_uid)
    {
        $this->layar_uid = $layar_uid;
    }
    
    /**
     * Returns $oauth_token.
     * @see User::$oauth_token
     */
    public function getOauth_token()
    {
        return $this->oauth_token;
    }
    
    /**
     * Sets $oauth_token.
     * @param object $oauth_token
     * @see User::$oauth_token
     */
    public function setOauth_token($oauth_token)
    {
        $this->oauth_token = $oauth_token;
    }
    
    /**
     * Returns $oauth_token_secret.
     * @see User::$oauth_token_secret
     */
    public function getOauth_token_secret()
    {
        return $this->oauth_token_secret;
    }
    
    /**
     * Sets $oauth_token_secret.
     * @param object $oauth_token_secret
     * @see User::$oauth_token_secret
     */
    public function setOauth_token_secret($oauth_token_secret)
    {
        $this->oauth_token_secret = $oauth_token_secret;
    }
    
    /**
     * Returns $updated seconds since Unix Epoch.
     * @see User::$updated
     */
    public function getUpdated()
    {
        return $this->updated;
    }
    
    /**
     * Sets $updated.
     * @param object $updated
     * @see User::$updated
     */
    public function setUpdated($updated)
    {
        $this->updated = $updated;
    }

	/**
	 * Serialize data fields to JSON array
	 * @return string JSON serialized data fields
	 */
	public function toJson() {
		$fields = array('id', 'layar_uid', 'app_uid', 'app_user_name', 'oauth_token', 'oauth_token_secret');
		$props = array();
		foreach ($fields as $field) {
			$props[$field] = $this->{$field};
		}
		return json_encode($props);
	}

	/**
	 * Initiaize data fields from JSON array
	 * @return void 
	 * @param string $json serialized data fields
	 */
	public function fromJson($json) {
		$props = json_decode($json, true);
		foreach ($props as $k => $v) {
			call_user_func(array($this, 'set' . ucfirst($k)), $v);
		}
	}
	
	/**
	 * Reset all user data fields to undefined
	 * @return void 
	 */
	public function clear() {
		$fields = array('id', 'layar_uid', 'app_uid', 'app_user_name', 'oauth_token', 'oauth_token_secret', 'updated');
		foreach ($fields as $field) {
			$this->{$field} = null;
		}		
	} 

}

/**
 * User - dummy implementation
 * WARNINGL this class does not provide data persistence! 
 * 
 * @package PorPOISe
 */
class DummyUser extends User {
	
	/**
	 * This method does *NOT* save the user credentials
	 * but may still be useful as long as only session
	 * and cookie storage are used elsewhere.
	 *
	 */
	public function save() {
		// noop
	}

	/**
	 * This method clears user credentials from
	 * the session- or memory persistence.
	 */
	public function delete() {
		$this->clear();
	}

	public function getById($id) {
		// noop
	}
}


/**
 * User object persistence using SQL database
 *
 * @package PorPOISe
 */
class DbUser extends User {
	/** @var string DSN */
	protected $source;
	/** @var string username */
	protected $username;
	/** @var string password */
	protected $password;
	/** @var PDO PDO instance */
	protected $pdo;

	/**
	 * Constructor
	 * @param string $source DSN of the database
	 * @param string $username Username to access the database
	 * @param string $password Password to go with the username
	 */
	public function __construct($source, $username = "", $password = "") {
		$this->source = $source;
		$this->username = $username;
		$this->password = $password;
	}
	
	/**
	 * Get PDO instance
	 *
	 * @return PDO
	 */
	protected function getPDO() {
		if (empty($this->pdo)) {
			$this->pdo = new PDO ($this->source, $this->username, $this->password);

			$this->pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
			// force UTF-8 (Layar talks UTF-8 and nothing else)
			$sql = "SET NAMES 'utf8'";
			$stmt = $this->pdo->prepare($sql);
			$stmt->execute();
		}
		return $this->pdo;
	}



	public function save() {
		$fields = array('id', 'layar_uid', 'app_uid', 'app_user_name', 'oauth_token', 'oauth_token_secret', 'updated');
		$sql = 'INSERT INTO User (' . implode(",", $fields) . ")
			        VALUES (:" . implode(",:", $fields) . ")";		
		try {
			$pdo = $this->getPDO();
			
			$stmt = $pdo->prepare($sql);
			$userArray = array();
			foreach ($fields as $field) {
				$userArray[$field] = call_user_func(array($this, 'get' . ucfirst($field)));
			}
			$userArray['updated'] = null; // force 'on update CURRENT_TIMESTAMP' clause
			$stmt->execute($userArray);
		} catch (PDOException $e) {
			throw new Exception("Database error: " . $e->getMessage());
		}
	}
	
	public function delete() {
		$fields = array('id', 'layar_uid', 'app_uid', 'app_user_name');
		try {
			$pdo = $this->getPDO();

			$clauses = array();
			$userArray = array();
			foreach ($fields as $field) {
				$val = call_user_func(array($this, 'get' . ucfirst($field)));
				if ($val) {
					$clauses []= "$field = :$field";
					$userArray[$field] = $val;
				}
			}
			$sql = 'DELETE FROM User WHERE ' . implode(" AND ", $clauses);
			$stmt = $pdo->prepare($sql);
			$stmt->execute($userArray);
		} catch (PDOException $e) {
			throw new Exception("Database error: " . $e->getMessage());
		}
		// reset all data fields
		$this->clear();		
	}

	/**
	 * Look up user data by id or layar_uid.
	 * It is assumed that both native ID and layar_uid are unique
	 *
	 */
	public function getById($id) {
		$fields = array('id', 'layar_uid', 'app_uid', 'app_user_name', 'oauth_token', 'oauth_token_secret', 'updated');
		try {
			$pdo = $this->getPDO();

			$clauses = array();
			$userArray = array();
			foreach ($fields as $field) {
				$val = call_user_func(array($this, 'get' . ucfirst($field)));
				if ($val) {
					$clauses []= "$field = :$field";
					$userArray[$field] = $val;
				}
			}
			$sql = 'SELECT '. implode(",", $fields) .' FROM User WHERE id=:id OR layar_uid=:id';
			$stmt = $pdo->prepare($sql);
			$stmt->execute(array('id' => $id));

			$row = $stmt->fetch(PDO::FETCH_ASSOC);
			if ($stmt->rowCount()) {
				foreach ($row as $k => $v) {
					call_user_func(array($this, 'set' . ucfirst($k)), $v);
				}
			}
			
		} catch (PDOException $e) {
			throw new Exception("Database error: " . $e->getMessage());
		}
	}

	public function getByLayarId($layar_uid) {
		$fields = array('id', 'layar_uid', 'app_uid', 'app_user_name', 'oauth_token', 'oauth_token_secret', 'updated');
		try {
			$pdo = $this->getPDO();

			$clauses = array();
			$userArray = array();
			foreach ($fields as $field) {
				$val = call_user_func(array($this, 'get' . ucfirst($field)));
				if ($val) {
					$clauses []= "$field = :$field";
					$userArray[$field] = $val;
				}
			}
			$sql = 'SELECT '. implode(",", $fields) .' FROM User WHERE layar_uid=:id';
			$stmt = $pdo->prepare($sql);
			$stmt->execute(array('id' => $layar_uid));

			$row = $stmt->fetch(PDO::FETCH_ASSOC);
			if ($stmt->rowCount()) {
				foreach ($row as $k => $v) {
					call_user_func(array($this, 'set' . ucfirst($k)), $v);
				}
			}
			
		} catch (PDOException $e) {
			throw new Exception("Database error: " . $e->getMessage());
		}
	}
	
}
