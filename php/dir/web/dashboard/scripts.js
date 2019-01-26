/*
 * PorPOISe
 * Copyright 2009 SURFnet BV
 * Thanks to PLKT for the Google maps widget (http://plkt.free.fr)
 * Released under a permissive license (see LICENSE)
 */

var GUI = {
	getMaxInputIndexInTables : function(tables) {
		var maxIndex = 0;

		for ( var i = 0; i < tables.length; i++) {
			var inputs = tables[i].select("input");
			if (inputs.length == 0) {
				/* weird, page must be corrupt */
				return -1;
			}
			var inputName = inputs[0].name;
			var indexWithBrackets = inputName.match(/\[.+\]/);
			if (indexWithBrackets.length == 0) {
				/* again, invalid */
				return;
			}
			var index = parseInt(indexWithBrackets[0].substr(1,
					indexWithBrackets[0].length - 2));
			if (index > maxIndex) {
				maxIndex = index;
			}
		}
		return maxIndex;
	}

	,
	addAction : function(source, actionTables, layerAction) {
		var maxIndex = this.getMaxInputIndexInTables(actionTables);
		if (maxIndex < 0) {
			return;
		}

		var newIndex = maxIndex + 1;

		var newRow = document.createElement("tr");
		var td = document.createElement("td");
		td.insert("Action<br><button type=\"button\" onclick=\"GUI.remove"
				+ (layerAction ? "Layer" : "POI") + "Action(" + newIndex
				+ ")\">Remove</button>");
		newRow.appendChild(td);
		td = document.createElement("td");
		newRow.appendChild(td);
		new Ajax.Updater({
			success : td
		}, "gui.php", {
			parameters : {
				action : "newAction",
				index : newIndex,
				layerAction : layerAction
			},
			insertion : "bottom"
		});
		var sourceRow = source.up("tr");
		sourceRow.insert({
			before : newRow
		});
	}

	,
	addPOIAction : function(source) {
		var poiActionTables = document.body.select("table.poi table.action");
		this.addAction(source, poiActionTables, false);
	}

	,
	addLayerAction : function(source) {
		var layerActionTables = document.body
				.select("table.layer table.action");
		this.addAction(source, layerActionTables, true);
	}

	,
	removePOIAction : function(indexToRemove) {
		var poiActionTables = document.body.select("table.poi table.action");
		this.removeAction(indexToRemove, poiActionTables);
	}

	,
	removeLayerAction : function(indexToRemove) {
		var layerActionTables = document.body
				.select("table.layer table.action");
		this.removeAction(indexToRemove, layerActionTables);
	}

	,
	removeAction : function(indexToRemove, actionTables) {
		toRemove = this.findInputInTables(indexToRemove, actionTables);
		if (toRemove) {
			toRemove.up("tr").remove();
		}
	}

	,
	findInputInTables : function(indexToFind, tables) {
		for ( var i = 0; i < tables.length; i++) {
			var inputs = tables[i].select("input");
			if (inputs.length == 0) {
				/* weird, page must be corrupt */
				return;
			}
			var inputName = inputs[0].name;
			var indexWithBrackets = inputName.match(/\[.+\]/);
			if (indexWithBrackets.length == 0) {
				/* again, invalid */
				return;
			}
			var index = parseInt(indexWithBrackets[0].substr(1,
					indexWithBrackets[0].length - 2));
			if (index == indexToFind) {
				return tables[i];
			}
		}
	}

	,
	addAnimation : function(source, animationTables, layerAnimation) {
		var maxIndex = this.getMaxInputIndexInTables(animationTables);
		if (maxIndex < 0) {
			return;
		}

		var newIndex = maxIndex + 1;

		var newRow = document.createElement("tr");
		var td = document.createElement("td");
		td.insert("Animation<br><button type=\"button\" onclick=\"GUI.remove"
				+ (layerAnimation ? "Layer" : "POI") + "Animation(" + newIndex
				+ ")\">Remove</button>");
		newRow.appendChild(td);
		td = document.createElement("td");
		newRow.appendChild(td);
		new Ajax.Updater({
			success : td
		}, "gui.php", {
			parameters : {
				action : "newAnimation",
				index : newIndex
			},
			insertion : "bottom"
		});
		var sourceRow = source.up("tr");
		sourceRow.insert({
			before : newRow
		});
	}

	,
	addPOIAnimation : function(source) {
		var poiAnimationTables = document.body
				.select("table.poi table.animation");
		this.addAnimation(source, poiAnimationTables, false);
	}

	,
	addLayerAnimation : function(source) {
		var layerAnimationTables = document.body
				.select("table.layer table.animation");
		this.addAnimation(source, layerAnimationTables, true);
	}

	,
	removePOIAnimation : function(indexToRemove) {
		var poiAnimationTables = document.body
				.select("table.poi table.animation");
		this.removeAnimation(indexToRemove, poiAnimationTables);
	}

	,
	removeLayerAnimation : function(indexToRemove) {
		var layerAnimationTables = document.body
				.select("table.layer table.animation");
		this.removeAnimation(indexToRemove, layerAnimationTables);
	}

	,
	removeAnimation : function(indexToRemove, animationTables) {
		toRemove = this.findInputInTables(indexToRemove, animationTables);
		if (toRemove) {
			toRemove.up("tr").remove();
		}
	}

};

