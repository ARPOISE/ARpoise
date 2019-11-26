![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise-Directory Back End

## Overview
This is an ARpoise internal version of PorPOISe used for the placing of artist's layers in the ARpoise directory.

The ARpoise directory has to be maintaned on www.arpoise.com. It contains a single "meta" layer, the Arpoise-Directory.
The "**POI**s" of this layer define the name, url, attributes, and location of all the layers visible in ARpoise.
Content creators who want to use ARpoise or AR-vos to deliver their 3D content via the apps, do not need to run this service.

## Functionality
The ARpoise directory front end contacts this web service with the client's location as parameter.

The service returns all definitions of layers that are within the range of the client's location.

If there are no layers within range of the client's location, an empty list of layer definitions is returned.

In the ARpoise-Directory back end the placing of the layers can be performed in a Google-maps-based click-and-drag web interface.

## Documentation:
## ARpoise-Directory Back End Layer Configuration
### Screen Shot:
![DirectoryImg1](/images/Directory1.png)
### Explanation:
The following attributes of a layer can be edited.
* **Layer Name**: The name of the layer.
* **Lat/Lon**: Allows to change the latitude and longitude of the layer. The layer's location can also be changed by dragging the layer's marker in the placement map.
* **Is visible**: This combo box defines whether the layer is visible.
* **Porpoise URL**: The URL of the ARpoise-PorPOISe Back End serving the layer.
* **Layer Title**: The title of the layer shown in the client application when a list of layers is displayed for selection.
* **Line 2**: The second line of text shown in the client application when a list of layers is displayed for selection.
* **Line 3**: The third line of text shown in the client application when a list of layers is displayed for selection.
* **Icon name**: The name of the icon shown in the client application when a list of layers is displayed for selection. The icon has to be bundled into the ARpoise global Unity asset bundle of icons.
* **Save** button: Saves the layers's attributes to the ARpoise Directory's xml file.

## Original Documentation

===PorPOISe for Layar===
Portable Point-of-Interest Server for Layar

===Administrative contact===
Jens de Smit, jens@layar.com

===Introduction===
PorPOISe is a server for Layar clients. It converts your data sets of POIs
(Points of Interest) into responses to the Layar client. Things like JSON
formatting and distance calculation are all done for you. PorPOISe supports
XML files as data stores.

===Getting started===
Read INSTALL for installation instructions. Once properly installed, you can
use the dashboard to create your first POIs. The interface is pretty spartan
but this will generate correct output format. Study the format if you intend to
generate your own XML files.

From here on you're on your own. Build a better interface for the dashboard or
expand PorPOISe to have more features if you need more.

===History===
PorPOISe originated at SURFnet in 2009 as a spin-off from a small layer-
building experiment. Over 2010 functionality expanded and feature support
grew with Layar's feature support. In 2011, PorPOISe's primary author moved
from SURFnet to Layar and took the project with him.

===More information===
  * http://www.surfnet.nl/en The home of the creator of PorPOISe
  * http://teknograd.wordpress.com/2009/10/19/augmented-reality-create-your-own-layar-layer/ An explanation on how to build the most minimal of Layar servers. Very useful to get started
  * http://layar.com/ is, of course, the reason this project exists
