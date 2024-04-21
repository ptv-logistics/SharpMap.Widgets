/*
 * L.VirtualLayer
 */
L.VirtualLayer = L.Layer.extend({
    options: {
        attribution: '',
        minZoom: 0,
        maxZoom: 99,
        bounds: L.latLngBounds([-85.05, -180], [85.05, 180])
    },

    hostLayer: null,

    name: '',

    visible: false,

    initialize: function (hostLayer, name, options) {

        this.hostLayer = hostLayer;
        this.name = name;

        L.setOptions(this, options);
    },

    onAdd: function (map) {
        this._map = map;
        this.hostLayer.virtualLayers[this.name] = true;

        setTimeout(L.bind(this.redraw, this, map), 0);
    },

    isVisible: function () {
        return this.hostLayer.virtualLayers[this.name];
    },

    onRemove: function (map) {
        this.hostLayer.virtualLayers[this.name] = false;

        setTimeout(L.bind(this.redraw, this, map), 0);
    },

    _redraw: function () {
        this.redraw(this.map);
    },

    redraw: function (map) {
        var visibileLayers = [];
        for (var key in this.hostLayer.virtualLayers)
            if (this.hostLayer.virtualLayers[key])
                visibileLayers.push(key);

        if (this.hostLayer.wmsParams)
            this.hostLayer.wmsParams.layers = visibileLayers.join(',');
        else {
            var url = this.hostLayer._url = this.hostLayer._url.substring(0, this.hostLayer._url.indexOf("&layers="));
            url = this.hostLayer._url + '&layers=' + visibileLayers.join(',');
            this.hostLayer.setUrl(url);
        }

        if (visibileLayers.length == 0)
            map.removeLayer(this.hostLayer, false);
        else {
            map.addLayer(this.hostLayer);
            this.hostLayer.redraw();
        }

        return this;
    }
});
