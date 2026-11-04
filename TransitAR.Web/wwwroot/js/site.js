
//Nueva funcion acutalizar cada, aparte del polling donde pido data cada unos segundos, pido un actualiazr una vez antes 
function actualizarCada(caja, segundos, opciones = {}) {
    if (!caja) return;

    const actualizar = async () => {
        //pestaña escondida
        if (document.hidden) return;

        try {
            const r = await fetch(caja.dataset.url, { cache: 'no-store' });

            //redirected
            if (!r.ok || r.redirected) return;

            const html = await r.text();
            const guardado = opciones.antes ? opciones.antes() : null;
            caja.innerHTML = html;
            if (opciones.despues) opciones.despues(guardado);
        } catch {
            //sin conexion
        }
    };

    if (opciones.ahora) actualizar();

    setInterval(actualizar, segundos * 1000);
}