/*
 * copyright (c) 2009 Google inc.
 * 
 * You are free to copy and use this sample. License can be found here:
 * http://code.google.com/apis/ajaxsearch/faq/#license
 */

/*
 * ------------------------------------------------------- --- Porpoise POI
 * coords selection unsing Google Map ---
 * ------------------------------------------------------- ---------- PLKT
 * http://plkt.free.fr -------------------
 * -------------------------------------------------------
 * 
 * \[^-^]/ !
 * 
 */

// ////////////////////////////////////// VARS
// Objects
var map;
var geocoder;

// Nodes
var addressInput;
var mapPopin;
var mapDiv;

// ////////////////////////////////////// FUNCTIONS

// Find place on the map
function geocode() {
	geocoder.geocode({
		'address' : addressInput.value,
		'partialmatch' : true
	}, function(results, status) {
		if (status == 'OK' && results.length > 0) {
			map.fitBounds(results[0].geometry.viewport);
		} else {
			alert("Geocode was not successful for the following reason: "
					+ status);
		}
	});
}



// Onload function
function initialize() {

	// ////////////////////////////////////////////////
	// Here : porpoising the script
	// Creating "map popin" nodes ( Non Intrusive :)
	
//	mapPopinLink = document.createElement('input');	// was popup is always visible TT 2016-05-07
//	mapPopinLink.type = "button";
//	mapPopinLink.value = "Find on Google Map";
//	mapPopinLink.onclick = function() {
//		mapPopin.style.visibility = "visible";
//	};
	mapPopin = document.createElement('div');
	mapPopin.style.visibility = "visible"; // was hidden is visible TT 2016-05-07
	mapPopin.style.position = "fixed";
//	mapPopin.style.position = "absolute";
	mapPopin.style.background = "#FFFFFF";
	mapPopin.style.border = "solid 1px #000000";
	mapPopin.style.padding = "1em";
	mapPopin.style.marginTop = "0em";	// was 1em is 0
	mapPopin.style.marginLeft = "22em";	// to left of table TT 2016-05-07
	
	niceDisplay = document.createElement('p');
	addressInput = document.createElement('input');
	addressInput.type = "text";
	addressInput.style.width = "325px";
	niceDisplay.appendChild(addressInput);
	findPlaceButton = document.createElement('input');
	findPlaceButton.type = "button";
	findPlaceButton.value = "Find Place";
	findPlaceButton.onclick = function() {
		geocode();
	};
	niceDisplay.appendChild(findPlaceButton);

//	Close button not needed, as map is always visible
//	CloseButton = document.createElement('input');
//	CloseButton.type = "button";
//	CloseButton.value = "CLOSE";
//	CloseButton.style.marginLeft = "1.5em";
//	CloseButton.onclick = function() {
//		mapPopin.style.visibility = "hidden";
//	};
//	niceDisplay.appendChild(CloseButton);
	
	mapPopin.appendChild(niceDisplay);
	mapDiv = document.createElement('div');
	mapDiv.style.display = "block";
	mapDiv.style.width = "400px";
	mapDiv.style.height = "400px";
	mapDiv.innerHTML = "ok";
	
	mapPopin.appendChild(mapDiv);

	// two cases: map on layer page or on POI page
	var page = "poi";
	var pages = document.getElementsByName('page');
	if (pages.length > 0) {
		page = pages[0].value;
		
	}
	var showLayer = document.getElementsByName('doShowLayer');
	if (showLayer.length > 0) {
		Redirect(showLayer[0].value);		
	}

	porpoiselnginputs = document.getElementsByName('lon'); // looks for lon field on POI page and appends google map after it
	if (porpoiselnginputs.length > 0 && page == "poi") {
		porpoiselnginputs[0].parentNode.appendChild(mapPopin);		
	}
	else {
		porpoiseResponseMsg = document.getElementsByName('copylayers'); // looks for copylayers field on layer page and appends google map after it
		if (porpoiseResponseMsg.length > 0) {
			porpoiseResponseMsg[0].parentNode.appendChild(mapPopin);
		}
		else {
			mapDiv.innerHTML = "did not find copylayers";
			return;
		}
	}
	
	
	
	// ////////////////////////////////////////////////
	// GET input FIELDS 

	// default position @ainmillerstrasse if not set
	var currentLat = "48.1586487";
	var currentLon = "11.5787151";

	var zooming = 18;

//	// Use lat/lon on page, if already set - this works only for POI page, which has only one lat/lon
//
//	var currentLatElements = document.getElementsByName('lat');
//	if (currentLatElements.length > 0) {
//		currentLat = currentLatElements[0].value;
//	}
//	var currentLonElements = document.getElementsByName('lon');
//	if (currentLonElements.length > 0) {
//		currentLon = currentLonElements[0].value;
//	}

	// ////////////////////////////////////////////////
	// Set map on POI page

	if (page == "poi") {

		// Use lat/lon on page, if already set
		var currentLatElements = document.getElementsByName('lat');
		if (currentLatElements.length > 0) {
			currentLat = currentLatElements[0].value;
		}
		
		var currentLonElements = document.getElementsByName('lon');
		if (currentLonElements.length > 0) {
			currentLon = currentLonElements[0].value;
		}

		// create new map centered around single POI
		map = new google.maps.Map(mapDiv, {
			center : new google.maps.LatLng(currentLat, currentLon),
			zoom : zooming,
			mapTypeId : google.maps.MapTypeId.HYBRID
		});
		map.setTilt(0); // disable 45� tilt - added TT 5 may 2016
		geocoder = new google.maps.Geocoder();
		
		var poiTitle = document.getElementsByName('title');	// gets title of POI for marker TT 5 May 2016
		fillMap(map, poiTitle[0].value, 1, currentLat, currentLon, true);		
	}
	
	if (page == "layer") {
		var latElement = document.getElementById('markerLat1');
		var lonElement = document.getElementById('markerLon1');
		
		if (latElement != null && lonElement != null) {
			
			map = new google.maps.Map(mapDiv, {
				center : new google.maps.LatLng(latElement.value, lonElement.value),
				zoom : zooming,
				mapTypeId : google.maps.MapTypeId.HYBRID
			});
			map.setTilt(0); // disable 45� tilt - added TT 5 may 2016
			geocoder = new google.maps.Geocoder();
			
			// get info for each POI of the layer from the hidden POI table created in gui.class.php
			for (var index = 1; true; index++) {
				latElement = document.getElementById('markerLat' + index);
				lonElement = document.getElementById('markerLon' + index);

				if (latElement == null || lonElement == null) {		// break if there are no POIs
					break;
				}
				fillMap(map, latElement.name, index, latElement.value, lonElement.value, false);		
			}					
		}
	}
}

