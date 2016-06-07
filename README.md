# SharpMap.Widgets
Build responsive map applications for web and desktop

[Web-Sample](http://80.146.239.139/SharpMap.Widgets/)

Windows-Sample
![Windows-Sample](/Doc/SharpMap.Win.png)

[SharpMap](https://sharpmap.codeplex.com/) is a powerful mapping library that supports many standards and formats.
But the interactive Web- and Windows-Widgets of the SharpMap project are not up to date.
SharpMap.Widgets shows how to combine the SharpMap renderer with map widgets 
like [Leaflet](http://leafletjs.com/) or [Ptv xServer.NET](http://xserver.ptvgroup.com/en-uk/cookbook/explore/xserver-net-demo-center/).
So it combines the power of SharpMap with the Look&Feel of modern widgets.

### What the project shows
* Efficiently render large data sets with SharpMap for "slippy" map widgets
+ Implementing SharpMap interfaces to render your custom data source
+ Sharing your map business-code between web and desktop applications

### The basic technique

The basic idea is a client-side composition of imagery (both tiled and non-tiled) and vector data. "First-class" map widgets like Leaflet and OpenLayers (for browser applications) or PTV xServer.NET (for Microsoft Windows applications) support this. The layering-stack when using PTV xMapServer as base-map looks as follows:

![Client-Composition](/Doc/ClientComposition.png)

### ToDos
* Add interaction (picking) for Win-Demo
* Enabling/disabling SharpMap layers
* Some dynamic filtering and styling
* Better sample data sources
* More docs
