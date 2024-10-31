<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * Defines where all custom files are located for your PorPOISe
 * installation. Backup this file in between upgrades or don't
 * overwrite it when upgrading to keep your configuration.
 *
 * @package PorPOISe
 */

/**
 * All other config is in config.xml in the directory pointed at
 * by PORPOISE_CONFIG_PATH. Change this constant to where your config
 * is at.
 */
 if (!defined('PORPOISE_CONFIG_PATH')) {
 	define("PORPOISE_CONFIG_PATH", "/var/www/arpoise.com/config/porpoise");
 }
