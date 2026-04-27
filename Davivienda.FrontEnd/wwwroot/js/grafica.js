// ============================================================
// Davivienda.FrontEnd / wwwroot / js / grafica.js
// ============================================================

var _asignacionesRef = null;

window.registrarInstanciaAsignaciones = function (dotNetRef) {
    _asignacionesRef = dotNetRef;
    console.log('Instancia Asignaciones registrada OK');
};

window.renderEstadoTareasChart = function (canvasId, pendientes, enProgreso, completadas) {
    try {
        var existing = Chart.getChart(canvasId);
        if (existing) existing.destroy();

        var ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.warn('Canvas no encontrado:', canvasId);
            return;
        }

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Pendiente', 'En Progreso', 'Completado'],
                datasets: [{
                    label: 'Tareas',
                    data: [pendientes, enProgreso, completadas],
                    backgroundColor: [
                        'rgba(251, 191, 36, 0.85)',
                        'rgba(249, 115, 22, 0.85)',
                        'rgba(34, 197, 94, 0.85)'
                    ],
                    borderColor: ['#f59e0b', '#f97316', '#16a34a'],
                    borderWidth: 2,
                    borderRadius: 10,
                    borderSkipped: false,
                    barThickness: 55,
                    // Mostrar barra aunque el valor sea 0
                    minBarLength: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                onHover: function (event, elements) {
                    if (event.native && event.native.target) {
                        event.native.target.style.cursor = elements.length > 0 ? 'pointer' : 'default';
                    }
                },
                onClick: function (event, elements) {
                    if (elements.length > 0 && _asignacionesRef) {
                        var idx = elements[0].index;
                        var estados = ['Pendiente', 'En Progreso', 'Completado'];
                        var estado = estados[idx];
                        console.log('Barra clickeada:', estado);
                        _asignacionesRef.invokeMethodAsync('FiltrarDesdeGrafica', estado)
                            .catch(function (err) {
                                console.error('Error al llamar FiltrarDesdeGrafica:', err);
                            });
                    }
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#1e293b',
                        titleColor: '#f8fafc',
                        bodyColor: '#94a3b8',
                        borderColor: '#334155',
                        borderWidth: 1,
                        padding: 12,
                        cornerRadius: 8,
                        callbacks: {
                            label: function (ctx) {
                                return ' ' + ctx.parsed.y + ' tarea(s) — clic para filtrar';
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        border: { display: false },
                        ticks: {
                            color: '#94a3b8',
                            font: { size: 13, weight: '600' }
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: { color: '#f1f5f9' },
                        border: { display: false },
                        ticks: {
                            color: '#cbd5e1',
                            font: { size: 11 },
                            stepSize: 1,
                            precision: 0
                        }
                    }
                },
                animation: {
                    duration: 800,
                    easing: 'easeInOutQuart'
                }
            }
        });

        console.log('Grafica OK: P=' + pendientes + ' EP=' + enProgreso + ' C=' + completadas);
    } catch (e) {
        console.error('Error grafica:', e);
    }
};