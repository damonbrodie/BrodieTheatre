# Brodie Home Theatre

This project was developed to meet the needs for my 
[home theatre](http://www.avsforum.com/forum/19-dedicated-theater-design-construction/1033681-brodie-home-theatre-build-thread-2.html#post46048545) 
where I wanted very selective home automation to make the experience as seemless as possible.

The project incorporates the following technologies: 
 - Insteon light dimmers that provide a programmable lighting interface with a standard wall dimmer formfactor
 - Insteon motion sensor to detect room occupancy
 - Harmony Remote controls for media playback control, but also allow programmatic automation of certain functions
 - Serial port integration to the Panasonic projector
 - Connection to the JSON port on the Kodi HTPC.  This is used to determine when media playback 
 pauses/plays/stops.

This project forms an automation application that runs on the HTPC in the theatre that listens 
to the events and uses them as decision points to signal events.

## Use Cases
 
This project aims to provide very specific automation to both enhance the *cool* factor of the 
room as well as make it easier to use the theatre.  The following high level use cases are 
supported by this application:
- Upon entering the room using the Insteon motion and door sensors, bring up the room lights with the Insteon 
Dimmer.
- Listen for Harmony Hub activity changes to power on the AV Amplifier. Use this to send serial commands to the
projector and power it on.  Once the projector is powered up the lights are automatically dimmed to a comfortable 
lighting preset.
- Using the Kodi JSON feed to stay current with playback status (pause/stop/start), alter the lighting, dimming
it further during playback, raising when playback is paused or stopped.  The Aspect Ratio of the movie is also 
sent over this interface so that the projector Zoom level is set appropriately.
- Listen for custom button presses from the Harmony remote: the user.  Lighting changes, projector aspect ratio 
changes can be initiated by the remote control.  The app captures these  and actions them.
- When the user powers off the system via the remote control (shuts down the Harmony Activity), the app will 
power down the AV equipment and the lighting is brought up to full.
- Automatically shutdown the Amp and Projector if Kodi is Stopped for the set time period
