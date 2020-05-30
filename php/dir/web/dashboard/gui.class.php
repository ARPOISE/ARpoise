<?php

/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Released under a permissive license (see LICENSE)
 */

/**
 * PorPOISe dashboard GUI
 *
 * @package PorPOISe
 * @subpackage Dashboard
 */

/**
 * Dashboard includes
 */
require_once ("mapskey.inc.php");

/**
 * GUI class
 *
 * All methods are static
 *
 * @package PorPOISe
 * @subpackage Dashboard
 */
class GUI
{

    /**
     * controls whether the GUI displays developer key
     */
    const SHOW_DEVELOPER_KEY = TRUE;

    /**
     * Callback for ob_start()
     *
     * Adds header and footer to HTML output and does post-processing
     * if required
     *
     * @param string $output
     *            The output in the buffer
     * @param int $state
     *            A bitfield specifying what state the script is in (start, cont, end)
     *            
     * @return string The new output
     */
    public static function finalize($output, $state)
    {
        $result = "";
        if ($state & PHP_OUTPUT_HANDLER_START) {
            $result .= self::createHeader();
        }
        $result .= $output;
        if ($state & PHP_OUTPUT_HANDLER_END) {
            $result .= self::createFooter();
        }
        return $result;
    }

    /**
     * Print a formatted message
     *
     * @param string $message
     *            sprintf-formatted message
     *            
     * @return void
     */
    public static function printMessage($message)
    {
        $args = func_get_args();
        /* remove first argument, which is $message */
        array_splice($args, 0, 1);
        vprintf($message, $args);
    }

    /**
     * Print an error message
     *
     * @param string $message
     *            sprintf-formatted message
     *            
     * @return void
     */
    public static function printError($message)
    {
        $args = func_get_args();
        $args[0] = sprintf("<p class=\"error\">%s</p>\n", $args[0]);
        call_user_func_array(array(
            "GUI",
            "printMessage"
        ), $args);
    }

    /**
     * Create a header
     *
     * @return string
     */
    public static function createHeader()
    {
        $result = <<<HTML1
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
<meta http-equiv="Content-Type" content="text/html;charset=UTF-8">
<title>ARpoise Directory - Directory Management Interface for ARpoise</title>
<link rel="stylesheet" type="text/css" href="styles.css">
<script type="text/javascript" src="prototype.js"></script>
HTML1;
        $result .= sprintf("\n<script type=\"text/javascript\" src=\"http://maps.google.com/maps/api/js?key=%s&sensor=false\"></script>\n", $GLOBALS["_googleMapsKey"]);
        $result .= <<<HTML2
<script type="text/javascript" src="scripts.js"></script>
</head>
<body>

<h1><img src="http://www.arpoise.com/images/arpoise_logo_rgb-1024.png" width=64></img>
ARpoise Directory Service</h1>

<div class="menu">
 <a href="?logout=true">Log out</a>
 <a href="?action=main">Home</a>
 <a href="?action=migrate" name="copylayers">Copy layers</a>
</div>

<div class="main">
HTML2;

        return $result;
    }

    /**
     * Create a footer
     *
     * @return string
     */
    public static function createFooter()
    {
        return <<<HTML
</div> <!-- end main div -->
</body>
</html>
HTML;
    }

    /**
     * Create a select box
     *
     * @param string $name
     * @param array $options
     * @param mixed $selected
     *
     * @return string
     */
    protected static function createSelect($name, $options, $selected = NULL)
    {
        $result = sprintf("<select name=\"%s\">\n", $name);
        foreach ($options as $value => $label) {
            $result .= sprintf("<option value=\"%s\"%s>%s</option>\n", $value, ($value == $selected ? " selected" : ""), $label);
        }
        $result .= "</select>\n";
        return $result;
    }

    /**
     * Create a Yes/No select box
     *
     * @param string $name
     * @param bool $checked
     *
     * @return string
     */
    protected static function createCheckbox($name, $checked = FALSE)
    {
        return self::createSelect($name, array(
            "1" => "Yes",
            "0" => "No"
        ), $checked ? "1" : "0");
    }

