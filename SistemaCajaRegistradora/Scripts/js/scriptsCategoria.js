﻿function AgregarForms(urlForms) {
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
    } else {
        mensaje = "Error: El campo nombre esta vacio";
        showMenssage('error', mensaje, true);
    }

}
function formsEditar(urlEditarForms, id) {
    $.get(urlEditarForms + '/' + id, function (data) {
        abrirModal(data);
    });
}
function editarCategoria(urlEditar) {
    var idCategoria = $('#id').val();
    var nombre = $('#nombre').val();
    var descripcion = $('#descripcion').val();

    var isValidCampo = true;
    isValidCampo = validarCampos(nombre);
    if (isValidCampo) {
        var categoria = {
            "id": idCategoria,
            "nombre": nombre,
            "descripcion": descripcion,
        };
        $.post(urlEditar + '/', categoria, function (data) {
            if (data > 0) {
                sessionStorage.clear();
                sessionStorage.nombre = categoria.nombre;
                sessionStorage.mensaje = 'Categoria modificada correctamente: ';
                location = location.href;
            } else {
                alert("Error");
            }
        });
    } else {
        mensaje = "Error: El campo nombre esta vacio";
        showMenssage('error', mensaje, true);
    }
}
function formsEliminar(urlFormsEliminar, id) {
    $.get(urlFormsEliminar + '/' + id, function (data) {
        abrirModal(data);
    });
}
function eliminarCategoria(urlEliminar, id) {
    $.post(urlEliminar + '/' + id, function (data) {
        console.log(data);
        if (data > 0) {
            sessionStorage.clear();
            sessionStorage.mensaje = 'Categoria eliminado correctamente';
            location = location.href;
        } else {
            sessionStorage.clear();
            mensaje = "Error: La categoria no se elimino, asegurese de que la categoria no se ocupe";
            showMenssage('error', mensaje, true);
        }
    },'json');
}

const abrirModal = (data) => {
    try {
        $('#coreModal').html(data);
        $('#coreModal').modal('show');
    } catch (e) {
        Swal.fire({
            icon: 'error',
            title: 'Error de autorizacion!',
            text: 'No puede ingresar a este modulo o funcion, ya que no tiene los permisos suficientes'
        });
    }

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
const showMenssage = (type, mensaje, toast) => {
    if (type == 'error') {
        Swal.fire({
            icon: 'error',
            title: 'Error!',
            text: mensaje,
            toast: toast,
            position: 'top-end'
        });
        return;
    } else if (type == 'success') {
        Swal.fire({
            icon: 'success',
            title: 'Exito!',
            text: mensaje,
            toast: toast,
            position: 'top-end',
        });
        return;
    } else if (type == 'warning') {
        const swal = Swal.mixin({
            customClass: {
                confirmButton: 'btn btn-success',
                cancelButton: 'btn btn-danger'
            },
            buttonsStyling: false
        });
        swal.fire({
            title: 'Informacion!',
            text: mensaje,
            icon: type,
            showCancelButton: true,
            confirmButtonText: 'Si',
            cancelButtonText: 'No',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                verificarOpcion(true)
            }
            return;
        })
    }
    console.log('Type not found')
}