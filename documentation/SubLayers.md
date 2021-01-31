![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# *A*ugmented *R*eality *p*oint *o*f *i*nterest *s*ervice *e*nvironment
# -- Using Sub-Layers --
## Overview
This a step by step tutorial detailing an example of how sub layers can be used with POIs in **ARpoise** or **AR-vos**.

## Layer Definition
The layer Example-SlamBoxes of the **AR-vos** examples is defined as follows, it contains only a single POI, called BellCube.
### Image - Layer Example-SlamBoxes:
![SubLayers-LayerDefinition](/documentation/images/SubLayers-LayerDefinition.png)

## POI Definition
The POI BellCube is defined as follows, it contains only a single prefab, called BellCube and with it's parameter **Layer name** it references the sub-layer **Slam-Example**.
### Image - POI BellCube:
![SubLayers-PoiDefinition](/documentation/images/SubLayers-PoiDefinition.png)

## Sub-Layer Definition
The sub-layer Slam-Example is defined as follows, it contains four simple cube prefabs.
### Image - Sub-Layer Slam-Example:
![SubLayers-SubLayerDefinition](/documentation/images/SubLayers-SubLayerDefinition.png)

## Sub-Layer POI Definition
Each of the POIs in the sublayer is a simple POI, defined as in the example.
### Image - Sub-Layer POI StripesCube:
![SubLayers-SubLayerPoiDefinition](/documentation/images/SubLayers-SubLayerPoiDefinition.png)
