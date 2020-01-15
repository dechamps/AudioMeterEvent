# AudioMeterEvent
*Brought to you by [Etienne Dechamps][] - [GitHub][]*

**If you are looking for executables, see the [GitHub releases page][].**

This is a small utility that can monitor the sound level on a Windows audio
device and react when it crosses a threshold by sending HTTP requests.

Currently it is mainly designed to interact with the [OpenHAB][] [REST API][],
allowing you to switch an [Item][] on and off based on audio device levels. My
personal use case is to command "smart" wall plugs to automatically turn on my
active speakers when sound is playing on my PC. That said, the utility should be
flexible enough to adapt to other use cases.

## Features

 - Sends HTTP requests when detecting sound on a Windows audio device
 - Keeps sending requests at regular intervals for a period of time
 - Sends a special request when…
   - …no sound has been detected for some time
   - …the computer is going into standby
   - …AudioMeterEvent is shutting down
 - Sound level threshold and minimum sound duration can be customized
 - HTTP URI, POST payload and Content-Type can be customized
 - Defaults work well with OpenHAB

## Requirements

 - Windows Vista or later
 - .NET Framework 4.8

## How to use

Run `AudioMeterEvent.exe --help` to see a list of options. The most important
one is `--audio-device-id` to specify which device AudioMeterEvent should
monitor. Use `AudioDevicesList.exe` to figure out what your device ID is.

If you don't specify any URI to use for HTTP requests, all AudioMeterEvent does
is output messages to the console. Use the `--event-uri` option to specify an
address to send requests to.

### How to interface with OpenHAB

AudioMeterEvent is designed to send an ON request at regular intervals after
sound is playing, to "keep alive" an OpenHAB item that would otherwise be turned
off by an [Expire][] trigger. This approach is the most robust because it
ensures the item will eventually be turned off in case of crashes or
connectivity issues, or any other case where AudioMeterEvent did not get the
chance to send an explicit OFF request.

Example items configuration:

```
Switch PC_Sounding {expire="1m,state=OFF"}
```

Example AudioMeterEvent command line:

```
AudioMeterEvent.exe --audio-device-id={…}.{…} --event-uri=http://openhab.example/rest/items/PC_Sounding
```

If you have a secure OpenHAB setup using an HTTP username and password, you can
use the `--http-username` and `--http-password-file` options to provide them.

## How to install as a service

If you plan on using AudioMeterEvent continuously, it is possible (and
recommended) to run it as a standard Windows Service.

The service can be installed using `InstallUtil.exe` which can be found in a
subdirectory of `C:\Windows\Microsoft.NET\Framework`. For example:

```
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe --audio-device-id={…,…} … "C:\Path\To\AudioMeterEvent.exe"
```

**Note:** `InstallUtil.exe` is opinionated about command line parsing -
`--option=value` will work, but `--option value` won't.

**Note:** for security reasons, the service runs with limited privileges. Make
sure that your file permissions are set up so that the service can access the
executable (e.g. move the files to Program Files first). The same applies to 
`--http-password-file`. The service runs under a dedicated
`NT SERVICE\AudioMeterEvent` user account.

You can then find and start the service under the name "AudioMeterEvent" in the
Windows services list.

Log messages from the service will appear in the Windows Event Viewer
(Application).

To uninstall the service, use `InstallUtil.exe` again with the `/u` switch.

## Limitations and caveats

This is an early release. Expect bugs. [Feedback is welcome][feedback].

I have only tested monitoring output devices. Monitoring input devices (e.g.
microphones) should work, but I have not tried.

AudioMeterEvent works by continuously polling the sound level on the audio
device. This necessarily means it uses a tiny bit of CPU in the background. In
practice, with the default settings the CPU usage seems to be less than 0.5% of
one logical CPU.

## Developer information

AudioMeterEvent is written in C#. It should build out-of-the-box in Visual
Studio 2019.

**Note:** in addition to the C# components, some Visual Studio C++ components
might be required in order to build the WASAPI shim layers.

[Etienne Dechamps]: mailto:etienne@edechamps.fr
[GitHub]: https://github.com/dechamps/AudioMeterEvent
[GitHub releases page]: https://github.com/dechamps/AudioMeterEvent/releases
[OpenHAB]: https://www.openhab.org/
[REST API]: https://www.openhab.org/docs/configuration/restdocs.html
[Item]: https://www.openhab.org/docs/configuration/items.html
[Expire]: https://www.openhab.org/addons/bindings/expire1/
[feedback]: https://github.com/dechamps/AudioMeterEvent/issues