document.addEventListener("DOMContentLoaded", () => {
    // 1. Initialize map (default to center of Pakistan)
    const map = L.map('map').setView([30.3753, 69.3451], 5);
    
    // Check theme for map styling
    const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
    const tileUrl = isDark 
        ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
        : 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
        
    L.tileLayer(tileUrl, {
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    let customerMarker = null;
    let radiusCircle = null;
    let labourerLayerGroup = L.layerGroup().addTo(map);
    
    let currentLat = null;
    let currentLng = null;

    // Custom pins
    const customerIcon = L.divIcon({
        className: 'custom-pin',
        html: `<div style="background-color: #3b82f6; width: 24px; height: 24px; border-radius: 50%; border: 3px solid white; box-shadow: 0 0 10px rgba(0,0,0,0.5);"></div>`,
        iconSize: [24, 24],
        iconAnchor: [12, 12]
    });

    const labourerIcon = L.divIcon({
        className: 'custom-pin',
        html: `<div style="background-color: #7c3aed; width: 20px; height: 20px; border-radius: 50%; border: 2px solid white; box-shadow: 0 0 5px rgba(0,0,0,0.5);"></div>`,
        iconSize: [20, 20],
        iconAnchor: [10, 10]
    });

    // 2. DOM Elements
    const btnDetect = document.getElementById('detectLocation');
    const inputAddress = document.getElementById('addressInput');
    const radiusSlider = document.getElementById('radiusSlider');
    const radiusDisplay = document.getElementById('radiusDisplay');
    const categoryFilter = document.getElementById('categoryFilter');
    const ratingFilter = document.getElementById('ratingFilter');
    const ratingDisplay = document.getElementById('ratingDisplay');
    const expFilter = document.getElementById('expFilter');
    const btnReset = document.getElementById('resetFilters');
    const resultsContent = document.getElementById('resultsContent');
    const mapLoading = document.getElementById('mapLoading');

    // 3. Event Listeners
    btnDetect.addEventListener('click', () => {
        if (navigator.geolocation) {
            mapLoading.style.display = 'flex';
            navigator.geolocation.getCurrentPosition(
                (pos) => {
                    updateCustomerLocation(pos.coords.latitude, pos.coords.longitude);
                    fetchResults();
                },
                (err) => {
                    alert("Could not get location. Please allow location access or type an address.");
                    mapLoading.style.display = 'none';
                }
            );
        }
    });

    // Debounce for inputs
    let fetchTimer = null;
    function debouncedFetch() {
        clearTimeout(fetchTimer);
        fetchTimer = setTimeout(fetchResults, 500);
    }

    radiusSlider.addEventListener('input', (e) => {
        const val = e.target.value;
        radiusDisplay.textContent = val;
        if (radiusCircle) {
            radiusCircle.setRadius(val * 1000); // km to meters
            map.fitBounds(radiusCircle.getBounds());
        }
        if (currentLat) debouncedFetch();
    });

    ratingFilter.addEventListener('input', (e) => {
        ratingDisplay.textContent = e.target.value;
        if (currentLat) debouncedFetch();
    });

    categoryFilter.addEventListener('change', () => { if (currentLat) fetchResults(); });
    expFilter.addEventListener('input', () => { if (currentLat) debouncedFetch(); });

    btnReset.addEventListener('click', () => {
        radiusSlider.value = 10;
        radiusDisplay.textContent = 10;
        categoryFilter.value = "";
        ratingFilter.value = 0;
        ratingDisplay.textContent = 0;
        expFilter.value = 0;
        inputAddress.value = "";
        if (radiusCircle) radiusCircle.setRadius(10 * 1000);
        if (currentLat) fetchResults();
    });

    // Geocoding via Nominatim
    inputAddress.addEventListener('change', async (e) => {
        const query = e.target.value.trim();
        if (!query) return;
        
        mapLoading.style.display = 'flex';
        try {
            const res = await fetch(`https://nominatim.openstreetmap.org/search?format=json&limit=1&q=${encodeURIComponent(query)}`);
            const data = await res.json();
            if (data && data.length > 0) {
                updateCustomerLocation(parseFloat(data[0].lat), parseFloat(data[0].lon));
                fetchResults();
            } else {
                alert("Location not found.");
                mapLoading.style.display = 'none';
            }
        } catch (err) {
            console.error(err);
            mapLoading.style.display = 'none';
        }
    });

    // 4. Core Functions
    function updateCustomerLocation(lat, lng) {
        currentLat = lat;
        currentLng = lng;
        
        if (customerMarker) {
            customerMarker.setLatLng([lat, lng]);
        } else {
            customerMarker = L.marker([lat, lng], {icon: customerIcon}).addTo(map);
        }

        const radiusMeters = radiusSlider.value * 1000;
        if (radiusCircle) {
            radiusCircle.setLatLng([lat, lng]);
            radiusCircle.setRadius(radiusMeters);
        } else {
            radiusCircle = L.circle([lat, lng], {
                radius: radiusMeters,
                color: '#7c3aed',
                fillColor: '#7c3aed',
                fillOpacity: 0.1,
                weight: 1
            }).addTo(map);
        }

        map.fitBounds(radiusCircle.getBounds());
    }

    async function fetchResults() {
        if (!currentLat || !currentLng) return;
        
        mapLoading.style.display = 'flex';
        
        const r = radiusSlider.value;
        const c = categoryFilter.value;
        const rat = ratingFilter.value;
        const e = expFilter.value;
        
        const url = `/Customer/Browse?handler=Results&lat=${currentLat}&lng=${currentLng}&radiusKm=${r}&categoryId=${c}&minRating=${rat}&minExp=${e}`;
        
        try {
            const response = await fetch(url);
            if (response.ok) {
                const html = await response.text();
                resultsContent.innerHTML = html;
            }
        } catch (err) {
            console.error("Error fetching results", err);
        } finally {
            mapLoading.style.display = 'none';
        }
    }

    // Exposed to global scope for the partial view to call
    window.updateMapPins = function(labourers) {
        labourerLayerGroup.clearLayers();
        
        if (!labourers || !Array.isArray(labourers)) return;
        
        labourers.forEach(l => {
            if (l.lat && l.lng) {
                const marker = L.marker([l.lat, l.lng], {icon: labourerIcon});
                marker.bindPopup(`
                    <div style="text-align: center; min-width: 150px;">
                        <strong>${l.name}</strong><br>
                        <span style="color: #666; font-size: 0.9em;">${l.cat}</span><br>
                        <div style="margin: 5px 0; font-weight: bold;">Rs. ${l.rate}/hr</div>
                        <a href="/Customer/LabourerProfile/${l.id}" style="display: inline-block; padding: 4px 8px; background: #7c3aed; color: white; border-radius: 4px; text-decoration: none; font-size: 0.9em;">View Profile</a>
                    </div>
                `);
                labourerLayerGroup.addLayer(marker);
            }
        });
    };
});
