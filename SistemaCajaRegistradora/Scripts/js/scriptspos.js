$(document).ready(init);

let listaVenta = [];
let metodoPago = 1; //1: Efectivo - 2: Cred Deb
let totalPagado = 0; //monto recibido por el cliente
let precioPagar = 0; //el monto total de la venta
let totalvuelto = 0;
//input_montoRecibido

$("#stockProd").change(function () { verificarStockNegativo(); });
$("#btnBorrar").click(function () { borrarCodigo(); });
$("#btn_printLastSales").click(function () { imprimirLastSales() });
//$("#btn_cerrarCaja").click(function () {
//    $.get('/Venta/CerrarCaja/');
//})

let btn_finalizarventa = document.getElementById('btn_finalizarventa');
let btn_cancelarventa = document.getElementById('btn_cancelarventa');
let btn_efectivo = document.getElementById('btn_efectivo');
let btn_debcred = document.getElementById('btn_debcred');
let input_montoRecibido = document.getElementById('input_montoRecibido');

btn_cancelarventa.addEventListener("click", cancelarVenta);
input_montoRecibido.addEventListener('change', drawTable);
btn_finalizarventa.addEventListener("click", confirmacionFinalizarVenta);
btn_efectivo.addEventListener("click", function () {
    metodoPago = 1;
    $("#txt_metodopago").html("Efectivo"); 
    $("#input_montoRecibido").attr('disabled', false);
});
btn_debcred.addEventListener("click", function () {
    metodoPago = 2;
    $("#txt_metodopago").html("Debito/Credito");
    $("#input_montoRecibido").val(precioPagar);
    drawTable();
    $("#input_montoRecibido").attr('disabled', true);
});
$("#btn_print").click(function () {
    $(".modal-body").printThis({
        importCSS: true,
        importStyle: true,
    });
});


function imprimirLastSales() {
    $.get('/Venta/ultimaVenta', function (data) {
        let msg = '';
        if (data.data == 0) {
            msg = data.msg;
            showMenssage('error', msg, true);
        } else {
            let idventa = data.data;
            msg = data.msg;
            showMenssage('success', msg, true);
            $.get('/Venta/viewBoletaVenta/' + idventa, function (data) {
                sessionStorage.print = true;
                abrirModal(data);
            });
        }
    });
}
function confirmacionFinalizarVenta() {
    Swal.fire({
        title: 'Finalizar Venta',
        text: "¿Quieres finalizar para generar la boleta de la venta?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Si, finalizar la venta',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            finalizarVenta();
        }
    });
}
function finalizarVenta() {
    let dV = [];
    //Creo el objeto Venta, con sus atributos correspondientes
    var venta = {
        "total_venta": $("#precioTotalPagar").html().replace(/[$.]/g, ''),
        "fecha_creacion": "",
        "vuelto": $("#totalVuelto").html().replace(/[$.]/g, ''),
        "movimientocajaid": "",
        "metodo_pagoid": metodoPago,
        "monto_ingresado": $("#input_montoRecibido").val().replace(/[$.]/g, ''),
        "num_boleta": ""
    }
    //Creo un array con los datos de detalle venta
    for (var i = 0; i < listaVenta.length; i++) {
        detalleVenta = {
            "id": '',
            "productoid": listaVenta[i].id,
            "ventaid": '',
            "total_precio_producto": listaVenta[i].preciototal,
            "total_cantidad_producto": listaVenta[i].cantidad
        }
        dV.push(detalleVenta);
    }
    
    var ventaDetalle = {
        venta,
        dV
    }

    $.ajax({
        url: '/Venta/finalizarVenta',
        contentType: "application/json",
        dataType: 'json',
        data: JSON.stringify(ventaDetalle),
        type: 'POST',
        success: function (data) {
            console.log(data);
            let msg = '';
            if (data.data == '') {
                if (data.idventa == -1) {
                    location = location.href;
                }
                msg = data.msg;
                showMenssage('error', msg, true);
            } else {
                let idventa = data.idventa;
                msg = data.msg;
                showMenssage('success', msg, true);
                $.get('/Venta/viewBoletaVenta/' + idventa, function (data) {
                    sessionStorage.print = true;
                    abrirModal(data);
                });
            }
        },
        error: function (data) {
            console.log(data);
        }
    });
}
function init() {
    $('#bodyTable').empty();
    $('#bodyTable').append(
        `<tr class="h-25 overflow-auto">
            <td colspan="5">
                <h4 class="text-center">No existe ningun producto cargado</h4>
                <h5 class="text-center">Empiece escaneando un producto para agregarlo a la lista</h5>
            </td>                    
        </tr>`
    );
    $('#codigoAdd').focus();
    $('#stockProd').val(1);
    $("#input_montoRecibido").val(0);
    disabledSwitch(true);

    countdownInit()
    formateodatos();
}


