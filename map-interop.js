export function initMap(mapId) {
    let mymap = L.map(mapId).setView([49.01086, 8.37253], 6);

    let OpenStreetMap_DE = L.tileLayer('https://{s}.tile.openstreetmap.de/tiles/osmde/{z}/{x}/{y}.png', {
        maxZoom: 18,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    });
    OpenStreetMap_DE.addTo(mymap);

    return mymap;
}

var polyLines = [];

export function drawLine(map, color, opacity, lat1, lng1, lat2, lng2, lengthPercentage = 1, removeable = true) {

    let latlngs = [
        [lat1, lng1],
        [lat2, lng2]
    ];

    if (lengthPercentage !== 1) {
        let p1 = map.project([lat1, lng1]);
        let p2 = map.project([lat2, lng2]);

        let xDist = (p2.x - p1.x) * lengthPercentage;
        let yDist = (p2.y - p1.y) * lengthPercentage;

        let calcP = L.point(p1.x + xDist, p1.y + yDist);
        let calcLatLng = map.unproject(calcP);

        latlngs = [
            [lat1, lng1],
            [calcLatLng.lat, calcLatLng.lng]
        ];
    }

    var polyline = L.polyline(latlngs, { color: color, weight: 2.5, opacity: opacity });
    polyline.addTo(map);
    if (removeable) {
        polyLines[polyLines.length] = polyline;
    }
}

export function addMarker(map, lat, lng, popupHtml) {
    let marker = L.marker([lat, lng]).addTo(map);
    marker.bindPopup(popupHtml);
}

export function removeLines(map) {
    polyLines.forEach((item, index) => { item.remove(map) });
    polyLines = [];
}