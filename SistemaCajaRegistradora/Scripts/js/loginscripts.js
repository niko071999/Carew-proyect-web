//let text = '';
//Capturar las teclas
//$(document).keyup(function (e) {
//    text = text + e.key;
//    console.log(text);
//    setTimeout(eliminarText, 1000);
//});
//function eliminarText() {
//    text = '';
//}
verificarSession(localStorage.getItem('user'));

function verificarSession(user) {
    if (user != null) {
        $.ajax({
            url: '/Sesion/verificarSession',
            data: { 'user': user },
            type: "post",
            cache: false,
            success: function (data) {
                console.log(data);
                localStorage.removeItem('user');
            },
            error: function (error) {
                console.log(error);
            }
        });
    }
}

$("#btn_ingresarAdmin").click(function () {
    let nameuser = "ncasanova";
    let clave = 123456;

    $("#InputUser").val(nameuser);
    $("#InputPassword").val(clave);
    $("#form").submit();
});
$("#btn_ingresarCajero").click(function () {
    let nameuser = "jgonzalez";
    let clave = 654321;

    $("#InputUser").val(nameuser);
    $("#InputPassword").val(clave);
    $("#form").submit();
});

$("#InputUser").change(function () {
    localStorage.setItem('user', $(this).val());
});