    /**
     * Create "main" screen
     *
     * @return string
     */
    public static function createMainScreen()
    {
        $result = "";
        //$result .= "<p>Welcome to ARpoise Directory</p>\n";
        //$result .= self::createMainConfigurationTable();
        $result .= "<p>Layers:</p>\n";
        $result .= self::createLayerList();
        return $result;
    }

    /**
     * Create a table displaying current configuration
     *
     * @return string
     */
    public static function createMainConfigurationTable()
    {
        $config = DML::getConfiguration();
        $result = "";
        $result .= "<table class=\"config\">\n";
        $result .= sprintf("<tr><td>Developer ID</td><td>%s</td></tr>\n", $config->developerID);
        $result .= sprintf("<tr><td>Developer key</td><td>%s</td></tr>\n", (self::SHOW_DEVELOPER_KEY ? $config->developerKey : "&lt;hidden&gt;"));
        $result .= sprintf("</table>\n");
        return $result;
    }

    /**
     * Create a list of layers
     *
     * @return string
     */
    public static function createLayerList()
    {
        $config = DML::getConfiguration();
        $result = "";
        $result .= "<ul>\n";
        foreach ($config->layerDefinitions as $layerDefinition) {
            $result .= sprintf("<li><a href=\"%s?action=layer&layerName=%s\">%s</a></li>\n", $_SERVER["PHP_SELF"], $layerDefinition->name, $layerDefinition->name);
        }
        $result .= "</ul>\n";
        return $result;
    }

    /**
     * Create a screen for viewing/editing a layer
     *
     * @param string $layerName
     *
     * @return string
     */
    public static function createLayerScreen($layerName)
    {
        $layerDefinition = DML::getLayerDefinition($layerName);
        if ($layerDefinition == NULL) {
            throw new Exception(sprintf("Unknown layer: %s\n", $layerName));
        }
        $result = "";
        $result .= sprintf("<p>Layers of the %s</p>\n", $layerName);

        $result .= "</p>\n";

        $result .= sprintf("<form accept-charset=\"utf-8\" action=\"?action=layer&layerName=%s\" method=\"POST\">\n", $layerName);


        
        $layerProperties = DML::getLayerProperties($layerName);
        $result .= sprintf("<table class=\"layer\">\n");
        $result .= sprintf("<tr><td>No content message</td><td><input type=\"text\" name=\"noPoisMessage\" value=\"%s\"></td></tr>\n", $layerProperties->noPoisMessage);

        $result .= sprintf("<caption><button type=\"submit\">Save</button></caption>\n");
        $result .= sprintf("</table>\n");
        
        $result .= "<br>\n";
        $result .= "<br>\n";
        $result .= "<br>\n";
        $result .= "<br>\n";
        $result .= "<br>\n";
        $result .= "<br>\n";
        $result .= "<br>\n";
        $result .= "<br>\n";
        $result .= "<br>\n";
        
        $result .= sprintf("</form>\n");

        
        /**
         * add new POI to layer, add entry to POI table below
         */
        $result .= "<table><tr><td height=\"175\"></td></tr></table>\n";
        $result .= sprintf("<p><a href=\"?action=newPOI&layerName=%s\">New Layer</a></p>\n", urlencode($layerName));
        $result .= self::createPOITable($layerName);
        return $result;
    }

