![ARpoise Logo](/images/arpoise_logo_rgb-128.png)  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  ![AR-vos Logo](/images/arvos_logo_rgb-weiss128.png)
# Release Notes for ARpoise and AR-vos -Mobile- Apps

## Version 20241029 - October 2024
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 2.0.9 (2024102900)** and **iOS ARpoise 2.0.9 (20241029)**
- - Fixed [Bug #43](https://github.com/ARPOISE/ARpoise/issues/43). Make device buzz as porpoise level animation..

## Version 20241014 - October 2024
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 2.0.8 (2024101400)** and **iOS ARpoise 2.0.8 (20241014)**
- - Fixed [Bug #42](https://github.com/ARPOISE/ARpoise/issues/42). Pausing the app stops the shared event layer from working.
  - 
## Version 20240915 - September 2024
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 2.0.7 (2024091500)** and **iOS ARpoise 2.0.7 (20240915)**
- - Added [Feature #41](https://github.com/ARPOISE/ARpoise/issues/41). Rendering should use the Color Space 'Linear'.
- - Added [Feature #40](https://github.com/ARPOISE/ARpoise/issues/40). Lights need to be controllable.
- - Added [Feature #39](https://github.com/ARPOISE/ARpoise/issues/39). Shared event animations should be able to differentiate between the ARpoise instance that started the animations and others.

## Version 20240303 - March 2024
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 2.0.6 (2024030300)** and **iOS ARpoise 2.0.6 (20240303)**
- - Fixed [Bug #37](https://github.com/ARPOISE/ARpoise/issues/37). Made sure 'What You Sow' objects are destroyed on layer change.
  
## Version 20240224 - February 2024
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 2.0.5 (2024022400)** and **iOS ARpoise 2.0.5 (20240224)**
- - Fixed [Bug #35](https://github.com/ARPOISE/ARpoise/issues/35). Made sure synchronisation works without a label.

## Version 20240207 - February 2024
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 2.0.4 (2024020700)** and **iOS ARpoise 2.0.4 (20240207)**
- - Internal update in preparation of showing Vera Plastica as ARpoise based art work at Museum Ludwing in Budapest, 2024-02-09.

## Version 20231029 - October 2023
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 2.0.2 (2023102900)** and **iOS ARpoise 2.0.2 (20231029)**
- - Added feature **ARpoise is now based on Unity 2022.3.10 LTS**, see [#29](https://github.com/ARPOISE/ARpoise/issues/29).
  - Added feature **Share events via a multi-user back end works for long messages**, see [#34](https://github.com/ARPOISE/ARpoise/issues/34).

## Version 20230901 - September 2023
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 2.0.1 (2023090100)** and **iOS ARpoise 2.0.1 (20230901)**
- - Added feature **ARpoise is now based on Unity 2022.3.6 LTS**, see [#29](https://github.com/ARPOISE/ARpoise/issues/29).
  - Added feature **Share events via a multi-user back end**, see [#27](https://github.com/ARPOISE/ARpoise/issues/27).
  - Added feature **Event type inMinutes**, see [#28](https://github.com/ARPOISE/ARpoise/issues/28).
  - Added feature **CameraChild-poi**, see [#30](https://github.com/ARPOISE/ARpoise/issues/30).
  - Added feature **AllowTakeScreenshot**, see [#31](https://github.com/ARPOISE/ARpoise/issues/31).
  - Added feature **'Relative location' relative to starting direction of device**, see [#32](https://github.com/ARPOISE/ARpoise/issues/32).
  - Added feature **Animation type 'Volume'**, see [#33](https://github.com/ARPOISE/ARpoise/issues/33).
    
## Version 20220131 - January 2022
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 1.13 (202200131)** and **iOS ARpoise 1.14 (20220131)**
- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **Android AR-vos 1.11 (20220131)** and **iOS AR-vos 1.8 (20220131)**
  - Fixed [Bug #23](https://github.com/ARPOISE/ARpoise/issues/23). Made sure animations run currectly with recorder.
  - Added feature **Time synchronization between more than one device**, see [#22](https://github.com/ARPOISE/ARpoise/issues/22).

## Version 20210912 - September 2021
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 1.12 (20210912)** and **iOS ARpoise 1.13 (20210912)**
- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **Android AR-vos 1.10 (20210912)** and **iOS AR-vos 1.7 (20210912)**
  - Fixed [Bug #21](https://github.com/ARPOISE/ARpoise/issues/21). Made sure SetActive/SetInActive animations can be triggered for seperate pois.
  - Added feature **Loading inner layers of inner layers**, see [#14](https://github.com/ARPOISE/ARpoise/issues/14).

## Version 20210814 - August 2021
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 1.11 (20210814)** and **iOS ARpoise 1.12 (20210814)**
- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **Android AR-vos 1.9 (20210814)** and **iOS AR-vos 1.6 (20210814)**
  - Fixed [Bug #17](https://github.com/ARPOISE/ARpoise/issues/17). Made sure non-repeating, persistent animations are ended correctly and do not end in a random state.
  - Added feature [Position Update Interval](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md#optional-layer-parameters), see [#20](https://github.com/ARPOISE/ARpoise/issues/20).
  - Added feature [Animating the Same Values with two Animations](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md#animating-the-same-values-with-two-animations), see [#19](https://github.com/ARPOISE/ARpoise/issues/19).
  - Added feature [Setting the Game Object State](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md#setting-the-game-object-state), see [#18](https://github.com/ARPOISE/ARpoise/issues/18).

## Version 20210408 - April 2021
- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 1.10 (20210408)** and **iOS ARpoise 1.11 (20210408)** 
- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **Android AR-vos 1.8 (20210408)** and **iOS AR-vos 1.5 (20210408)**
  - Fixed [Bug #12](https://github.com/ARPOISE/ARpoise/issues/12).
  - Fixed [Bug #11](https://github.com/ARPOISE/ARpoise/issues/11).
  
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
  - Fixed [Bug #10](https://github.com/ARPOISE/ARpoise/issues/10).
  
## Version 20200522 - May 2020
- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **iOS AR-vos 1.2 (20200522), May 28, 2020 at 10:45 PM**
  - Fixed [Bug #8](https://github.com/ARPOISE/ARpoise/issues/8).
  - iOS AR-vos now uses Unity 2018.4.23 LTS as build environment.
  - Added the two new animation types 'Fade' and 'Destroy'.

- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **iOS ARpoise 1.8 (20200522), May 28, 2020 at 10:13 PM**
  - Added the two new animation types 'Fade' and 'Destroy'.

- ![AR-vos Logo](/images/arvos_logo_rgb-weiss32.png) **Android AR-vos 1.5 (200522), May 23, 2020 at 4:10 PM**
  - Fixed [Bug #8](https://github.com/ARPOISE/ARpoise/issues/8).
  - Android AR-vos now uses Unity 2018.4.23 LTS as build environment.
  - Added the two new animation types 'Fade' and 'Destroy'. 

- ![ARpoise Logo](/images/arpoise_logo_rgb-32.png) **Android ARpoise 1.7 (200522), May 23, 2020 at 1:46 PM**
  - Android ARpoise now uses Unity 2018.4.23 LTS as build environment.
  - Added the two new animation types 'Fade' and 'Destroy'.
