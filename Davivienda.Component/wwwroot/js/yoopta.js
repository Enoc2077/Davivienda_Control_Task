// ========================================
// ARCHIVO: Davivienda.Component/wwwroot/js/yoopta.js
// ========================================

window.descargarArchivo = function (dataUrl, nombreArchivo) {
    try {
        console.log('📥 Iniciando descarga de:', nombreArchivo);
        const link = document.createElement('a');
        link.href = dataUrl;
        link.download = nombreArchivo;
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        setTimeout(() => {
            document.body.removeChild(link);
            console.log('✅ Descargado:', nombreArchivo);
        }, 100);
    } catch (error) {
        console.error('❌ Error al descargar:', error);
    }
};

console.log('✅ yoopta.js cargado correctamente');