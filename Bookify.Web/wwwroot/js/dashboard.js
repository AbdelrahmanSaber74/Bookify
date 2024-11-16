document.addEventListener('DOMContentLoaded', function () {
    const renderChart = (selector, apiEndpoint, chartName, color) => {
        fetch(apiEndpoint)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! Status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                const chartOptions = {
                    series: [{
                        name: chartName,
                        data: data.map(item => parseInt(item.value)) // Ensure values are integers
                    }],
                    chart: {
                        fontFamily: 'inherit',
                        type: 'area',
                        height: 350,
                        toolbar: { show: false }
                    },
                    legend: { show: false },
                    dataLabels: { enabled: false },
                    fill: { type: 'solid', opacity: 1 },
                    stroke: {
                        curve: 'smooth',
                        show: true,
                        width: 3,
                        colors: [color]
                    },
                    xaxis: {
                        categories: data.map(item => item.label),
                        axisBorder: { show: true, color: '#ccc' },
                        axisTicks: { show: true, color: '#ccc' },
                        labels: {
                            style: {
                                colors: '#6c757d',
                                fontSize: '12px'
                            }
                        },
                        crosshairs: {
                            position: 'front',
                            stroke: {
                                color,
                                width: 1,
                                dashArray: 3
                            }
                        },
                        tooltip: {
                            enabled: true,
                            offsetY: 0,
                            style: { fontSize: '12px' }
                        }
                    },
                    yaxis: {
                        tickAmount: Math.max(...data.map(item => parseInt(item.value))),
                        min: 0,
                        labels: {
                            style: {
                                colors: '#6c757d',
                                fontSize: '12px'
                            }
                        }
                    },
                    tooltip: { style: { fontSize: '12px' } },
                    colors: ['#ecf0f1'],
                    grid: {
                        borderColor: '#e0e0e0',
                        strokeDashArray: 4,
                        yaxis: { lines: { show: true } }
                    },
                    markers: {
                        strokeColor: color,
                        strokeWidth: 3
                    }
                };

                const chart = new ApexCharts(document.querySelector(selector), chartOptions);
                chart.render();
            })
            .catch(error => {
                console.error(`Error fetching data for ${chartName}:`, error);
            });
    };

    // Render the RentalsPerDay chart
    renderChart('#RentalsPerDay', '/Dashboard/GetRentalsPerDay', 'Books', '#3498db');

    // Render the SubscribersPerCity chart
    renderChart('#SubscribersPerCity', '/Dashboard/GetSubscribersPerCity', 'Subscribers', '#3498db');
});
