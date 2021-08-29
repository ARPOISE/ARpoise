![ARpoise Logo](/images/arpoise_logo_rgb-128.png)  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  ![AR-vos Logo](/images/arvos_logo_rgb-weiss128.png)
# Release Notes for ARpoise and AR-vos -Mobile- Apps

## Version 20210814 - August 2021
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 1.11 (20210814)** and **iOS ARpoise 1.12 (20210814)**
- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **Android AR-vos 1.9 (20210814)** and **iOS AR-vos 1.6 (20210814)**
  - Fixed Bug #17. Made sure non-repeating, persistent animations are ended correctly and do not end in a random state.
  - Added feature [position update interval] (https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md#optional-layer-parameters). The new layer action PositionUpdateInterval allows to restrict the time intervals after which the device position is updated into the app.
  - By default, each animation on a poi creates a new wrapper object for the animation. In order to allow to have two animations to change the same wrapper object, animations whose name starts with the same sequence of characters before the first '/' in the name, e.g. 'scaleObject/up' and 'scaleObject/down', are animating the same transform.
  - The game object of a poi can now be set to active or in-active. If an animation has a name of SetActive or SetInActive and the animation is started the game object of the POI is set to active/inactive. If a follow animation of an animation is called SetActive or SetInActive and the animation ends, the game object of the POI is set to active/inactive.

## Version 20210408 - April 2021
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 1.10 (20210408)** and **iOS ARpoise 1.11 (20210408)** 
- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **Android AR-vos 1.8 (20210408)** and **iOS AR-vos 1.5 (20210408)**
  - Fixed Bug #12.
  - Fixed Bug #11.
  
## Version 20210124 - January 2021
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 1.9 (20210124)** and **iOS ARpoise 1.10 (20210124)**
  - Added animation feature [Opening a WEB Page](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md#opening-a-web-page)
  
- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **Android AR-vos 1.7 (20210124)** and **iOS AR-vos 1.4 (20210124)**
  - Added animation feature [Opening a WEB Page](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md#opening-a-web-page)
  - Added [Image - Tracking Timeout](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md#explanation-2)
  - Added [SLAM - Maximum Count](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md#explanation-2)
  
## Version 20200608 - June 2020
- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **iOS AR-vos 1.3 (20200608), Jun 8, 2020 at 3:47 PM**
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **iOS ARpoise 1.9 (20200608), Jun 8, 2020 at 3:57 PM**
- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **Android AR-vos 1.6 (200608), Jun 08, 2020**
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 1.8 (200608), Jun 08, 2020**
  - Fixed Bug #10. Destroy of POIs with absolute GPS coordinates works now.
  
## Version 20200522 - May 2020
- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **iOS AR-vos 1.2 (20200522), May 28, 2020 at 10:45 PM**
  - Fixed Bug #8.
  - iOS AR-vos now uses Unity 2018.4.23 LTS as build environment.
  - Added the two new animation types 'Fade' and 'Destroy'.

- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **iOS ARpoise 1.8 (20200522), May 28, 2020 at 10:13 PM**
  - Added the two new animation types 'Fade' and 'Destroy'.

- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **Android AR-vos 1.5 (200522), May 23, 2020 at 4:10 PM**
  - Fixed Bug #8.
  - Android AR-vos now uses Unity 2018.4.23 LTS as build environment.
  - Added the two new animation types 'Fade' and 'Destroy'. 

- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 1.7 (200522), May 23, 2020 at 1:46 PM**
  - Android ARpoise now uses Unity 2018.4.23 LTS as build environment.
  - Added the two new animation types 'Fade' and 'Destroy'.