document.getElementById('coreModal').addEventListener('hide.bs.modal', function (event) {
    if (sessionStorage.print) {
        sessionStorage.print = false;
        location = location.href;
    }
});
document.getElementById('coreModal').addEventListener('shown.bs.modal', function (event) {
    if (sessionStorage.print) {
        $(".modal-body").printThis({
            importCSS: true,
            importStyle: true,
        });
    }
});
$('#codigoAdd').bind('keyup', function (e) {
    var key = e.keyCode || e.which;
    cargarProducto(key);
});
$('#stockProd').bind('keyup', function (e) {
    var key = e.keyCode || e.which;
    cargarProducto(key);
});

function ingresarCodigo(codigo) {
    $('#codigoAdd').val(codigo);
    cargarProducto(13);
}

function cargarProducto(key) {
    let codigo = $('#codigoAdd').val();
    let sprod = $('#stockProd').val();
    let producto = {
        "id": 0,
        "codigo_barra": codigo,
        "nombre": 'test',
        "precio": 0,
        "stock": sprod,
        "stockmin": 0,
        "stockmax": 0,
        "fecha_creacion": null,
        "prioridadid": 0,
        "categoriaid": 0,
        "rutaImg": 'test'
    }
    if (key === 13 && codigo.trim() != '') {
        $.post('/Venta/cargarProducto', producto, function (data) {
            let exist = false;
            if (data.codigobarra != "") {
                if (listaVenta.length > 0) {
                    for (let i = 0; i < listaVenta.length; i++) {
                        if (listaVenta[i].codigobarra == data.codigobarra) {
                            listaVenta[i].cantidad = listaVenta[i].cantidad + data.cantidad;
                            listaVenta[i].preciototal = listaVenta[i].precio * listaVenta[i].cantidad;
                            exist = true;
                        }
                    }
                    if (!exist) {
                        listaVenta.push(data);
                    }
                } else {
                    listaVenta.push(data);
                }
                
                drawTable();
                
                disabledSwitch(false);
            } else {
                showMenssage('error', data.mensaje, true);
            }
        });
        $('#codigoAdd').focus();
        $('#codigoAdd').val("");
        $('#stockProd').val(1);
    }
}