    /**
     * Create a list of POIs for a layer
     *
     * @param string $layerName
     *
     * @return string
     */
    public static function createPOITable($layerName)
    {
        $result = "";
        $pois = DML::getPOIs($layerName);
        if ($pois === NULL || $pois === FALSE) {
            throw new Exception("Error retrieving POIs");
        }

        $result .= "<table class=\"pois\">\n";
        $result .= sprintf("<input type=\"hidden\" name=\"page\" value=\"layer\"> \n");

        $result .= "<tr><th>Id</th><th>Layer Name</th><th>Lat</th><th>Lon</th></tr>\n";

        $index = 0;

        foreach ($pois as $poi) {
            $index ++;

            $result .= "<tr>\n";
            $result .= sprintf("<td>%s</td>\n", $poi->id);
            $result .= sprintf("<td><a href=\"?action=poi&layerName=%s&poiID=%s\">%s</a></td>\n", urlencode($layerName), urlencode($poi->id), ($poi->title ? $poi->title : "&lt;no title&gt;"));

            // can now change lat/lon directly here TT 15 May 2016
            $result .= sprintf("<form accept-charset=\"utf-8\" action=\"?layerName=%s&action=poi&poiID=%s\" method=\"POST\">\n", urlencode($layerName), urlencode($poi->id));

            $result .= sprintf("<td><input type=\"text\" id=\"lat%s\" name=\"lat\" value=\"%s\" size=\"12\"></td>\n", $index, $poi->lat);
            $result .= sprintf("<td><input type=\"text\" id=\"lon%s\" name=\"lon\" value=\"%s\" size=\"12\"></td>\n", $index, $poi->lon);
            $result .= sprintf("<input type=\"hidden\" name=\"showLayer\" value=\"showLayer\"> \n");
            $result .= sprintf("<td><button type=\"submit\">Save</button></td>\n");

            $result .= sprintf("<input type=\"hidden\" name=\"id\" value=\"%s\"> \n", $poi->id);
            $result .= sprintf("<input type=\"hidden\" name=\"title\" value=\"%s\" > \n", $poi->title);
            $result .= sprintf("<input type=\"hidden\" name=\"line1\" value=\"%s\" > \n", $poi->line1);
            $result .= sprintf("<input type=\"hidden\" name=\"line2\" value=\"%s\" > \n", $poi->line2);
            $result .= sprintf("<input type=\"hidden\" name=\"line3\" value=\"%s\" > \n", $poi->line3);
            $result .= sprintf("<input type=\"hidden\" name=\"line4\" value=\"%s\" > \n", $poi->line4);
            
            $result .= sprintf("<input type=\"hidden\" name=\"isVisible\" value=\"%s\" > \n", $poi->isVisible);
            $result .= sprintf("<input type=\"hidden\" name=\"visibilityRange\" value=\"%s\" > \n", $poi->visibilityRange);

            $result .= sprintf("<input type=\"hidden\" name=\"baseURL\" value=\"%s\" > \n", $poi->object->baseURL);

            $result .= "</form>\n\n\n\n";

            // Delete button for each POI row
            $result .= sprintf("<td><form accept-charset=\"utf-8\" action=\"?action=deletePOI\" method=\"POST\">");
            $result .= sprintf("<input type=\"hidden\" name=\"layerName\" value=\"%s\"><input type=\"hidden\" name=\"poiID\" value=\"%s\">", urlencode($layerName), urlencode($poi->id));
            $result .= sprintf("<button type=\"submit\">DEL</button></form></td>\n");

            $result .= "</tr>\n";
        }
        $result .= "</table>\n";

        /**
         * hidden form
         */
        $result .= "<form id=\"POIlist\">\n";
        $index = 0;
        foreach ($pois as $poi) {
            $index ++;
            $result .= sprintf("<input type=\"hidden\" name=\"%s\" id=\"markerLat%s\" value=\"%s\">\n", $poi->title, $index, $poi->lat);
            $result .= sprintf("<input type=\"hidden\" name=\"%s\" id=\"markerLon%s\" value=\"%s\">\n", $poi->title, $index, $poi->lon);
        }

        $result .= "</form>\n\n\n\n";
        return $result;
    }

