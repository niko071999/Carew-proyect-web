$(document).ready(function () {
    $.get("/Usuario/getSesion", function (data) {
        if (data != null) {
            document.getElementById('text_nameuser').innerText = data.nombreuser;
            document.getElementById('img_user').src = data.imgruta;
            //$("#text_nameuser").val(data.nombreuser);
            //$("#img_user").attr("src", data.imgruta);
            console.log(textuser, imgruta);
        } else {
            console.log('error null');
        }
    });
    $("#linksignout").click(function () {
        const swal = Swal.mixin({
            customClass: {
                confirmButton: 'btn btn-success',
                cancelButton: 'btn btn-danger'
            },
            buttonsStyling: false
        });
        swal.fire({
            title: 'Cerrar sesion',
            text: 'Seguro que quiere cerrar la sesion',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Si',
            cancelButtonText: 'No',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                window.location = '/Sesion/SignOut';
            }
            return;
        })
    });
})