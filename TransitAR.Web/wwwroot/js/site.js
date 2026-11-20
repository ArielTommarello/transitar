
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


//Modal confirmaciones para todos los formilarios. data-condfimrar="texto" y data-confirmar-boton="Borrar" para texto boton.
document.addEventListener('submit', (e) => {

    //si hay error return
    if (e.defaultPrevented) return;

    const form = e.target;
    const boton = e.submitter;
    const texto = boton?.dataset.confirmar ?? form.dataset.confirmar;
    if (!texto) return;

    e.preventDefault();
    const textoBoton = boton?.dataset.confirmarBoton ?? form.dataset.confirmarBoton;

    abrirConfirmacion(texto, textoBoton, () => {
        // propio handler (asp-page-handler en el botón)
        if (boton) form.action = boton.formAction;
        form.submit();//envio formulario
    });
});

function abrirConfirmacion(texto, textoBoton, alAceptar) {
    const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('modalConfirmar'));
    document.getElementById('modalConfirmarTexto').textContent = texto;

    const aceptar = document.getElementById('modalConfirmarAceptar');
    aceptar.textContent = textoBoton || 'Confirmar';
    aceptar.onclick = () => {
        modal.hide();
        alAceptar();
    };

    modal.show();
}

//Modal blanquear contraseña (admin-usuarios y equipo en el refugio)
const modalBlanquear = document.getElementById('modalBlanquear');
if (modalBlanquear) {
    modalBlanquear.addEventListener('show.bs.modal', (e) => {
        const boton = e.relatedTarget;
        document.getElementById('blanquearNombre').textContent = boton.dataset.nombre;
        document.getElementById('formBlanquear').action = boton.dataset.url;
        document.getElementById('formBlanquear').reset();
    });
        
    modalBlanquear.addEventListener('shown.bs.modal', () => {
        document.getElementById('passwordNueva').focus();
    });
}