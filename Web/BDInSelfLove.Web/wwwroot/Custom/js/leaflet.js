var mymap = L.map('mapid').setView([42.66849, 23.29027], 17);

L.tileLayer('https://api.mapbox.com/styles/v1/{id}/tiles/{z}/{x}/{y}?access_token={accessToken}', {
    attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, Imagery © <a href="https://www.mapbox.com/">Mapbox</a>',
    maxZoom: 18,
    id: 'mapbox/streets-v11',
    tileSize: 512,
    zoomOffset: -1,
    accessToken: 'pk.eyJ1Ijoic2xwd2xrciIsImEiOiJjazdpMzh4eXMwZnAwM2ZvMzkxZWU5ODJxIn0.a5gyt1RTpn7aNL38hohGZw'
}).addTo(mymap);

var marker = L.marker([42.66849, 23.29027]).addTo(mymap);
