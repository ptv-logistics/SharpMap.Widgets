# SharpMap.Widgets
Build responsive map applications for web and desktop

[SharpMap](https://sharpmap.codeplex.com/) is a powerful mapping library that supports a large variety of standards and formats.
But the interactive Web- and Windows-Widgets of the SharpMap project are somehow neglected. On the other hand, "slippy map" Widgets like [Leaflet](http://leafletjs.com/) or [Ptv xServer.NET](http://xserver.ptvgroup.com/en-uk/cookbook/explore/xserver-net-demo-center/) cannot handle mass data very well. SharpMap.Widgets shows how to combine the SharpMap renderer with these widgets, so it combines the power of SharpMap with the Look&Feel of modern widgets.

[Web-Sample](http://80.146.239.139/SharpMap.Widgets/)

**Windows-Sample:**
![Windows-Sample](/Doc/SharpMap.Win.png)

### What the project shows
* Efficiently render large data sets on "slippy" map widgets with SharpMap
* Add interaction to pick an item
* Implementing SharpMap interfaces to render your custom data source
* Sharing your map business-code between web and desktop applications

### The projects
* SharpMap.Win - Windows (Forms) sample
* SharpMap.Web - Browser sample
* SharpMap.WinThin - Windows sample that uses the middle ware from the web project
* SharpMap.Print - Sample for creating static images
* SharpMap.Common - Shared code

### The basic technique

The basic idea is to compose (or "mesh-up") imagery and vector data on the client-side. "First-class" map widgets like Leaflet and OpenLayers (for browser applications) or PTV xServer.NET (for Microsoft Windows applications) support this. The partitioning is done both between "base-map" and "application-data", wich is delivered from different services, as well as between different rendering-techniques, depending on the type of data and required responsiveness. There are tree main categroies of render-data:

* Persistent data than can be rendered indendent from the viewport. This is typically the case for polygons and lines (areas or road segments).
* Persistent data that cannot be rendered in tiles. This is the case for objects that "bleed" outside tiles or are using a heuristic layout algorithm. Symbols and labels need this strategy.
* Transient data, for example the currently selected item.

So the layering-stack when using PTV xMapServer as base-map looks as follows:

<a href="url"><img src="/Doc//ClientComposition.png" align="center" width="600" ></a>

### Architecture considerations

A crucial part for UI applications is the decision about the basic architecture. There are two categories of UI-applications:

* 2-tier, where the client directly accesses the data inside the database using SQL. It could even be possible that the database is not  hosted on an external server, but an embedded (MS Access, SQLite) database, or just a plain .csv file. In this case we speak of a 1-tier applicatin.
* 3-tier, where the client is only a thin application that consumes web services. A middleware supplies the client with the data via HTTP (SOAP, REST) interfaces. Web-clients usually use this architecture, but it's also possible to build a thin Windows (Forms, WPF) client that doesn't access the database directly, but only via a web interface.

The difference is very important with aspect on rendering performance. The renderer reads the geometry and attribute data from the database. This requires a high bandwidth between renderer and database server. If the client doesn't have this high-bandwidth connection to the database it's recommended to use a middleware, so only the rendered images have to be transferred to the client.

<a href="url"><img src="/Doc/RenderArchitecture.png" align="center" width="600" ></a>

### ToDos
* Add interaction to thin Windows-Client
* Some dynamic filtering and styling
* Better sample data sources
* More docs
