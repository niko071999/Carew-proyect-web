function AgregarForms(urlForms) {
    $.get(urlForms + '/', function (data) {
        abrirModal(data);
    });
}
function AgregarCategoria(urlAgregar) {
    var idCategoria = $('#id').val();
    var nombre = $('#nombre').val();
    var descripcion = $('#descripcion').val();

    let isValidCampo = true;
    isValidCampo = validarCampos(nombre);

    if (isValidCampo) {
        var categoria = {
            "id": idCategoria,
            "nombre": nombre,
            "descripcion": descripcion,
        };
        $.post(urlAgregar + '/', categoria, function (data) {
            if (data > 0) {
                sessionStorage.clear();
                sessionStorage.nombre = categoria.nombre;
                sessionStorage.mensaje = 'Categoria creada correctamente: ';
                location = location.href;
            } else {
                alert("Error");
            }
        });
    }

}

const abrirModal = (data) => {
    $('#coreModal').html(data);
    $('#coreModal').modal('show');
}

const validarCampos = (nombre) => {
    let valid = true;
    let atr = '';

    if (nombre.trim() == "") {
        atr = $('#text_nombre').attr('class');
        $('#text_nombre').addClass(atr + ' text-danger')
        $('#nombre').css('border-color', 'red');
        valid = false;
    } else {
        $('#text_nombre').removeClass('text-danger');
        $('#nombre').css('border-color', '');
    }
    return valid;
}