    public static function createHiddenAnimationSubtable($index, $event, Animation $animation)
    {
        $result = "";
        $result .= sprintf("<input type=\"hidden\" name=\"animations[%s][event]\" value=\"%s\">\n", $index, $event);
        $result .= sprintf("<input type=\"hidden\" name=\"animations[%s][type]\" value=\"%s\">\n", $index, $animation->type);
        $result .= sprintf("<input type=\"hidden\" name=\"animations[%s][length]\" value=\"%s\">\n", $index, $animation->length);
        $result .= sprintf("<input type=\"hidden\" name=\"animations[%s][delay]\" value=\"%s\">\n", $index, $animation->delay);
        $result .= sprintf("<input type=\"hidden\" name=\"animations[%s][interpolation]\" value=\"%s\">\n", $index, $animation->interpolation);
        $result .= sprintf("<input type=\"hidden\" name=\"animations[%s][interpolationParam]\" value=\"%s\">\n", $index, $animation->interpolationParam);
        $result .= sprintf("<input type=\"hidden\" name=\"animations[%s][persist]\" value=\"%s\">\n", $index, $animation->persist);
        $result .= sprintf("<input type=\"hidden\" name=\"animations[%s][repeat]\" value=\"%s\">\n", $index, $animation->repeat);
        $result .= sprintf("<input type=\"hidden\" name=\"animations[%s][from]\" value=\"%s\">\n", $index, $animation->from);
        $result .= sprintf("<input type=\"hidden\" name=\"animations[%s][to]\" value=\"%s\">\n", $index, $animation->to);
        $result .= sprintf("<input type=\"hidden\" name=\"animations[%s][axis]\" value=\"%s\">\n", $index, $animation->axisString());

        return $result;
    }

    public static function createHiddenActionSubtable($index, Action $action, $layerAction = FALSE)
    {
        $result = "";
        $result .= sprintf("<input type=\"hidden\" name=\"actions[%s][label]\" value=\"%s\">\n", $index, $action->label);
        $result .= sprintf("<input type=\"hidden\" name=\"actions[%s][uri]\" value=\"%s\">\n", $index, $action->uri);
        if (! $layerAction) {
            $result .= sprintf("<input type=\"hidden\" name=\"actions[%s][autoTriggerRange]\" value=\"%s\" >\n", $index, $action->autoTriggerRange);
            $result .= sprintf("<input type=\"hidden\" name=\"actions[%s][autoTriggerOnly]\" value=\"%s\" >\n", $index, $action->autoTriggerOnly);
        }
        $result .= sprintf("<input type=\"hidden\" name=\"actions[%s][contentType]\" value=\"%s\">\n", $index, $action->contentType);
        $result .= sprintf("<input type=\"hidden\" name=\"actions[%s][method]\" value=\"%s\">\n", $index, $action->method);
        $result .= sprintf("<input type=\"hidden\" name=\"actions[%s][activityType]\" value=\"%s\" >\n", $index, $action->activityType);
        $result .= sprintf("<input type=\"hidden\" name=\"actions[%s][params]\" value=\"%s\">\n", $index, implode(",", $action->params));
        if (! $layerAction) {
            $result .= sprintf("<input type=\"hidden\" name=\"actions[%s][closeBiw]\" value=\"%s\">\n", $index, $action->closeBiw);
        }
        $result .= sprintf("<input type=\"hidden\" name=\"actions[%s][showActivity]\" value=\"%s\">\n", $index, $action->showActivity);
        $result .= sprintf("<input type=\"hidden\" name=\"actions[%s][activityMessage]\" value=\"%s\">\n", $index, $action->activityMessage);

        return $result;
    }

