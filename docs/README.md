<img align="right" src="img/gemstone-wide-600.png" alt="gemstone logo">

<br /><br /><br /><br />

# PhasorProtocols
### GPA Gemstone Library

The Gemstone PhasorProtocols Library organizes all Gemstone functionality related to synchrophasor protocols, e.g., IEEE C37.118.

[![GitHub license](https://img.shields.io/github/license/gemstone/phasor-protocols?color=4CC61E)](https://github.com/gemstone/phasor-protocols/blob/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/u6qs98vlw8abidrv?svg=true)](https://ci.appveyor.com/project/ritchiecarroll/phasor-protocols)
![CodeQL](https://github.com/gemstone/phasor-protocols/workflows/CodeQL/badge.svg)
[![NuGet](https://buildstats.info/nuget/Gemstone.PhasorProtocols)](https://www.nuget.org/packages/Gemstone.PhasorProtocols#readme-body-tab)

This library includes helpful phasor protocol classes like the following:

* [Class](https://gemstone.github.io/phasor-protocols/help/html/T_Gemstone_phasor-protocols_MultiProtocolFrameParser.htm):
  * Represents a protocol independent frame parser that takes all protocol frame parsing implementations and reduces them to a single simple-to-use class exposing all data through abstract interfaces (e.g., IConfigurationFrame, IDataFrame, etc.) and also implements a variety of transport options (e.g., TCP, UDP, Serial, etc.), hides the complexities of this connectivity and internally pushes all data received from the selected transport protocol to the selected phasor parsing protocol.

Among others.

### Documentation
[Full Library Documentation](https://gemstone.github.io/phasor-protocols/help)
