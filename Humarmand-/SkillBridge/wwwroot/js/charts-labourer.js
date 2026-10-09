function initLabourerCharts(data) {
    if (!data) return;

    // 1. Earnings Chart (Line)
    const earningsCtx = document.getElementById('earningsChart');
    if (earningsCtx && data.earnings && data.earnings.length > 0) {
        new Chart(earningsCtx, {
            type: 'line',
            data: {
                labels: data.earnings.map(d => new Date(d.Date).toLocaleDateString(undefined, {month:'short', day:'numeric'})),
                datasets: [{
                    label: 'Earnings (PKR)',
                    data: data.earnings.map(d => d.Value),
                    borderColor: '#10b981',
                    backgroundColor: 'rgba(16, 185, 129, 0.1)',
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true, grid: { color: 'rgba(255, 255, 255, 0.1)' } },
                    x: { grid: { display: false } }
                }
            }
        });
    }

    // 2. Funnel Chart (Horizontal Bar)
    const funnelCtx = document.getElementById('funnelChart');
    if (funnelCtx && data.funnel) {
        new Chart(funnelCtx, {
            type: 'bar',
            data: {
                labels: ['Requests', 'Accepted', 'Completed'],
                datasets: [{
                    label: 'Job Funnel',
                    data: [data.funnel.TotalRequests, data.funnel.Accepted, data.funnel.Completed],
                    backgroundColor: ['#3b82f6', '#7c3aed', '#10b981'],
                    borderRadius: 4
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    x: { beginAtZero: true, grid: { color: 'rgba(255, 255, 255, 0.1)' } },
                    y: { grid: { display: false } }
                }
            }
        });
    }
}
