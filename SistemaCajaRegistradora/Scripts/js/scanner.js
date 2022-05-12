$('#codigo_barra').bind('keyup', function (e) {
    var key = e.keyCode || e.which;
    var text = document.getElementById("codigo_barra").value;
    if (key === 13) {
        console.log(text);
    }
});