    /**
     * Create a screen for a single POI
     *
     * @param string $layerName
     * @param string $poi
     *            POI to display in form. Leave empty for new POI
     *            
     * @return string
     */
    public static function createPOIScreen($layerName, $poi = NULL, $showLayer, $url)
    {
        if (empty($poi)) {
            $poi = new POI1D();
        }
        $result = "";
        $result .= sprintf("<p><a href=\"?action=layer&layerName=%s\">Back to %s</a></p>\n", urlencode($layerName), $layerName);
        $result .= sprintf("<form accept-charset=\"utf-8\" action=\"?layerName=%s&action=poi&poiID=%s\" method=\"POST\">\n", urlencode($layerName), urlencode($poi->id));
        $result .= "<table class=\"poi\">\n";
        $result .= sprintf("<input type=\"hidden\" name=\"page\" value=\"poi\"> \n");

        if (! empty($showLayer) && ! empty($url)) {
            $result .= sprintf("<input type=\"hidden\" name=\"doShowLayer\" value=\"%s?action=layer&layerName=%s\"> \n", $url, $layerName);
        }

        $result .= sprintf("<tr><td>ID</td><td><input type=\"hidden\" name=\"id\" value=\"%s\">%s</td></tr>\n", $poi->id, $poi->id);
        $result .= sprintf("<tr><td>Layer Name</td><td><input type=\"text\" name=\"title\" value=\"%s\" size=\"29\"></td></tr>\n", $poi->title);

        $result .= sprintf("<tr><td>Lat/Lon</td><td><input type=\"text\" id=\"lat1\" name=\"lat\" value=\"%s\" size=\"12\">\n", $poi->lat);
        $result .= sprintf("<input type=\"text\" id=\"lon1\" name=\"lon\" value=\"%s\" size=\"12\"></td></tr>\n", $poi->lon);

        $result .= sprintf("<tr><td>Is Visible</td><td>%s</td></tr>\n", self::createCheckbox("isVisible", $poi->isVisible));
        $result .= sprintf("<tr><td>Visibility in meters</td><td><input type=\"text\" name=\"visibilityRange\" value=\"%s\" size=\"12\"></td></tr>\n", $poi->visibilityRange);
        
        $result .= sprintf("<tr><td>Porpoise URL</td><td><input type=\"text\" name=\"baseURL\" value=\"%s\" size=\"29\"></td></tr>\n", $poi->object->baseURL);

        $result .= sprintf("<tr><td>Layer Title</td><td><input type=\"text\" name=\"line1\" value=\"%s\" size=\"29\"></td></tr>\n", $poi->line1);
        $result .= sprintf("<tr><td>Line 2</td>     <td><input type=\"text\" name=\"line2\" value=\"%s\" size=\"29\"></td></tr>\n", $poi->line2);
        $result .= sprintf("<tr><td>Line 3</td>     <td><input type=\"text\" name=\"line3\" value=\"%s\" size=\"29\"></td></tr>\n", $poi->line3);
        $result .= sprintf("<tr><td>Icon Name</td>  <td><input type=\"text\" name=\"line4\" value=\"%s\" size=\"29\"></td></tr>\n", $poi->line4);
        
        $result .= "<caption><button type=\"submit\">Save</button></caption>\n";
        $result .= "</table>\n";
        $result .= "</form>";
        return $result;
    }

    /**
     * Create a subtable for an action for inside a form
     *
     * @param string $index
     *            Index of the action in the actions[] array
     * @param Action $action
     *            The action
     * @param bool $layerAction
     *            Create a layer action form instead of a POI action form
     *            
     * @return string
     */
    public static function createActionSubtable($index, Action $action, $layerAction = FALSE)
    {
        $result = "";
        $result .= "<table class=\"action\">\n";
        $result .= sprintf("<tr><td>Label</td><td><input type=\"text\" name=\"actions[%s][label]\" value=\"%s\"></td></tr>\n", $index, $action->label);
        // $result .= sprintf("<tr><td>URI</td><td><input type=\"text\" name=\"actions[%s][uri]\" value=\"%s\"></td></tr>\n", $index, $action->uri);
        if (! $layerAction) {
            $result .= sprintf("<tr><td>Auto-trigger range</td><td><input type=\"text\" name=\"actions[%s][autoTriggerRange]\" value=\"%s\" size=\"2\"></td></tr>\n", $index, $action->autoTriggerRange);
            $result .= sprintf("<tr><td>Auto-trigger only</td><td>%s</td></tr>\n", self::createCheckbox(sprintf("actions[%s][autoTriggerOnly]", $index), $action->autoTriggerOnly));
        }

        $result .= sprintf("<tr><td>Show information</td><td>%s</td></tr>\n", self::createCheckbox(sprintf("actions[%s][showActivity]", $index), $action->showActivity));
        $result .= sprintf("<tr><td>Information message</td><td><input type=\"text\" name=\"actions[%s][activityMessage]\" value=\"%s\"></td></tr>\n", $index, $action->activityMessage);

        $result .= "</table>\n";

        return $result;
    }

