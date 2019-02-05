# ARpoise Directory Backend.

## Overview
This is a version of porpoise used for the placing of the pois of your layers.

The ARpoise directory has to be maintaned on www.arpoise.com. It contains a single meta layer, the Arpoise-Directory.
The "**POI**s" of this layer define the name, url and location of layers visible in ARpoise.

## Functionality
The ARpoise directory frontend contacts this web service with the client location as parameter.

The service returns all definitions of layers that are within 1500 meters of the client location.

If there are no layers within range of the client location, an empty list of layer definitions is returned.

The placing of the layers can be performed in a Google-maps-based click-and-drag interface.

## Original Documentation
[see]{README}.