function drawTable() {
    console.log(listaVenta);
    $('#bodyTable').empty();
    $("#txt_metodopago").html("Efectivo");
    precioPagar = 0; 
    totalvuelto = 0;
    let cantidadtotalprod = listaVenta.length; 
    if (cantidadtotalprod == 0) {
        $('#bodyTable').append(
            `<tr class="h-25 overflow-auto">
            <td colspan="5">
                <h4 class="text-center">No existe ningun producto cargado</h4>
                <h5 class="text-center">Empiece escaneando un producto para agregarlo a la lista</h5>
            </td>                    
        </tr>`
        );
        disabledSwitch(true);
        $("#icon_circle").removeClass("text-success");
        $("#icon_circle").addClass("text-secondary");
        $("#txt_estadoVenta").html("Sin proceso de venta");
    }
    if (cantidadtotalprod>0) {
        $("#icon_circle").removeClass("text-secondary");
        $("#icon_circle").addClass("text-success");
        $("#txt_estadoVenta").html("En proceso de venta");
    }
    for (let i = 0; i < listaVenta.length; i++) {
        listaVenta[i].preciototal = listaVenta[i].precio * listaVenta[i].cantidad;
        precioPagar += listaVenta[i].preciototal;
        $("#bodyTable").append(
            `<tr>
                <td>${listaVenta[i].nombre}</td>
                <td class="precio">${listaVenta[i].precio}</td>
                <td>
                    <div class="row col">
                        <div class="col-12">
                           <input id="txt_${listaVenta[i].codigobarra}" class="form-control form-control-sm w-50" 
                                   type="number" max="${listaVenta[i].cantidadmax}" 
                                   value="${listaVenta[i].cantidad}" 
                                   onchange="verificarStockMax('${listaVenta[i].codigobarra}')" />
                        </div>
                    </div>
                    <div class="row col">
                        <div class="col-12">
                            <b>Cantidad maxima:</b> ${listaVenta[i].cantidadmax}
                        </div>
                    </div>

                <td class="precio">${listaVenta[i].preciototal}</td>
                <td>
                    <button type="button"
                            class="btn btn-sm btn-circle btn-danger"
                            onclick="quitarProducto('${listaVenta[i].codigobarra}')">
                        <i class="fas fa-times"></i>
                    </button>
                </td>
            </tr>`
        )
    }
    totalPagado = $("#input_montoRecibido").val().replace(/[$.]/g, '');
    totalvuelto = totalPagado - precioPagar;
    $("#cantidadTotalProd").html(cantidadtotalprod);
    $("#precioTotalPagar").html(precioPagar);
    if (totalvuelto >= 0) {
        $("#totalVuelto").html(totalvuelto);
    } else {
        $("#input_montoRecibido").val(precioPagar);
        $("#totalVuelto").html(0);
    }
    formateodatos();
}
function disabledSwitch(bool) {
    $("#btn_finalizarventa").attr("disabled", bool);
    $("#btn_cancelarventa").attr("disabled", bool);
    $("#input_montoRecibido").attr('disabled', bool);
    $("#btn_efectivo").attr("disabled", bool);
    $("#btn_debcred").attr("disabled", bool);
}
function cancelarVenta() {
    Swal.fire({
        title: 'Cancelar la venta',
        text: "¿Seguro que quiere cancelar la venta?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Si, cancelar la venta',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire(
                'Cancelada',
                'La venta a sido cancelada con exito',
                'success'
            ).then((r) => {
                location = location.href;
            });
        }
    });
}
function quitarProducto(codigoProd) {
    if (listaVenta.length == 1) {
        cancelarVenta();
    } else {
        listaVenta = listaVenta.filter((item) => item.codigobarra != codigoProd);
        $('#bodyTable').empty();
        drawTable();
    }
}

function borrarCodigo() {
    $("#codigoAdd").val('');
    $('#codigoAdd').focus();
}
function verificarStockMax(codigoProd) {
    verificarStockNegativoTable(codigoProd);

    let i = listaVenta.findIndex((e) => e.codigobarra == codigoProd);
    let cantidad = $("#txt_" + codigoProd).val();
    let cantidad_max = listaVenta[i].cantidadmax;
    if (cantidad > cantidad_max) {
        $("#txt_" + codigoProd).val(cantidad_max);
        listaVenta[i].cantidad = cantidad_max;
    } else {
        listaVenta[i].cantidad = cantidad;
    }
    drawTable();
}
function verificarStockNegativo() {
    let cantidadProd = stockProd.value;
    if (cantidadProd < 1) {
        stockProd.value = 1;
    }
}
function verificarStockNegativoTable(codigo) {
    let cantidadProd = $("#txt_" + codigo).val();
    if (cantidadProd < 1) {
        $("#txt_" + codigo).val(1);
    }
}
function formateodatos() {
    //FORMATEO DE DATOS
    $("td.precio").each(function () { $(this).html(parseFloat($(this).text()).toLocaleString('es-CL', { style: 'currency', currency: 'CLP' })); });
    $("td.numero").each(function () { $(this).html(parseFloat($(this).text()).toLocaleString('es-CL')); });
    $("#input_montoRecibido").val(parseFloat($("#input_montoRecibido").val().replace(/[$.]/g, ''))
        .toLocaleString('es-CL'));
}
function countdownInit() {
    $.get('/MovimientoCaja/getFechaApertura', function (data) {
        let fecha = new Date(data);
        $('#div_countdown').countdown({
            until: fecha, compact: true,
            layout: 'Falta <b>{hnn}{sep}{mnn}{sep}{snn}</b> {desc}',
            description: 'para cerrar la caja', onExpiry: expireTime });
    });
}
function expireTime() {
    alert("Se ha acabado el tiempo!");
    location = location.href;
}