    /**
     * Create a dropdown for selecting animation types
     *
     * @param string $name
     *
     * @return string
     */
    protected static function createAnimationTypeSelector($name, $selected = NULL)
    {
        $result = sprintf("<select name=\"%s\">", $name);
        foreach (array(
            "scale",
            "rotate",
            "transform",
            "fade",
            "destroy"
        ) as $animationType) {
            $result .= sprintf("<option value=\"%s\"%s>%s</option>", $animationType, ($selected == $animationType ? " selected" : ""), $animationType);
        }
        $result .= "</select>";
        return $result;
    }

    /**
     * Create a dropdown for selecting interpolation types
     *
     * @param string $name
     *
     * @return string
     */
    protected static function createInterpolationTypeSelector($name, $selected = NULL)
    {
        $result = sprintf("<select name=\"%s\">", $name);
        foreach (array(
            "linear",
            "cyclic",
            "sine",
            "halfsine"
        ) as $interpolationType) {
            $result .= sprintf("<option value=\"%s\"%s>%s</option>", $interpolationType, ($selected == $interpolationType ? " selected" : ""), $interpolationType);
        }
        $result .= "</select>";
        return $result;
    }

    /**
     * Create a dropdown for selecting animation events
     *
     * @param string $name
     *
     * @return string
     */
    protected static function createEventSelector($name, $selected = NULL)
    {
        $result = sprintf("<select name=\"%s\">", $name);
        foreach (array(
            "onCreate",
            "onFocus",
            "onClick"
        ) as $event) {
            $result .= sprintf("<option value=\"%s\"%s>%s</option>", $event, ($selected == $event ? " selected" : ""), $event);
        }
        $result .= "</select>";
        return $result;
    }

    /**
     * Create a subtable for an animation for inside a form
     *
     * @param string $index
     *            Index of the action in the actions[] array
     * @param string $event
     *            Event for which the animation is
     * @param Animation $animation
     *            The animation
     *            
     * @return string
     */
    public static function createAnimationSubtable($index, $event, Animation $animation)
    {
        $result = "";
        $result .= "<table class=\"animation\">\n";
        $result .= sprintf("<tr><td>Name</td><td><input type=\"text\" name=\"animations[%s][name]\" value=\"%s\"></td></tr>\n", $index, $animation->name);
        $result .= sprintf("<tr><td>Event</td><td>%s</td></tr>\n", self::createEventSelector(sprintf("animations[%s][event]", $index), $event));
        $result .= sprintf("<tr><td>Type</td><td>%s</td></tr>\n", self::createAnimationTypeSelector(sprintf("animations[%s][type]", $index), $animation->type));
        $result .= sprintf("<tr><td>Length</td><td><input type=\"text\" name=\"animations[%s][length]\" value=\"%s\"></td></tr>\n", $index, $animation->length);
        $result .= sprintf("<tr><td>Delay</td><td><input type=\"text\" name=\"animations[%s][delay]\" value=\"%s\"></td></tr>\n", $index, $animation->delay);
        $result .= sprintf("<tr><td>Interpolation</td><td>%s</td></tr>\n", self::createInterpolationTypeSelector(sprintf("animations[%s][interpolation]", $index), $animation->interpolation));
        $result .= sprintf("<tr><td>Persist</td><td>%s</td></tr>\n", self::createCheckbox(sprintf("animations[%s][persist]", $index), $animation->persist));
        $result .= sprintf("<tr><td>Repeat</td><td>%s</td></tr>\n", self::createCheckbox(sprintf("animations[%s][repeat]", $index), $animation->repeat));
        $result .= sprintf("<tr><td>From</td><td><input type=\"text\" name=\"animations[%s][from]\" value=\"%s\"></td></tr>\n", $index, $animation->from);
        $result .= sprintf("<tr><td>To</td><td><input type=\"text\" name=\"animations[%s][to]\" value=\"%s\"></td></tr>\n", $index, $animation->to);
        $result .= sprintf("<tr><td>Axis (x,y,z)</td><td><input type=\"text\" name=\"animations[%s][axis]\" value=\"%s\"></td></tr>\n", $index, $animation->axisString());
        $result .= sprintf("<tr><td>Followed By</td><td><input type=\"text\" name=\"animations[%s][followedBy]\" value=\"%s\"></td></tr>\n", $index, $animation->followedBy);
        $result .= "</table>\n";

        return $result;
    }

