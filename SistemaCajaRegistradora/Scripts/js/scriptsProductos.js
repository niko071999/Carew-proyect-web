function AgregarForms(urlForms) {
    $.get(urlForms+'/', function (data) {
        abrirModal(data);
    })
}
function AgregarProducto(urlAgregar) {
    var idproducto = $('#id').val();
    var codigo_barra = $('#codigo_barra').val();
    var nombre = $('#nombre').val();
    var precio = $('#precio').val();
    var stock = $('#stock').val();
    var stockmin = $('#stockmin').val();
    var stockmax = $('#stockmax').val();
    var prioridadid = $('#prioridadId').val();
    var categoriaid = $('#categoriaId').val();
    var rutaImg = $('#rutaImg').val();
    var fecha_creacion = "";
    var eliminado = "null";

    //Validaciones campos vacios
    var isValid = true;
    isValid = validarCampos(codigo_barra, nombre, prioridadid, categoriaid);

    //console.log("codigo: " + codigo_barra,
    //    "nombre: " + nombre,
    //    "precio " + precio,
    //    "stock " + stock,
    //    "stockmin: " + stockmin,
    //    "stockmax: " + stockmax,
    //    "fecha_creacion: " + fecha_creacion,
    //    "prioridadid: " + prioridadid,
    //    "categoriaid: " + categoriaid,
    //    "rutaImg: " + rutaImg,
    //    isValid
    //)

    if (isValid) {
        var producto = {
            "id": idproducto,
            "codigo_barra": codigo_barra,
            "nombre": nombre,
            "precio": precio,
            "stock": stock,
            "stockmin": stockmin,
            "stockmax": stockmax,
            "fecha_creacion": fecha_creacion,
            "prioridadid": prioridadid,
            "categoriaid": categoriaid,
            "rutaImg": rutaImg,
            "eliminado": eliminado
        };

        console.log(producto);

        $.post(urlAgregar+'/', producto, function (data) {
            if (data > 0) {
                location = location.href;
            } else {
                alert("Error");
            }
        });
    }
}
function formsImage(urlFormsImagen, id) {
    $.get(urlFormsImagen + '/' + id, function (data) {
        abrirModal(data);
        if ($("#idArchivo").files.length==0) {
            $("#btnSubir").prop("disabled");
        }
    });
}
function subirImagen(url) {
    var inputArchivoId = document.getElementById('idArchivo');
    var archivo = inputArchivoId.files[0];
    var dataForm = new FormData();

    dataForm.append('archivo', archivo);
    $.ajax({
        url: url,
        type: 'POST',
        data: dataForm,
        contentType: false,
        processData: false,
        success: function (data) { location = location.href; },
        error: function (data) { alert("Error de subida de archivo") },
    });
}
function formsEliminar(urlFormsEliminar, id) {
    $.get(urlFormsEliminar + '/' + id, function (data) {
        abrirModal(data);
    });
}
function eliminarProducto(urlEliminar, id) {
    console.log(urlEliminar+id)
    $.post(urlEliminar + '/' + id, function (data) {
        if (data != null) {
            location = location.href;
        } else {
            alert('Producto no eliminado')
        }
    });
}

const validar = () => {
    // Obtener nombre de archivo
    let archivo = document.getElementById('idArchivo').value,
        // Obtener extensión del archivo
        extension = archivo.substring(archivo.lastIndexOf('.'), archivo.length);
    // Si la extensión obtenida no está incluida en la lista de valores
    // del atributo "accept", mostrar un error.
    if (document.getElementById('archivo').getAttribute('accept').split(',').indexOf(extension) < 0) {
        alert('Archivo inválido. No se permite la extensión ' + extension);
    }
    console.log(archivo)
}

const abrirModal = (data) => {
    $('#coreModal').html(data);
    $('#coreModal').modal('show');
}

const validarCampos = (codigo_barra, nombre, prioridadid, categoriaid) => {
    var valid = true;
    if (codigo_barra.trim() == "") {
        $('#codigo_barra').css('border-color', 'red');
        valid = false;
    }
    if (nombre.trim() == "") {
        $('#nombre').css('border-color', 'red');
        valid = false;
    }
    if (prioridadid == "") {
        $('#prioridadId').css('border-color', 'red');
        valid = false;
    }
    if (categoriaid == "") {
        $('#categoriaId').css('border-color', 'red');
        valid = false;
    }
    return valid;
}