<?php
/*
 * PorPOISe
 * Copyright 2009 Squio.nl / SURFnet BV
 * Released under a permissive license (see LICENSE)
 *
 */

/**
 * Very basic mobile web template
 *
 * @package PorPOISe
 */
 
/**
 * Very basic mobile web template
 * 
 * @see WebApp::render()
 * 
 * @package PorPOISe
 */
class Template {
	
	public static function render($view) {
		header('Content-Type: text/html; charset=utf-8');
		?>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
    <head>
        <title>porPOISe: <?php echo @$view['title']; ?></title>
        <meta http-equiv="Content-Type" content="text/html;charset=utf-8" />
		<meta name="viewport" content="initial-scale=1.0; width=300" />
<style type="text/css">
body {
	width: 300px; /* useful for first gen android / iphone devices in vertical orientation, you may want to change this */
}
* {
    font-family: 'Lucida Grande', sans-serif;
	font-size: 12px;
}
#hdr {
	font-weight: bold;
	font-size: 22px;
	height: 25px;
	color: #06c;
}
#hdr img {
	float: left;
	width: 53px;
	height: 25px;
}
.sm, .sm a {
	font-size: smaller;
}
.clr {
	clear: both;
}
.cntr {
    text-align: center;
    padding: 12px;
}
.btn {
    padding: 4px;
    background-color: #06c;
    color: white;
    font-size: 20px;
    font-family: arial, helvetica, sans-serif;
    font-weight: bold;
    border: 1px outset black;
    text-decoration: none;
}
</style>
    </head>
    <body>
    	<div id="hdr"><img src="img/logo.png" />&#160;PorPOISe</div>
        <hr class="clr" />
		<h1><?php echo @$view['title']; ?></h1>
       <?php
        if ( !empty($view['user'])) {
	        echo '<div class="sm">You are currently logged in as <strong>'.$view['user'].'</strong></div>';
        }
        ?>
        <p><?php echo $view['content']; ?></p>
		<hr />
		<p class="sm">[ 
			<a href="http://dev.layar.com/media/getbacktoapp.html">Return to Layar</a><!-- see Layar v3.x docs -->
	       	<?php
	        if ( !empty($view['logout'])) {
		        printf('| <a href="%s">Logout</a>', $view['logout']);
	        }
	        if ( !empty($view['login'])) {
		        printf('| <a href="%s">Login</a>', $view['login']);
	        }
	        ?>
			| Powered by <a href="http://code.google.com/p/porpoise/">PorPOISe</a>
		]</p>
    </body>
</html><?php
	} // render
} // template