    /**
     * Create a screen for a new POI
     *
     * @param string $layerName
     *
     * @return string
     */
    public function createNewPOIScreen($layerName)
    {
        $result = "";
        $result .= sprintf("<form accept-charset=\"utf-8\" action=\"?action=newPOI&layerName=%s\" method=\"POST\">\n", urlencode($layerName));
        $result .= sprintf("<table class=\"newPOI\">\n");
        $result .= sprintf("<tr><td>Dimension</td><td><input type=\"text\" name=\"dimension\" size=\"1\" value=\"3\"></td></tr>\n");
        $result .= sprintf("<caption><button type=\"submit\">Create</button></caption>");
        $result .= "</table>\n";
        $result .= "</form>\n";
        return $result;
    }

    /**
     * Create login screen
     *
     * @return string
     */
    public static function createLoginScreen()
    {
        $result = "<h1><img src=\"http://www.arpoise.com/images/arpoise_logo_rgb-1024.png\" width=64></img>";
        $result .= "ARpoise Directory Login</h1>\n";
        
        /* preserve GET parameters */
        $get = $_GET;
        unset($get["username"]);
        unset($get["password"]);
        unset($get["logout"]);
        $getString = "";
        $first = TRUE;
        foreach ($get as $key => $value) {
            if ($first) {
                $first = FALSE;
                $getString .= "?";
            } else {
                $getString .= "&";
            }
            $getString .= urlencode($key) . "=" . urlencode($value);
        }
        $result .= sprintf("<br><br><form accept-charset=\"utf-8\" method=\"POST\" action=\"%s%s\">\n", $_SERVER["PHP_SELF"], $getString);
        $result .= "<table class=\"login\">\n";
        $result .= "<tr><td>Username</td><td><input type=\"text\" name=\"username\" size=\"15\"></td></tr>\n";
        $result .= "<tr><td>Password</td><td><input type=\"password\" name=\"password\" size=\"15\"></td></tr>\n";
        $result .= "<caption><button type=\"submit\">Log in</button></caption>\n";
        $result .= "</table>\n";
        /* preserve POST */
        foreach ($_POST as $key => $value) {
            switch ($key) {
                case "username":
                case "password":
                case "logout":
                    break;
                default:
                    $result .= sprintf("<input type=\"hidden\" name=\"%s\" value=\"%s\">\n", $key, $value);
                    break;
            }
        }

        $result .= "</form>\n";

        return $result;
    }

    /**
     * Create a screen for migrating (copying) layers
     *
     * @return string
     */
    public static function createMigrationScreen()
    {
        $result = "";
        $layers = DML::getLayers();
        $layers = array_combine($layers, $layers);
        $result .= sprintf("<form accept-charset=\"utf-8\" method=\"POST\" action=\"%s?action=migrate\">\n", $_SERVER["PHP_SELF"]);
        $result .= sprintf("<p>Copy from %s to %s <button type=\"submit\">Copy</button></p>\n", GUI::createSelect("from", $layers), GUI::createSelect("to", $layers));
        $result .= sprintf("<p>Warning: copying contents will overwrite any old data in the destination layer</p>\n");
        $result .= "</form>\n";
        return $result;
    }