function Redirect(url) {
    window.location=url;
 }

// PG 9 May 2016
function fillMap(map, title, index, currentLat, currentLon, addClick) {
		
	var marker = new google.maps.Marker({
		position : new google.maps.LatLng(currentLat, currentLon),
		map : map,
		draggable : true,			// added TT 5 May 2016
		title : title	// now shows title of POI TT 5 May 2016 (useful if I can get this to work for layer)
	});

	// Add event to the map ( when user clicks on the map ... )
	var infoWindow = new google.maps.InfoWindow();

	if (addClick) {

		google.maps.event.addListener(map, 'click', function(event) {

			var latLng = event.latLng;
			var lat = latLng.lat(); // latitude of click
			var lng = latLng.lng(); // longitude of click

			// pop-up window with lat/lon of click, positioned at clicked
			// location
			var html = '<strong>Lat:</strong><br >' + lat
					+ '<br ><strong>Long:</strong><br >' + lng;
			infoWindow.setContent(html);
			infoWindow.setPosition(latLng);
			infoWindow.open(map);

			// overwrite existing val of lat/lon with click position
			porpoiselatinputs = document.getElementById('lat' + index);
			porpoiselatinputs.value = lat;
			porpoiselnginputs = document.getElementById('lon' + index);
			porpoiselnginputs.value = lng;

		});
	}
	
	google.maps.event.addListener(marker, 'drag', function() {	// TT 5 May 2016

		var latLng = marker.getPosition();	// get current lat/lon of marker
		var lat = latLng.lat(); // latitude
		var lng = latLng.lng(); // longitude
		
		// pop-up window with current lat/lon of marker
//		var html = '<strong>Lat:</strong><br >' + lat
//				+ '<br ><strong>Long:</strong><br >' + lng;
//		infoWindow.setContent(html);
// 		infoWindow.setPosition(latLng);
//		infoWindow.open(map);

		// overwrite existing val of lat/lon with current position during drag
		porpoiselatinputs = document.getElementById('lat' + index);
		porpoiselatinputs.value = lat;
		porpoiselnginputs = document.getElementById('lon' + index);
		porpoiselnginputs.value = lng;

	});
}

// ONLOAD

google.maps.event.addDomListener(window, 'load', initialize);