    /**
     * Handle POST
     *
     * Checks whether there is something in the POST to handle and calls
     * appropriate methods if there is.
     *
     * @throws Exception When invalid data is passed in POST
     */
    public static function handlePOST()
    {
        $post = $_POST;
        /* not interested in login attempts */
        unset($post["username"]);
        unset($post["password"]);

        if (empty($post)) {
            /* nothing interesting in POST */
            return;
        }
        $action = $_REQUEST["action"];
        switch ($action) {
            case "poi":
                $poi = self::makePOIFromRequest($post);
                DML::savePOI($_REQUEST["layerName"], $poi);
                break;
            case "newPOI":
                $poi = self::makePOIFromRequest($post);
                DML::savePOI($_REQUEST["layerName"], $poi);
                self::redirect("layer", array(
                    "layerName" => $_REQUEST["layerName"]
                ));
                break;
            case "deletePOI":
                DML::deletePOI($_REQUEST["layerName"], $_REQUEST["poiID"]);
                self::redirect("layer", array(
                    "layerName" => $_REQUEST["layerName"]
                ));
                break;
            case "migrate":
                DML::migrateLayers($_REQUEST["from"], $_REQUEST["to"]);
                break;
            case "layer":
                $layerProperties = self::makeLayerPropertiesFromRequest($post);
                $layerProperties->layer = $_REQUEST["layerName"];
                DML::saveLayerProperties($_REQUEST["layerName"], $layerProperties);
                break;
            default:
                throw new Exception(sprintf("No POST handler defined for action %s\n", $action));
        }
    }

    /**
     * Turn request data into a POI object
     *
     * @param array $request
     *            The data from the request
     *            
     * @return POI
     */
    protected static function makePOIFromRequest($request)
    {
        $result = new POI3D();
        foreach ($request as $key => $value) {
            switch ($key) {
                case "dimension":
                    $result->$key = (int) $request[$key];
                    break;
                case "lat":
                case "lon":
                    $result->$key = (float) $request[$key];
                    break;
                case "baseURL":
                    $result->object->$key = (string) $request[$key];
                    break;
                default:
                    $result->$key = (string) $request[$key];
                    break;
            }
        }

        return $result;
    }

    /**
     * Turn request data into a LayarResponse object
     *
     * @param array $request
     *
     * @return LayarResponse
     */
    public static function makeLayerPropertiesFromRequest($request)
    {
        $result = new LayarResponse();
        foreach ($request as $name => $value) {
            switch ($name) {
                case "noPoisMessage":
                    $result->$name = (string) $value;
                    break;
            }
        }
        return $result;
    }

    /**
     * Redirect (HTTP 300) user
     *
     * This method fails if headers are already sent
     *
     * @param string $action
     *            New action to go to
     * @param array $arguments
     *
     * @return void On success, does not return but calls exit()
     */
    protected static function redirect($where, array $arguments = array())
    {
        if (headers_sent()) {
            self::printError("Headers are already sent");
            return;
        }
        $getString = "";
        $getString .= sprintf("?action=%s", urlencode($where));
        foreach ($arguments as $key => $value) {
            $getString .= sprintf("&%s=%s", urlencode($key), urlencode($value));
        }
        if (empty($_SERVER["HTTPS"]) || $_SERVER["HTTPS"] == "off") {
            $scheme = "http";
        } else {
            $scheme = "https";
        }
        $location = sprintf("%s://%s%s%s", $scheme, $_SERVER["HTTP_HOST"], $_SERVER["PHP_SELF"], $getString);
        header("Location: " . $location);
        exit();
    }
}
